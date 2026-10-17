using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Mapster;
using SqlSugar;
using ZR.Common;
using ZR.Model;
using ZR.Model.Finance.Budget;
using ZR.Model.Finance.Budget.Dto;
using ZR.Model.Finance.GL;
using ZR.Repository.Finance.Budget;
using ZR.Repository.Finance.GL;
using ZR.Service.IService.Finance.Budget;
using ZR.Service.IService.Finance.GL;

namespace ZR.Service.Finance.Budget
{
    /// <summary>
    /// Budget Service - خدمة الميزانية
    /// </summary>
    [AppService(ServiceType = typeof(IBudgetService), ServiceLifetime = LifeTime.Transient)]
    public class BudgetService : BaseService<BudgetHeader>, IBudgetService
    {
        private readonly IBudgetHeaderRepository _headerRepository;
        private readonly IBudgetLineRepository _lineRepository;
        private readonly IBudgetVarianceSummaryRepository _varianceRepository;
        private readonly IGLAccountRepository _glAccountRepository;
        private readonly IGLJournalEntryRepository _glEntryRepository;
        private readonly INormalBalanceResolver _normalBalanceResolver;

        public BudgetService(
            IBudgetHeaderRepository headerRepository,
            IBudgetLineRepository lineRepository,
            IBudgetVarianceSummaryRepository varianceRepository,
            IGLAccountRepository glAccountRepository,
            IGLJournalEntryRepository glEntryRepository,
            INormalBalanceResolver normalBalanceResolver)
        {
            _headerRepository = headerRepository;
            _lineRepository = lineRepository;
            _varianceRepository = varianceRepository;
            _glAccountRepository = glAccountRepository;
            _glEntryRepository = glEntryRepository;
            _normalBalanceResolver = normalBalanceResolver;
        }

        #region Budget Header Operations

        /// <summary>
        /// Get paginated list of budgets
        /// </summary>
        public PagedInfo<BudgetHeaderListDto> GetList(BudgetHeaderQueryDto query, PagerInfo pager)
        {
            var searchModel = new BudgetHeader
            {
                BudgetCode = query.BudgetCode,
                Name = query.Name,
                FiscalYear = query.FiscalYear ?? 0,
                Type = query.Type,
                Status = query.Status,
                ProjectId = query.ProjectId,
                DepartmentId = query.DepartmentId,
                CostCenterId = query.CostCenterId
            };

            var result = _headerRepository.GetList(searchModel, pager);

            var dtoList = result.Result.Select(x => MapToListDto(x)).ToList();

            return new PagedInfo<BudgetHeaderListDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                Result = dtoList
            };
        }

        /// <summary>
        /// Get budget by ID with lines
        /// </summary>
        public BudgetHeaderDetailDto GetById(Guid budgetId)
        {
            var header = _headerRepository.GetById(budgetId);
            if (header == null) return null;

            var dto = MapToDetailDto(header);

            // Get lines
            var lines = _lineRepository.GetByHeaderId(budgetId);
            dto.Lines = lines.Select(MapToBudgetLineDto).ToList();

            // Get variance summaries
            var variances = _varianceRepository.GetByHeaderId(budgetId);
            dto.VarianceSummaries = variances.Select(MapToVarianceSummaryDto).ToList();

            return dto;
        }

        /// <summary>
        /// Create budget with lines
        /// </summary>
        public BudgetHeader Create(CreateBudgetHeaderDto dto, string userName)
        {
            // Check if code exists (within same company)
            if (_headerRepository.CodeExists(dto.BudgetCode, dto.CompanyID))
            {
                throw new AppException(ExceptionCode.FINANCE_BUDGET_DUPLICATE_CODE, new Dictionary<string, object> { { "BudgetCode", dto.BudgetCode } });
            }

            var header = dto.Adapt<BudgetHeader>();
            header.Id = Guid.NewGuid();
            header.Status = "Draft";
            header.CreatedAt = DateTime.Now;
            header.Create_by = userName;
            header.Create_time = DateTime.Now;

            // Insert header
            _headerRepository.Insert(header);

            // Insert lines
            if (dto.Lines != null && dto.Lines.Count > 0)
            {
                foreach (var lineDto in dto.Lines)
                {
                    var line = new BudgetLine
                    {
                        Id = Guid.NewGuid(),
                        BudgetHeaderId = header.Id,
                        GLAccountId = lineDto.GLAccountId,
                        Period = lineDto.Period,
                        BudgetedAmount = lineDto.BudgetedAmount,
                        RevisedAmount = lineDto.RevisedAmount,
                        ActualAmount = 0,
                        Create_by = userName,
                        Create_time = DateTime.Now
                    };
                    _lineRepository.Insert(line);
                }
            }

            return header;
        }

