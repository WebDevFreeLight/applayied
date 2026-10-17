using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using Infrastructure.Exceptions;
using Mapster;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.Configuration;
using ZR.Model.Finance.Configuration.Dto;
using ZR.Model.Finance.Configuration.Enums;
using ZR.Repository.Finance.Configuration;
using ZR.Service.IService.Finance.Configuration;

namespace ZR.Service.Finance.Configuration
{
    /// <summary>
    /// Fiscal Year Service - خدمة السنوات المالية
    /// </summary>
    [AppService(ServiceType = typeof(IFiscalYearService), ServiceLifetime = LifeTime.Transient)]
    public class FiscalYearService : BaseService<FiscalYear>, IFiscalYearService
    {
        private readonly IFiscalYearRepository _fiscalYearRepository;

        public FiscalYearService(IFiscalYearRepository fiscalYearRepository)
        {
            _fiscalYearRepository = fiscalYearRepository;
        }

        /// <summary>
        /// Get paginated list of fiscal years
        /// </summary>
        public PagedInfo<FiscalYearListDto> GetList(FiscalYearQueryDto query, PagerInfo pager)
        {
            var searchModel = new FiscalYear
            {
                
                CompanyID = query.CompanyId
            };

            var result = _fiscalYearRepository.GetList(searchModel, pager);
            var dtoList = result.Result.Select(MapToDto).ToList();

            return new PagedInfo<FiscalYearListDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                Result = dtoList
            };
        }

        /// <summary>
        /// Get fiscal year by ID
        /// </summary>
        public FiscalYearDetailDto GetById(Guid id)
        {
            var fiscalYear = _fiscalYearRepository.GetById(id);
            if (fiscalYear == null) return null;

            return MapToDetailDto(fiscalYear);
        }