        /// <summary>
        /// Update budget with lines
        /// </summary>
        public int Update(UpdateBudgetHeaderDto dto, string userName)
        {
            // Check if code exists (excluding current, within same company)
            if (_headerRepository.CodeExists(dto.BudgetCode, dto.CompanyID, dto.Id))
            {
                throw new AppException(ExceptionCode.FINANCE_BUDGET_DUPLICATE_CODE, new Dictionary<string, object> { { "BudgetCode", dto.BudgetCode } });
            }

            var existing = _headerRepository.GetById(dto.Id);
            if (existing == null)
            {
                throw new AppException(ExceptionCode.FINANCE_BUDGET_NOT_FOUND);
            }

            // Check if draft - only draft budgets can be modified
            if (existing.Status != "Draft")
            {
                throw new AppException(ExceptionCode.FINANCE_BUDGET_CANNOT_MODIFY);
            }

            existing.BudgetCode = dto.BudgetCode;
            existing.Name = dto.Name;
            existing.FiscalYear = dto.FiscalYear;
            existing.Currency = dto.Currency;
            existing.Type = dto.Type;
            existing.ProjectId = dto.ProjectId;
            existing.DepartmentId = dto.DepartmentId;
            existing.CostCenterId = dto.CostCenterId;
            existing.IsActive = dto.IsActive;
            existing.Update_by = userName;
            existing.Update_time = DateTime.Now;

            var result = _headerRepository.Update(existing);

            // Update lines - delete old and insert new
            if (dto.Lines != null)
            {
                _lineRepository.DeleteByHeaderId(dto.Id);

                foreach (var lineDto in dto.Lines)
                {
                    var line = new BudgetLine
                    {
                        Id = lineDto.Id ?? Guid.NewGuid(),
                        BudgetHeaderId = dto.Id,
                        GLAccountId = lineDto.GLAccountId,
                        Period = lineDto.Period,
                        BudgetedAmount = lineDto.BudgetedAmount,
                        RevisedAmount = lineDto.RevisedAmount,
                        ActualAmount = 0,
                        Create_by = userName,
                        Create_time = DateTime.Now
                    };
                    _lineRepository.Insert(line);
                }
            }

            return result;
        }

        /// <summary>
        /// Delete budgets
        /// </summary>
        public int Delete(Guid[] ids)
        {
            var count = 0;
            foreach (var id in ids)
            {
                var header = _headerRepository.GetById(id);
                if (header != null)
                {
                    // Check if draft - only draft budgets can be deleted
                    if (header.Status != "Draft")
                    {
                        throw new AppException(ExceptionCode.FINANCE_BUDGET_CANNOT_MODIFY, new Dictionary<string, object> { { "BudgetCode", header.BudgetCode } });
                    }

                    // Delete variance summaries
                    _varianceRepository.DeleteByHeaderId(id);

                    // Delete lines
                    _lineRepository.DeleteByHeaderId(id);

                    // Delete header
                    count += _headerRepository.Delete(header);
                }
            }
            return count;
        }

        /// <summary>
        /// Approve budget
        /// </summary>
        public int ApproveBudget(Guid budgetId, Guid approvedBy)
        {
            var header = _headerRepository.GetById(budgetId);
            if (header == null)
            {
                throw new AppException(ExceptionCode.FINANCE_BUDGET_NOT_FOUND);
            }

            if (header.Status != "Draft")
            {
                throw new AppException(ExceptionCode.FINANCE_BUDGET_CANNOT_MODIFY);
            }

            // Check if budget has lines
            var lines = _lineRepository.GetByHeaderId(budgetId);
            if (lines == null || lines.Count == 0)
            {
                throw new AppException(ExceptionCode.VALIDATION_FAILED);
            }

            return _headerRepository.ApproveBudget(budgetId, approvedBy);
        }

        /// <summary>
        /// Close budget
        /// </summary>
        public int CloseBudget(Guid budgetId, Guid closedBy)
        {
            var header = _headerRepository.GetById(budgetId);
            if (header == null)
            {
                throw new AppException(ExceptionCode.FINANCE_BUDGET_NOT_FOUND);
            }

            if (header.Status != "Approved")
            {
                throw new AppException(ExceptionCode.FINANCE_BUDGET_CANNOT_MODIFY);
            }

            return _headerRepository.CloseBudget(budgetId, closedBy);
        }

        /// <summary>
        /// Get budgets by status
        /// </summary>
        public List<BudgetSelectItemDto> GetByStatus(string status)
        {
            var budgets = _headerRepository.GetByStatus(status);
            return budgets.Select(x => new BudgetSelectItemDto
            {
                Id = x.Id,
                BudgetCode = x.BudgetCode,
                Name = x.Name,
                FiscalYear = x.FiscalYear,
                Type = x.Type,
                Status = x.Status,
                IsActive = x.IsActive
            }).ToList();
        }

        /// <summary>
        /// Get active budgets for dropdown
        /// </summary>
        public List<BudgetSelectItemDto> GetActiveBudgets()
        {
            var budgets = _headerRepository.GetActiveBudgets();
            return budgets.Select(x => new BudgetSelectItemDto
            {
                Id = x.Id,
                BudgetCode = x.BudgetCode,
                Name = x.Name,
                FiscalYear = x.FiscalYear,
                Type = x.Type,
                Status = x.Status,
                IsActive = x.IsActive
            }).ToList();
        }

        /// <summary>
        /// Get budgets by fiscal year
        /// </summary>
        public List<BudgetSelectItemDto> GetByFiscalYear(int fiscalYear)
        {
            var budgets = _headerRepository.GetByFiscalYear(fiscalYear);
            return budgets.Select(x => new BudgetSelectItemDto
            {
                Id = x.Id,
                BudgetCode = x.BudgetCode,
                Name = x.Name,
                FiscalYear = x.FiscalYear,
                Type = x.Type,
                Status = x.Status,
                IsActive = x.IsActive
            }).ToList();
        }

        /// <summary>
        /// Get budgets by cost center
        /// </summary>
        public List<BudgetSelectItemDto> GetByCostCenter(long costCenterId)
        {
            var budgets = _headerRepository.GetByCostCenter(costCenterId);
            return budgets.Select(x => new BudgetSelectItemDto
            {
                Id = x.Id,
                BudgetCode = x.BudgetCode,
                Name = x.Name,
                FiscalYear = x.FiscalYear,
                Type = x.Type,
                Status = x.Status,
                IsActive = x.IsActive
            }).ToList();
        }

        /// <summary>
        /// Get budgets by project
        /// </summary>
        public List<BudgetSelectItemDto> GetByProject(Guid projectId)
        {
            var budgets = _headerRepository.GetByProject(projectId);
            return budgets.Select(x => new BudgetSelectItemDto
            {
                Id = x.Id,
                BudgetCode = x.BudgetCode,
                Name = x.Name,
                FiscalYear = x.FiscalYear,
                Type = x.Type,
                Status = x.Status,
                IsActive = x.IsActive
            }).ToList();
        }

        /// <summary>
        /// Get budgets by department
        /// </summary>
        public List<BudgetSelectItemDto> GetByDepartment(long departmentId)
        {
            var budgets = _headerRepository.GetByDepartment(departmentId);
            return budgets.Select(x => new BudgetSelectItemDto
            {
                Id = x.Id,
                BudgetCode = x.BudgetCode,
                Name = x.Name,
                FiscalYear = x.FiscalYear,
                Type = x.Type,
                Status = x.Status,
                IsActive = x.IsActive
            }).ToList();
        }