        /// <summary>
        /// Create fiscal year
        /// </summary>
        public FiscalYear Create(CreateFiscalYearDto dto, string userName)
        {
            // Validate dates
            if (dto.YearEndDate <= dto.YearStartDate)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_FISCAL_YEAR_INVALID_DATE_RANGE)
                    .WithParameter("StartDate", dto.YearStartDate.ToString("yyyy-MM-dd"))
                    .WithParameter("EndDate", dto.YearEndDate.ToString("yyyy-MM-dd"));
            }

            // Validate overlapping periods
            if (_fiscalYearRepository.HasOverlappingPeriod(dto.CompanyId, dto.YearStartDate, dto.YearEndDate))
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_FISCAL_YEAR_OVERLAP)
                    .WithParameter("StartDate", dto.YearStartDate.ToString("yyyy-MM-dd"))
                    .WithParameter("EndDate", dto.YearEndDate.ToString("yyyy-MM-dd"));
            }

            var model = dto.Adapt<FiscalYear>();
            model.Id = Guid.NewGuid();
            model.Status = FiscalYearStatus.Open;
            model.Create_by = userName;
            model.Create_time = DateTime.Now;

            // Check if there's already an open fiscal year for this company
            var existingOpen = _fiscalYearRepository.GetOpenFiscalYear(dto.CompanyId);
            if (existingOpen != null && existingOpen.CompanyID == dto.CompanyId)
            {
                // Close the existing open year
                existingOpen.Status = FiscalYearStatus.Closed;
                existingOpen.Update_by = userName;
                existingOpen.Update_time = DateTime.Now;
                _fiscalYearRepository.Update(existingOpen, it => new { it.Status, it.Update_by, it.Update_time }, f => f.Id == existingOpen.Id);
            }

            _fiscalYearRepository.Insert(model);
            return model;
        }

        /// <summary>
        /// Update fiscal year
        /// </summary>
        public int Update(UpdateFiscalYearDto dto, string userName)
        {
            var existing = _fiscalYearRepository.GetById(dto.Id);
            if (existing == null)
                throw new AppException(ExceptionCode.FINANCE_CONFIG_FISCAL_YEAR_NOT_FOUND)
                    .WithParameter("FiscalYearId", dto.Id);

            // Validate dates
            if (dto.YearEndDate <= dto.YearStartDate)
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_FISCAL_YEAR_INVALID_DATE_RANGE)
                    .WithParameter("StartDate", dto.YearStartDate.ToString("yyyy-MM-dd"))
                    .WithParameter("EndDate", dto.YearEndDate.ToString("yyyy-MM-dd"));
            }

            // Validate overlapping periods (exclude current)
            if (_fiscalYearRepository.HasOverlappingPeriod(dto.CompanyId, dto.YearStartDate, dto.YearEndDate, dto.Id))
            {
                throw new AppException(ExceptionCode.FINANCE_CONFIG_FISCAL_YEAR_OVERLAP)
                    .WithParameter("StartDate", dto.YearStartDate.ToString("yyyy-MM-dd"))
                    .WithParameter("EndDate", dto.YearEndDate.ToString("yyyy-MM-dd"));
            }

            var model = dto.Adapt<FiscalYear>();
            model.Update_by = userName;
            model.Update_time = DateTime.Now;
            model.Status = existing.Status; // Preserve status

            return Update(model, it => new
            {
                it.CompanyID,
                it.YearStartDate,
                it.YearEndDate,
                it.Currency,
                it.DescriptionAr,
                it.DescriptionEn,
                it.Update_by,
                it.Update_time
            }, f => f.Id == model.Id);
        }

        /// <summary>
        /// Delete fiscal years
        /// </summary>
        public int Delete(Guid[] ids)
        {
            // Check if any fiscal year is open
            foreach (var id in ids)
            {
                var fiscalYear = _fiscalYearRepository.GetById(id);
                if (fiscalYear != null && fiscalYear.Status == FiscalYearStatus.Open)
                {
                    throw new AppException(ExceptionCode.FINANCE_CONFIG_FISCAL_YEAR_CANNOT_DELETE_OPEN)
                        .WithParameter("FiscalYearId", id);
                }
            }

            return Context.Deleteable<FiscalYear>()
                .Where(it => ids.Contains(it.Id))
                .ExecuteCommand();
        }

        /// <summary>
        /// Open fiscal year
        /// </summary>
        public int OpenFiscalYear(Guid id, string userName)
        {
            var fiscalYear = _fiscalYearRepository.GetById(id);
            if (fiscalYear == null)
                throw new AppException(ExceptionCode.FINANCE_CONFIG_FISCAL_YEAR_NOT_FOUND)
                    .WithParameter("FiscalYearId", id);

            // Close any existing open fiscal year for the same company
            var existingOpen = _fiscalYearRepository.GetOpenFiscalYear(fiscalYear.CompanyID);
            if (existingOpen != null && existingOpen.Id != id)
            {
                existingOpen.Status = FiscalYearStatus.Closed;
                existingOpen.Update_by = userName;
                existingOpen.Update_time = DateTime.Now;
                _fiscalYearRepository.Update(existingOpen, it => new { it.Status, it.Update_by, it.Update_time }, f => f.Id == existingOpen.Id);
            }

            // Open the requested fiscal year
            fiscalYear.Status = FiscalYearStatus.Open;
            fiscalYear.Update_by = userName;
            fiscalYear.Update_time = DateTime.Now;

            return Update(fiscalYear, it => new { it.Status, it.Update_by, it.Update_time }, f => f.Id == id);
        }

        /// <summary>
        /// Close fiscal year
        /// </summary>
        public int CloseFiscalYear(Guid id, string userName)
        {
            var fiscalYear = _fiscalYearRepository.GetById(id);
            if (fiscalYear == null)
                throw new AppException(ExceptionCode.FINANCE_CONFIG_FISCAL_YEAR_NOT_FOUND)
                    .WithParameter("FiscalYearId", id);

            fiscalYear.Status = FiscalYearStatus.Closed;
            fiscalYear.Update_by = userName;
            fiscalYear.Update_time = DateTime.Now;

            return Update(fiscalYear, it => new { it.Status, it.Update_by, it.Update_time }, f => f.Id == id);
        }

        /// <summary>
        /// Validate overlapping periods
        /// </summary>
        public bool ValidateOverlappingPeriods(long? companyId, DateTime startDate, DateTime endDate, Guid? excludeId = null)
        {
            return !_fiscalYearRepository.HasOverlappingPeriod(companyId, startDate, endDate, excludeId);
        }

        private FiscalYearListDto MapToDto(FiscalYear fiscalYear)
        {
            return fiscalYear.Adapt<FiscalYearListDto>();
        }

        private FiscalYearDetailDto MapToDetailDto(FiscalYear fiscalYear)
        {
            return fiscalYear.Adapt<FiscalYearDetailDto>();
        }

        /// <summary>
        /// Get fiscal year containing a specific date
        /// </summary>
        public FiscalYear GetFiscalYearByDate(long? companyId, DateTime date)
        {
            return _fiscalYearRepository.GetFiscalYearByDate(companyId, date);
        }

        /// <summary>
        /// Get all fiscal years for a company ordered by start date descending
        /// </summary>
        public List<FiscalYear> GetAllFiscalYears(long? companyId)
        {
            return _fiscalYearRepository.GetAllFiscalYears(companyId);
        }

        /// <summary>
        /// Get earliest fiscal year for a company
        /// </summary>
        public FiscalYear GetEarliestFiscalYear(long? companyId)
        {
            return _fiscalYearRepository.GetEarliestFiscalYear(companyId);
        }
    }
}