        /// <summary>
        /// Check if budget code exists (with company isolation)
        /// </summary>
        public bool CodeExists(string code, long? companyId = null, Guid? excludeId = null)
        {
            return _headerRepository.CodeExists(code, companyId, excludeId);
        }

        /// <summary>
        /// Get next budget code (with company isolation)
        /// </summary>
        public string GetNextCode(string prefix, long? companyId = null)
        {
            return _headerRepository.GetNextCode(prefix, companyId);
        }

        #endregion

        #region Budget Line Operations

        /// <summary>
        /// Get budget lines by header ID
        /// </summary>
        public List<BudgetLineDto> GetBudgetLines(Guid budgetHeaderId)
        {
            var lines = _lineRepository.GetByHeaderId(budgetHeaderId);
            return lines.Select(MapToBudgetLineDto).ToList();
        }

        /// <summary>
        /// Add budget line
        /// </summary>
        public BudgetLine AddBudgetLine(Guid budgetHeaderId, CreateBudgetLineDto dto, string userName)
        {
            var header = _headerRepository.GetById(budgetHeaderId);
            if (header == null)
            {
                throw new AppException(ExceptionCode.FINANCE_BUDGET_NOT_FOUND);
            }

            if (header.Status != "Draft")
            {
                throw new AppException(ExceptionCode.FINANCE_BUDGET_CANNOT_MODIFY);
            }

            // Check if line already exists for this account/period
            var existing = _lineRepository.GetByHeaderAccountPeriod(budgetHeaderId, dto.GLAccountId, dto.Period);
            if (existing != null)
            {
                throw new AppException(ExceptionCode.VALIDATION_FAILED);
            }

            var line = new BudgetLine
            {
                Id = Guid.NewGuid(),
                BudgetHeaderId = budgetHeaderId,
                GLAccountId = dto.GLAccountId,
                Period = dto.Period,
                BudgetedAmount = dto.BudgetedAmount,
                RevisedAmount = dto.RevisedAmount,
                ActualAmount = 0,
                Create_by = userName,
                Create_time = DateTime.Now
            };

            _lineRepository.Insert(line);
            return line;
        }

        /// <summary>
        /// Update budget line
        /// </summary>
        public int UpdateBudgetLine(UpdateBudgetLineDto dto, string userName)
        {
            if (!dto.Id.HasValue)
            {
                throw new AppException(ExceptionCode.INVALID_PARAMETER, new Dictionary<string, object> { { "ParameterName", "LineId" } });
            }

            var line = _lineRepository.GetById(dto.Id.Value);
            if (line == null)
            {
                throw new AppException(ExceptionCode.NOT_FOUND);
            }

            var header = _headerRepository.GetById(line.BudgetHeaderId);
            if (header != null && header.Status != "Draft")
            {
                throw new AppException(ExceptionCode.FINANCE_BUDGET_CANNOT_MODIFY);
            }

            line.GLAccountId = dto.GLAccountId;
            line.Period = dto.Period;
            line.BudgetedAmount = dto.BudgetedAmount;
            line.RevisedAmount = dto.RevisedAmount;
            line.Update_by = userName;
            line.Update_time = DateTime.Now;

            return _lineRepository.Update(line);
        }

        /// <summary>
        /// Delete budget line
        /// </summary>
        public int DeleteBudgetLine(Guid lineId)
        {
            var line = _lineRepository.GetById(lineId);
            if (line == null)
            {
                throw new AppException(ExceptionCode.NOT_FOUND);
            }

            var header = _headerRepository.GetById(line.BudgetHeaderId);
            if (header != null && header.Status != "Draft")
            {
                throw new AppException(ExceptionCode.FINANCE_BUDGET_CANNOT_MODIFY);
            }

            return _lineRepository.Delete(line);
        }

        #endregion

        #region Actuals and Variance Operations

        /// <summary>
        /// Refresh actuals from GL
        /// </summary>
        public int RefreshActualsFromGL(Guid budgetId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var header = _headerRepository.GetById(budgetId);
            if (header == null)
            {
                throw new AppException(ExceptionCode.FINANCE_BUDGET_NOT_FOUND);
            }

            var lines = _lineRepository.GetByHeaderId(budgetId);
            if (lines == null || lines.Count == 0)
            {
                return 0;
            }

            // Set default date range based on fiscal year
            var startDate = fromDate ?? new DateTime(header.FiscalYear, 1, 1);
            var endDate = toDate ?? new DateTime(header.FiscalYear, 12, 31);

            var updateCount = 0;

            // Group lines by GL Account and sum actuals for each period
            var accountIds = lines.Select(l => l.GLAccountId).Distinct().ToList();

            foreach (var accountId in accountIds)
            {
                var accountLines = lines.Where(l => l.GLAccountId == accountId).ToList();

                // Get the GL Account to access its normalBalance
                var glAccount = _glAccountRepository.GetById(accountId);
                if (glAccount == null) continue;

                foreach (var line in accountLines)
                {
                    // Calculate period dates
                    var periodStart = new DateTime(header.FiscalYear, line.Period, 1);
                    var periodEnd = periodStart.AddMonths(1).AddDays(-1);

                    // Get actual balance from GL
                    var balance = _glEntryRepository.GetAccountBalance(accountId, periodStart, periodEnd);

                    // Use normalBalance-aware calculation for correct balance direction
                    // This ensures positive values for expenses (Debit-normal) and revenues (Credit-normal)
                    var actualAmount = _normalBalanceResolver.CalculateBalance(
                        balance.TotalDebit,
                        balance.TotalCredit,
                        glAccount.NormalBalance);

                    // Update line actual amount
                    updateCount += _lineRepository.UpdateActualAmount(line.Id, actualAmount);
                }
            }

            // Recalculate variance summaries
            RecalculateVarianceSummaries(budgetId);

            return updateCount;
        }

        /// <summary>
        /// Recalculate variance summaries
        /// </summary>
        public int RecalculateVarianceSummaries(Guid budgetId)
        {
            var header = _headerRepository.GetById(budgetId);
            if (header == null)
            {
                throw new AppException(ExceptionCode.FINANCE_BUDGET_NOT_FOUND);
            }

            var lines = _lineRepository.GetByHeaderId(budgetId);
            if (lines == null || lines.Count == 0)
            {
                return 0;
            }

            // Delete existing summaries
            _varianceRepository.DeleteByHeaderId(budgetId);

            // Create new summaries
            var summaries = new List<BudgetVarianceSummary>();

            foreach (var line in lines)
            {
                var summary = new BudgetVarianceSummary
                {
                    Id = Guid.NewGuid(),
                    BudgetHeaderId = budgetId,
                    GLAccountId = line.GLAccountId,
                    Period = line.Period,
                    BudgetAmount = line.BudgetedAmount,
                    ActualAmount = line.ActualAmount,
                    Variance = line.ActualAmount - line.BudgetedAmount,
                    LastRefreshed = DateTime.Now,
                    Create_time = DateTime.Now
                };
                summaries.Add(summary);
            }

            // Insert summaries
            foreach (var summary in summaries)
            {
                _varianceRepository.Insert(summary);
            }

            return summaries.Count;
        }

        /// <summary>
        /// Get variance summaries by budget
        /// </summary>
        public List<BudgetVarianceSummaryDto> GetVarianceSummaries(Guid budgetId)
        {
            var summaries = _varianceRepository.GetByHeaderId(budgetId);
            return summaries.Select(MapToVarianceSummaryDto).ToList();
        }

        /// <summary>
        /// Get variance summaries by period
        /// </summary>
        public List<BudgetVarianceSummaryDto> GetVarianceSummariesByPeriod(Guid budgetId, int period)
        {
            var summaries = _varianceRepository.GetByHeaderAndPeriod(budgetId, period);
            return summaries.Select(MapToVarianceSummaryDto).ToList();
        }

        #endregion

        #region Reporting Operations

        /// <summary>
        /// Get budget vs actual report
        /// </summary>
        public BudgetVsActualReportDto GetBudgetVsActualReport(Guid budgetId)
        {
            var header = _headerRepository.GetById(budgetId);
            if (header == null)
            {
                throw new AppException(ExceptionCode.FINANCE_BUDGET_NOT_FOUND);
            }

            var lines = _lineRepository.GetByHeaderId(budgetId);

            var report = new BudgetVsActualReportDto
            {
                BudgetHeaderId = header.Id,
                BudgetCode = header.BudgetCode,
                BudgetName = header.Name,
                FiscalYear = header.FiscalYear,
                TotalBudgeted = lines.Sum(l => l.BudgetedAmount),
                TotalActual = lines.Sum(l => l.ActualAmount)
            };

            // Group by GL Account
            var groupedByAccount = lines.GroupBy(l => l.GLAccountId);

            foreach (var group in groupedByAccount)
            {
                var glAccount = _glAccountRepository.GetById(group.Key);

                var lineDto = new BudgetVsActualLineDto
                {
                    GLAccountId = group.Key,
                    GLAccountCode = glAccount?.AccountNumber ?? "",
                    GLAccountName = glAccount?.AccountNameEn ?? "",
                    BudgetedAmount = group.Sum(l => l.BudgetedAmount),
                    ActualAmount = group.Sum(l => l.ActualAmount)
                };

                // Add period breakdown
                lineDto.Periods = group.Select(l => new BudgetPeriodBreakdownDto
                {
                    Period = l.Period,
                    BudgetedAmount = l.BudgetedAmount,
                    ActualAmount = l.ActualAmount
                }).OrderBy(p => p.Period).ToList();

                report.Lines.Add(lineDto);
            }

            return report;
        }

        /// <summary>
        /// Get budget vs actual report for fiscal year
        /// </summary>
        public List<BudgetVsActualReportDto> GetBudgetVsActualReportByFiscalYear(int fiscalYear)
        {
            var budgets = _headerRepository.GetByFiscalYear(fiscalYear);
            return budgets.Select(b => GetBudgetVsActualReport(b.Id)).ToList();
        }

        #endregion

        #region Private Helper Methods

        private BudgetHeaderListDto MapToListDto(BudgetHeader model)
        {
            var dto = model.Adapt<BudgetHeaderListDto>();

            // Calculate totals
            dto.TotalBudgetedAmount = _lineRepository.GetTotalBudgetedByHeader(model.Id);
            dto.TotalActualAmount = _lineRepository.GetTotalActualByHeader(model.Id);

            return dto;
        }

        private BudgetHeaderDetailDto MapToDetailDto(BudgetHeader model)
        {
            var dto = model.Adapt<BudgetHeaderDetailDto>();

            // Calculate totals
            dto.TotalBudgetedAmount = _lineRepository.GetTotalBudgetedByHeader(model.Id);
            dto.TotalActualAmount = _lineRepository.GetTotalActualByHeader(model.Id);

            dto.CreateBy = model.Create_by;
            dto.CreateTime = model.Create_time;
            dto.UpdateBy = model.Update_by;
            dto.UpdateTime = model.Update_time;

            return dto;
        }

        private BudgetLineDto MapToBudgetLineDto(BudgetLine model)
        {
            var dto = model.Adapt<BudgetLineDto>();

            // Get GL Account info
            var glAccount = _glAccountRepository.GetById(model.GLAccountId);
            if (glAccount != null)
            {
                dto.GLAccountCode = glAccount.AccountNumber;
                dto.GLAccountName = glAccount.AccountNameEn;
            }

            return dto;
        }

        private BudgetVarianceSummaryDto MapToVarianceSummaryDto(BudgetVarianceSummary model)
        {
            var dto = model.Adapt<BudgetVarianceSummaryDto>();

            // Get GL Account info
            var glAccount = _glAccountRepository.GetById(model.GLAccountId);
            if (glAccount != null)
            {
                dto.GLAccountCode = glAccount.AccountNumber;
                dto.GLAccountName = glAccount.AccountNameEn;
            }

            return dto;
        }

        #endregion
    }
}
