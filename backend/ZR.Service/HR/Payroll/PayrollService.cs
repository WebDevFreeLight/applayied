using Infrastructure.Attribute;
using System.Linq.Expressions;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.ARAP.Enums;
using ZR.Model.Finance.Configuration;
using ZR.Model.HR.Finance;
using ZR.Model.HR.Finance.Enums;
using ZR.Model.HR.Dto;
using ZR.Model.HR.Dto.EmployeeCore;
using ZR.Model.HR.Payroll;
using ZR.Model.System;
using ZR.Repository;
using ZR.Service.HR.IService;
using ZR.ServiceCore.Security;
using ZR.Model.HR.Attendance.LeaveManagement;
using ZR.Model.HR.EmployeeCore;

namespace ZR.Service.HR
{
    /// <summary>
    /// Payroll Service Implementation
    /// </summary>
    //public   class PayrollService : BaseService<HrPayroll>

    [AppService(ServiceType = typeof(IPayrollService), ServiceLifetime = LifeTime.Transient)]

    public class PayrollService : BaseService<HrPayroll>, IPayrollService
    {
        private readonly ISensitiveDataProtector _sensitive;
        private readonly IEmployeeDataService _employeeDataService;
        private readonly IHrStatutoryRateService _statutoryRateService;

        public PayrollService(ISensitiveDataProtector sensitiveDataProtector, IEmployeeDataService employeeDataService, IHrStatutoryRateService statutoryRateService)
        {
            _sensitive = sensitiveDataProtector;
            _employeeDataService = employeeDataService;
            _statutoryRateService = statutoryRateService;
        }

        private async Task<HrPayroll> GetLatestPayrollSnapshotAsync(long employeeId)
        {
            return await Context.Queryable<HrPayroll>()
                .Where(p => p.EmployeeId == employeeId)
                .OrderBy(p => p.PayPeriod, OrderByType.Desc)
                .FirstAsync();
        }









        // IPayrollService interface implementations

        public async Task<PagedInfo<HrPayroll>> GetPageListAsync(Expression<Func<HrPayroll, bool>> predicate, PayrollQueryDto pager)
        {
            return await Queryable()
                .Where(predicate)
                .OrderBy(p => p.PayrollId, OrderByType.Desc)
                .ToPageAsync(pager);
        }

        public async Task<HrPayroll> GetByIdAsync(long payrollId)
        {
            return await GetFirstAsync(p => p.PayrollId == payrollId);
        }

        public async Task<HrPayroll> GetEmployeePayrollAsync(long employeeId, string payPeriod)
        {
            return await GetFirstAsync(p => p.EmployeeId == employeeId && p.PayPeriod == payPeriod);
        }

        public async Task<int> CalculatePayrollAsync(long employeeId, string payPeriod)
        {
            var calculation = new PayrollCalculationDto
            {
                EmployeeId = employeeId,
                PayPeriod = payPeriod,
                IncludeOvertime = true,
                ApplyDeductions = true,
                CalculateGOSI = true,
                CalculateTax = true
            };

            var result = await ((IPayrollService)this).CalculatePayrollAsync(calculation);
            return result != null ? 1 : 0;
        }

        async Task<object> IPayrollService.GetPayrollListAsync(PayrollQueryDto query)
        {
            var data = await GetPayrollListAsync(query);
            return data;
        }

        async Task<object> IPayrollService.CalculatePayrollAsync(PayrollCalculationDto calculation)
        {
            var result = await CalculatePayrollAsync(calculation);
            return result;
        }

        async Task<object> IPayrollService.ProcessBulkPayrollAsync(BulkPayrollDto bulk)
        {
            var result = await ProcessBulkPayrollAsync(bulk);
            return result;
        }

        async Task<bool> IPayrollService.ApprovePayrollAsync(long payrollId, string approvedBy)
        {
            return await ApprovePayrollAsync(payrollId, approvedBy);
        }

        async Task<object> IPayrollService.GenerateBankTransferFileAsync(string payPeriod, string bankCode)
        {
            var result = await GenerateBankTransferFileAsync(payPeriod, bankCode);
            return result;
        }

        async Task<object> IPayrollService.GetGOSIComplianceReportAsync(string payPeriod)
        {
            var result = await GetGOSIComplianceReportAsync(payPeriod);
            return result;
        }

        public async Task<int> CalculateBulkPayrollAsync(string payPeriod, long? departmentId = null)
        {
            var bulk = new BulkPayrollDto
            {
                PayPeriod = payPeriod,
                DepartmentId = departmentId,
                AutoApprove = false
            };

            var result = await ((IPayrollService)this).ProcessBulkPayrollAsync(bulk);
            if (result is PayrollSummaryDto summary)
                return summary.ProcessedCount;
            return 0;
        }

        public async Task<int> ProcessPayrollAsync(string payPeriod)
        {
            var payrolls = await Queryable()
                .Where(p => p.PayPeriod == payPeriod && p.Status == "approved")
                .ToListAsync();

            var processed = 0;
            foreach (var payroll in payrolls)
            {
                if (await ProcessPaymentAsync(payroll.PayrollId, "bank_transfer", $"BATCH-{payPeriod}"))
                {
                    processed++;
                }
            }

            return processed;
        }

        public async Task<int> ApprovePayrollAsync(long payrollId, string approvedBy, bool returnInt)
        {
            var result = await ApprovePayrollAsync(payrollId, approvedBy);
            return result ? 1 : 0;
        }

        public async Task<int> RejectPayrollAsync(long payrollId, string reason, string rejectedBy)
        {
            var payroll = await GetByIdAsync(payrollId);
            if (payroll == null) return 0;

            payroll.Status = "rejected";
            payroll.Remark = reason;
            payroll.Update_by = rejectedBy;
            payroll.Update_time = DateTime.Now;

            return await Context.Updateable(payroll)
                .UpdateColumns(p => new { p.Status, p.Remark, p.Update_by, p.Update_time })
                .ExecuteCommandAsync();
        }

        public async Task<PayrollCalculationResult> CalculateSalaryComponentsAsync(long employeeId, string payPeriod)
        {
            var employee = await _employeeDataService.GetEmployeeFullDataAsync(employeeId);

            if (employee == null) return null;

            var snapshot = await GetLatestPayrollSnapshotAsync(employeeId);
            var snapshotBasic = snapshot?.BasicSalary ?? 0m;
            var snapshotHousing = snapshot?.HousingAllowance ?? 0m;
            var snapshotTransport = snapshot?.TransportAllowance ?? 0m;
            var snapshotOther = snapshot?.OtherAllowances ?? 0m;

            var periodParts = payPeriod.Split('-');
            var year = int.Parse(periodParts[0]);
            var month = int.Parse(periodParts[1]);
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var attendanceRecords = await Context.Queryable<ZR.Model.HR.Attendance.AttendanceProcessing.HrAttendanceDay>()
                .Where(a => a.EmployeeId == employeeId
                    && a.AttendanceDate >= startDate
                    && a.AttendanceDate <= endDate)
                .ToListAsync();

            var workingDays = attendanceRecords.Count(a => !a.IsAbsent);
            var totalOvertime = attendanceRecords.Sum(a => a.OvertimeHours);

            var result = new PayrollCalculationResult
            {
                EmployeeId = employeeId,
                PayPeriod = payPeriod,
                BasicSalary = snapshotBasic,
                HousingAllowance = snapshotHousing,
                TransportAllowance = snapshotTransport,
                OtherAllowances = snapshotOther,
                OvertimePay = CalculateOvertimePay(totalOvertime, snapshotBasic / 30 / 8),
                WorkingDays = workingDays,
                ActualDays = attendanceRecords.Count
            };

            result.GrossTotal = result.BasicSalary + result.HousingAllowance +
                               result.TransportAllowance + result.OtherAllowances + result.OvertimePay;

            var gosiResult = CalculateGOSI(employee.Nationality, result.BasicSalary + result.HousingAllowance);
            result.GOSIEmployee = gosiResult.employeeContribution;
            result.GOSIEmployer = gosiResult.employerContribution;

            result.Tax = await CalculateTaxAsync(result.GrossTotal - result.GOSIEmployee);
            result.Deductions = result.GOSIEmployee + result.Tax;
            result.NetSalary = result.GrossTotal - result.Deductions;

            result.DeductionDetails = new List<PayrollDeductionDetail>
            {
                new PayrollDeductionDetail { DeductionType = "GOSI", Amount = result.GOSIEmployee, Description = "Social Insurance" },
                new PayrollDeductionDetail { DeductionType = "Tax", Amount = result.Tax, Description = "Income Tax" }
            };

            result.AllowanceDetails = new List<PayrollAllowanceDetail>
            {
                new PayrollAllowanceDetail { AllowanceType = "Housing", Amount = result.HousingAllowance, Description = "Housing Allowance" },
                new PayrollAllowanceDetail { AllowanceType = "Transport", Amount = result.TransportAllowance, Description = "Transport Allowance" },
                new PayrollAllowanceDetail { AllowanceType = "Other", Amount = result.OtherAllowances, Description = "Other Allowances" }
            };

            return result;
        }

        public async Task<decimal> CalculateGOSIAsync(decimal basicSalary, bool isEmployeeContribution)
        {
            // Default to Saudi calculation
            if (isEmployeeContribution)
            {
                return basicSalary * 0.10m; // 10% for Saudi employees
            }
            else
            {
                return basicSalary * 0.12m; // 12% for employer contribution
            }
        }

        public async Task<decimal> CalculateTaxAsync(decimal taxableIncome)
        {
            // Saudi Arabia has no personal income tax for citizens
            // This is a placeholder for any future tax implementations
            return await Task.FromResult(0m);
        }

        public async Task<object> GetPayrollStatisticsAsync(string payPeriod)
        {
            var payrolls = await Queryable()
                .Where(p => p.PayPeriod == payPeriod)
                .ToListAsync();

            return new
            {
                PayPeriod = payPeriod,
                TotalEmployees = payrolls.Count,
                TotalGross = payrolls.Sum(p => p.BasicSalary + p.HousingAllowance + p.TransportAllowance + p.OtherAllowances),
                TotalNet = payrolls.Sum(p => p.NetSalary),
                TotalGOSI = payrolls.Sum(p => p.GosiEmployee + p.GosiEmployer),
                TotalTax = payrolls.Sum(p => p.Tax),
                StatusBreakdown = new
                {
                    Pending = payrolls.Count(p => p.Status == "pending"),
                    Approved = payrolls.Count(p => p.Status == "approved"),
                    Paid = payrolls.Count(p => p.Status == "paid"),
                    Rejected = payrolls.Count(p => p.Status == "rejected")
                }
            };
        }

        public async Task<object> GetPayrollSummaryAsync(string payPeriod, long? departmentId = null)
        {
            var payrolls = await Queryable()
                .Where(p => p.PayPeriod == payPeriod)
                .ToListAsync();

            if (departmentId.HasValue)
            {
                // Use EmployeeDataService to filter by department
                var deptEmployees = await _employeeDataService.GetEmployeesByDepartmentAsync(departmentId.Value);
                var deptEmployeeIds = deptEmployees.Select(e => e.EmployeeId).ToList();
                payrolls = payrolls.Where(p => deptEmployeeIds.Contains(p.EmployeeId)).ToList();
            }

            return new
            {
                PayPeriod = payPeriod,
                DepartmentId = departmentId,
                TotalEmployees = payrolls.Count,
                TotalGrossSalary = payrolls.Sum(p => p.BasicSalary + p.HousingAllowance + p.TransportAllowance + p.OtherAllowances),
                TotalDeductions = payrolls.Sum(p => p.Deductions + p.GosiEmployee + p.Tax),
                TotalNetSalary = payrolls.Sum(p => p.NetSalary),
                AverageNetSalary = payrolls.Any() ? payrolls.Average(p => p.NetSalary) : 0
            };
        }

        public async Task<PayslipDto> GetPayslipDetailsAsync(long payrollId)
        {
            return await GeneratePayslipAsync(payrollId);
        }

        async Task<byte[]> IPayrollService.GeneratePayslipAsync(long payrollId)
        {
            var payslip = await GeneratePayslipAsync(payrollId);
            if (payslip == null) return null;

            return GeneratePayslipPdf(payslip);
        }

        /// <summary>
        /// Generate payslip PDF using iText7
        /// </summary>
        private byte[] GeneratePayslipPdf(PayslipDto payslip)
        {
            using var ms = new System.IO.MemoryStream();
            using var writer = new iText.Kernel.Pdf.PdfWriter(ms);
            using var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
            var document = new iText.Layout.Document(pdf, iText.Kernel.Geom.PageSize.A4);
            document.SetMargins(36, 36, 36, 36);

            var bold = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            var normal = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            // Header
            document.Add(new iText.Layout.Element.Paragraph("PAYSLIP")
                .SetFont(bold).SetFontSize(18)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

            document.Add(new iText.Layout.Element.Paragraph($"Pay Period: {payslip.PayPeriod}    Payment Date: {payslip.PaymentDate:yyyy-MM-dd}")
                .SetFont(normal).SetFontSize(10)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

            document.Add(new iText.Layout.Element.LineSeparator(new iText.Kernel.Pdf.Canvas.Draw.SolidLine()));

            // Employee Info Table
            var infoTable = new iText.Layout.Element.Table(iText.Layout.Properties.UnitValue.CreatePercentArray(new float[] { 1, 1, 1, 1 }))
                .UseAllAvailableWidth().SetMarginTop(10).SetMarginBottom(10);

            AddInfoCell(infoTable, "Employee:", payslip.EmployeeName, bold, normal);
            AddInfoCell(infoTable, "Code:", payslip.EmployeeCode, bold, normal);
            AddInfoCell(infoTable, "Department:", payslip.Department, bold, normal);
            AddInfoCell(infoTable, "Position:", payslip.Position, bold, normal);
            AddInfoCell(infoTable, "Working Days:", payslip.WorkingDays.ToString(), bold, normal);
            AddInfoCell(infoTable, "Actual Days:", payslip.ActualDays.ToString(), bold, normal);
            AddInfoCell(infoTable, "Leave Days:", payslip.LeaveDays.ToString(), bold, normal);
            AddInfoCell(infoTable, "Late Days:", payslip.LateDays.ToString(), bold, normal);
            document.Add(infoTable);

            // Earnings & Deductions side by side
            var mainTable = new iText.Layout.Element.Table(iText.Layout.Properties.UnitValue.CreatePercentArray(new float[] { 1, 1 }))
                .UseAllAvailableWidth().SetMarginTop(10);

            // Earnings column
            var earningsCell = new iText.Layout.Element.Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPaddingRight(10);
            earningsCell.Add(new iText.Layout.Element.Paragraph("EARNINGS").SetFont(bold).SetFontSize(12));
            var earningsTable = new iText.Layout.Element.Table(iText.Layout.Properties.UnitValue.CreatePercentArray(new float[] { 2, 1 }))
                .UseAllAvailableWidth();
            AddAmountRow(earningsTable, "Basic Salary", payslip.BasicSalary, normal);
            AddAmountRow(earningsTable, "Housing Allowance", payslip.HousingAllowance, normal);
            AddAmountRow(earningsTable, "Transport Allowance", payslip.TransportAllowance, normal);
            AddAmountRow(earningsTable, "Other Allowances", payslip.OtherAllowances, normal);
            AddAmountRow(earningsTable, "Overtime Pay", payslip.OvertimePay, normal);
            AddAmountRow(earningsTable, "Bonus", payslip.Bonus, normal);
            AddAmountRow(earningsTable, "Commission", payslip.Commission, normal);
            AddAmountRow(earningsTable, "Total Earnings", payslip.TotalEarnings, bold);
            earningsCell.Add(earningsTable);
            mainTable.AddCell(earningsCell);

            // Deductions column
            var deductionsCell = new iText.Layout.Element.Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPaddingLeft(10);
            deductionsCell.Add(new iText.Layout.Element.Paragraph("DEDUCTIONS").SetFont(bold).SetFontSize(12));
            var deductionsTable = new iText.Layout.Element.Table(iText.Layout.Properties.UnitValue.CreatePercentArray(new float[] { 2, 1 }))
                .UseAllAvailableWidth();
            AddAmountRow(deductionsTable, "GOSI (Employee)", payslip.GOSIEmployee, normal);
            AddAmountRow(deductionsTable, "Tax", payslip.Tax, normal);
            AddAmountRow(deductionsTable, "Leave Deduction", payslip.LeaveDeduction, normal);
            AddAmountRow(deductionsTable, "Late Deduction", payslip.LateDeduction, normal);
            AddAmountRow(deductionsTable, "Other Deductions", payslip.OtherDeductions, normal);
            AddAmountRow(deductionsTable, "Total Deductions", payslip.TotalDeductions, bold);
            deductionsCell.Add(deductionsTable);
            mainTable.AddCell(deductionsCell);

            document.Add(mainTable);

            // Net Pay
            document.Add(new iText.Layout.Element.LineSeparator(new iText.Kernel.Pdf.Canvas.Draw.SolidLine()).SetMarginTop(10));
            document.Add(new iText.Layout.Element.Paragraph($"NET PAY: SAR {payslip.NetSalary:N2}")
                .SetFont(bold).SetFontSize(16)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT)
                .SetMarginTop(10));

            // Bank Details
            if (!string.IsNullOrEmpty(payslip.BankName))
            {
                document.Add(new iText.Layout.Element.LineSeparator(new iText.Kernel.Pdf.Canvas.Draw.SolidLine()).SetMarginTop(10));
                document.Add(new iText.Layout.Element.Paragraph("BANK DETAILS").SetFont(bold).SetFontSize(10).SetMarginTop(5));
                document.Add(new iText.Layout.Element.Paragraph($"Bank: {payslip.BankName}    Account: {payslip.AccountNumber}    IBAN: {payslip.IBAN}")
                    .SetFont(normal).SetFontSize(9));
            }

            // Footer
            document.Add(new iText.Layout.Element.Paragraph("This is a system-generated payslip.")
                .SetFont(normal).SetFontSize(8)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                .SetMarginTop(20));

            document.Close();
            return ms.ToArray();
        }

        private void AddInfoCell(iText.Layout.Element.Table table, string label, string value, iText.Kernel.Font.PdfFont bold, iText.Kernel.Font.PdfFont normal)
        {
            table.AddCell(new iText.Layout.Element.Cell()
                .Add(new iText.Layout.Element.Paragraph(label).SetFont(bold).SetFontSize(9))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            table.AddCell(new iText.Layout.Element.Cell()
                .Add(new iText.Layout.Element.Paragraph(value ?? "-").SetFont(normal).SetFontSize(9))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
        }

        private void AddAmountRow(iText.Layout.Element.Table table, string label, decimal amount, iText.Kernel.Font.PdfFont font)
        {
            table.AddCell(new iText.Layout.Element.Cell()
                .Add(new iText.Layout.Element.Paragraph(label).SetFont(font).SetFontSize(9))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            table.AddCell(new iText.Layout.Element.Cell()
                .Add(new iText.Layout.Element.Paragraph($"SAR {amount:N2}").SetFont(font).SetFontSize(9))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
        }

        public async Task<int> GenerateBankFileAsync(string payPeriod)
        {
            var transfer = await ((IPayrollService)this).GenerateBankTransferFileAsync(payPeriod, "DEFAULT");

            // Save to file or database
            // This is a placeholder implementation
            if (transfer is BankTransferDto bankTransfer && bankTransfer.Entries.Any())
            {
                // Generate actual bank file format (could be CSV, XML, etc.)
                return bankTransfer.Entries.Count;
            }

            return 0;
        }

        public async Task<object> GetComplianceReportAsync(string payPeriod)
        {
            var gosiReport = await ((IPayrollService)this).GetGOSIComplianceReportAsync(payPeriod);

            return new
            {
                PayPeriod = payPeriod,
                GOSICompliance = gosiReport,
                TaxCompliance = new
                {
                    TotalTaxableIncome = 0m,
                    TotalTaxDeducted = 0m,
                    ComplianceStatus = "Compliant"
                },
                LaborLawCompliance = new
                {
                    MinimumWageCompliance = true,
                    OvertimeCompliance = true,
                    LeaveCompliance = true
                }
            };
        }

        public async Task<List<PayrollValidationError>> ValidatePayrollAsync(string payPeriod)
        {
            var errors = new List<PayrollValidationError>();

            var payrolls = await Queryable()
                .Where(p => p.PayPeriod == payPeriod)
                .ToListAsync();

            // Batch load employee data
            var employeeIds = payrolls.Select(p => p.EmployeeId).ToList();
            var employees = await _employeeDataService.GetEmployeesFullDataAsync(employeeIds);
            var employeeDict = employees.ToDictionary(e => e.EmployeeId);

            foreach (var payroll in payrolls)
            {
                var employee = employeeDict.GetValueOrDefault(payroll.EmployeeId);
                if (employee == null) continue;

                // Validate minimum wage
                if (payroll.BasicSalary < 3000) // Example minimum wage
                {
                    errors.Add(new PayrollValidationError
                    {
                        EmployeeId = employee.EmployeeId,
                        EmployeeName = $"{employee.FirstName} {employee.LastName}",
                        ErrorType = "MinimumWage",
                        ErrorMessage = "Salary below minimum wage",
                        Severity = "Error"
                    });
                }

                // Validate GOSI calculation
                var expectedGosi = CalculateGOSI(employee.Nationality,
                    payroll.BasicSalary + payroll.HousingAllowance);

                if (Math.Abs(payroll.GosiEmployee - expectedGosi.employeeContribution) > 0.01m)
                {
                    errors.Add(new PayrollValidationError
                    {
                        EmployeeId = employee.EmployeeId,
                        EmployeeName = $"{employee.FirstName} {employee.LastName}",
                        ErrorType = "GOSICalculation",
                        ErrorMessage = "GOSI calculation mismatch",
                        Severity = "Warning"
                    });
                }

                // Validate net salary
                var expectedNet = payroll.BasicSalary + payroll.HousingAllowance +
                                 payroll.TransportAllowance + payroll.OtherAllowances -
                                 payroll.Deductions - payroll.GosiEmployee - payroll.Tax;

                if (Math.Abs(payroll.NetSalary - expectedNet) > 0.01m)
                {
                    errors.Add(new PayrollValidationError
                    {
                        EmployeeId = employee.EmployeeId,
                        EmployeeName = $"{employee.FirstName} {employee.LastName}",
                        ErrorType = "NetSalaryCalculation",
                        ErrorMessage = "Net salary calculation mismatch",
                        Severity = "Error"
                    });
                }
            }

            return errors;
        }

        public async Task<int> RecalculatePayrollAsync(long payrollId)
        {
            var payroll = await GetByIdAsync(payrollId);
            if (payroll == null || payroll.Status == "paid")
                return 0;

            return await CalculatePayrollAsync(payroll.EmployeeId, payroll.PayPeriod);
        }

        public async Task<object> GetYearlyPayrollSummaryAsync(long employeeId, int year)
        {
            var payrolls = await Queryable()
                .Where(p => p.EmployeeId == employeeId && p.PayPeriod.StartsWith(year.ToString()))
                .OrderBy(p => p.PayPeriod)
                .ToListAsync();

            var monthlyBreakdown = payrolls.Select(p => new
            {
                Month = p.PayPeriod,
                GrossSalary = p.BasicSalary + p.HousingAllowance + p.TransportAllowance + p.OtherAllowances,
                Deductions = p.Deductions + p.GosiEmployee + p.Tax,
                NetSalary = p.NetSalary,
                Status = p.Status
            }).ToList();

            return new
            {
                EmployeeId = employeeId,
                Year = year,
                TotalGross = payrolls.Sum(p => p.BasicSalary + p.HousingAllowance + p.TransportAllowance + p.OtherAllowances),
                TotalDeductions = payrolls.Sum(p => p.Deductions + p.GosiEmployee + p.Tax),
                TotalNet = payrolls.Sum(p => p.NetSalary),
                TotalGOSI = payrolls.Sum(p => p.GosiEmployee + p.GosiEmployer),
                TotalTax = payrolls.Sum(p => p.Tax),
                MonthsProcessed = payrolls.Count,
                MonthlyBreakdown = monthlyBreakdown
            };
        }
    


        /// <summary>
        /// Get payroll records with filtering
        /// </summary>
        public async Task<PagedInfo<PayrollDto>> GetPayrollListAsync(PayrollQueryDto query)
        {
            var predicate = Expressionable.Create<HrPayroll>();
            
            if (query.EmployeeId.HasValue)
                predicate = predicate.And(p => p.EmployeeId == query.EmployeeId.Value);
            
            if (!string.IsNullOrEmpty(query.PayPeriod))
                predicate = predicate.And(p => p.PayPeriod == query.PayPeriod);
            
            if (!string.IsNullOrEmpty(query.Status))
                predicate = predicate.And(p => p.Status == query.Status);
            
            if (query.DepartmentId.HasValue)
            {
                var now = DateTime.Now;
                predicate = predicate.And(p => SqlFunc.Subqueryable<HrEmployeeAssignment>()
                    .Where(a => a.EmployeeId == p.EmployeeId
                        && a.DepartmentId == query.DepartmentId.Value
                        && a.EffectiveFrom <= now
                        && (a.EffectiveTo == null || a.EffectiveTo > now)
                        && a.IsPrimary)
                    .Any());
            }

            
            if (query.PaymentDateFrom.HasValue)
                predicate = predicate.And(p => p.PaymentDate >= query.PaymentDateFrom.Value);
            
            if (query.PaymentDateTo.HasValue)
                predicate = predicate.And(p => p.PaymentDate <= query.PaymentDateTo.Value);

            var list = await Context.Queryable<HrPayroll>()
                .Where(predicate.ToExpression())
                .OrderBy(p => p.PayrollId, OrderByType.Desc)
                .ToPageAsync(query );

            var result = new PagedInfo<PayrollDto>
            {
                TotalNum = list.TotalNum,
                PageIndex = list.PageIndex,
                PageSize = list.PageSize,
                Result = list.Result.Select(MapToDto).ToList()
            };

            return result;
        }

        /// <summary>
        /// Calculate payroll for an employee
        /// </summary>
        public async Task<PayrollDto> CalculatePayrollAsync(PayrollCalculationDto calculation)
        {
            // Get employee details
            var employee = await _employeeDataService.GetEmployeeFullDataAsync(calculation.EmployeeId);

            if (employee == null)
                throw new Exception("Employee not found");

            // Check if payroll already exists for this period
            var existingPayroll = await Context.Queryable<HrPayroll>()
                .FirstAsync(p => p.EmployeeId == calculation.EmployeeId && p.PayPeriod == calculation.PayPeriod);
            
            if (existingPayroll != null && existingPayroll.Status == "approved")
                throw new Exception("Approved payroll already exists for this period");

            // Parse pay period (format: YYYY-MM)
            var periodParts = calculation.PayPeriod.Split('-');
            var year = int.Parse(periodParts[0]);
            var month = int.Parse(periodParts[1]);
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var totalDays = endDate.Day;

            // Get attendance summary for the period
            var attendanceRecords = await Context.Queryable<ZR.Model.HR.Attendance.AttendanceProcessing.HrAttendanceDay>()
                .Where(a => a.EmployeeId == calculation.EmployeeId
                    && a.AttendanceDate >= startDate
                    && a.AttendanceDate <= endDate)
                .ToListAsync();

            var workingDays = attendanceRecords.Count(a => !a.IsAbsent);
            var actualDays = attendanceRecords.Count;
            var totalOvertime = attendanceRecords.Sum(a => a.OvertimeHours);
            var totalLateMinutes = attendanceRecords.Sum(a => a.LateMinutes);

            // Get leave records for deduction calculation
            var leaveRecords = await Context.Queryable<HrLeaveRequest>()
                .Where(l => l.EmployeeId == calculation.EmployeeId
                    && l.Status == "approved"
                    && l.StartDate <= endDate
                    && l.EndDate >= startDate)
                .ToListAsync();

            var unpaidLeaveDays = 0m;
            foreach (var leave in leaveRecords)
            {
                var leaveType = await Context.Queryable<ZR.Model.HR.Attendance.LeaveManagement.HrLeaveType>()
                    .FirstAsync(lt => lt.LeaveTypeId == leave.LeaveTypeId);

                // Unpaid leave should be deducted from salary
                if (leaveType != null && !leaveType.IsPaid)
                {
                    unpaidLeaveDays += leave.Days ?? 0;
                }
            }

            // Calculate salary components
            var snapshot = await GetLatestPayrollSnapshotAsync(calculation.EmployeeId);
            // TODO: Get salary from HrEmployeeCompensationPackage instead of snapshot
            // For now, use snapshot (requires employee to have previous payroll or manual setup)
            var basicSalary = snapshot?.BasicSalary ?? 0m;
            var housingAllowance = snapshot?.HousingAllowance ?? 0m;
            var transportAllowance = snapshot?.TransportAllowance ?? 0m;
            var otherAllowances = snapshot?.OtherAllowances ?? 0m;
            
            // Calculate daily rate
            var dailyRate =( basicSalary / totalDays) ;
            
            // Calculate deductions
            var leaveDeduction = unpaidLeaveDays * dailyRate;
            var lateDeduction = CalculateLateDeduction(totalLateMinutes, dailyRate / 8); // Assuming 8 hour work day
            
            // Calculate overtime pay
            var overtimePay = calculation.IncludeOvertime ? 
                CalculateOvertimePay(totalOvertime, dailyRate / 8) : 0;
            
            // Get bonus and commission (if any)
            var bonus = await GetBonusForPeriod(calculation.EmployeeId, calculation.PayPeriod);
            var commission = await GetCommissionForPeriod(calculation.EmployeeId, calculation.PayPeriod);
            
            // Calculate gross salary
            var grossSalary = basicSalary + housingAllowance + transportAllowance + 
                             otherAllowances + overtimePay + bonus + commission;
            
            // Apply deductions
            var deductions = calculation.ApplyDeductions ? 
                (leaveDeduction + lateDeduction) : 0;

            // Loan deduction (salary deduction repayments for the same pay period)
            var loanDeduction = 0m;
            if (calculation.ApplyDeductions)
            {
                loanDeduction = await Context.Queryable<HrLoanRepayment>()
                    .Where(r => r.EmployeeId == calculation.EmployeeId
                                && r.PayPeriod == calculation.PayPeriod
                                && r.Status == "paid"
                                && r.PaymentMethod == "salary_deduction")
                    .SumAsync(r => r.RepaymentAmount);
            }
            
            // Calculate GOSI
            decimal gosiEmployee = 0, gosiEmployer = 0;
            if (calculation.CalculateGOSI)
            {
                (gosiEmployee, gosiEmployer) = CalculateGOSI(employee.Nationality, basicSalary + housingAllowance);
            }
            
            // Calculate tax
            decimal tax = 0;
            if (calculation.CalculateTax)
            {
                tax = CalculateTax(grossSalary - gosiEmployee);
            }
            
            // Calculate net salary
            var totalDeductions = deductions + loanDeduction + gosiEmployee + tax;
            var netSalary = grossSalary - totalDeductions;

            // Create or update payroll record
            var payroll = existingPayroll ?? new HrPayroll();
            
            payroll.EmployeeId = calculation.EmployeeId;
            payroll.PayPeriod = calculation.PayPeriod;
            payroll.BasicSalary = basicSalary;
            payroll.HousingAllowance = housingAllowance;
            payroll.TransportAllowance = transportAllowance;
            payroll.OtherAllowances = otherAllowances;
            payroll.OvertimePay = overtimePay;
            payroll.Bonus = bonus;
            payroll.Commission = commission;
            payroll.Deductions = deductions;
            payroll.GosiEmployee = gosiEmployee;
            payroll.GosiEmployer = gosiEmployer;
            payroll.Tax = tax;
            payroll.NetSalary = netSalary;
            payroll.WorkingDays = workingDays;
            payroll.ActualDays = actualDays;
            payroll.LeaveDeduction = leaveDeduction;
            payroll.LateDeduction = lateDeduction;
            payroll.Status = "pending";
            payroll.Create_time = DateTime.Now;
            payroll.Create_by = "System";

            if (existingPayroll != null)
            {
                payroll.Update_time = DateTime.Now;
                payroll.Update_by = "System";
                await Context.Updateable(payroll).ExecuteCommandAsync();
            }
            else
            {
                await Context.Insertable(payroll).ExecuteCommandAsync();
            }

            return MapToDto(payroll);
        }

        /// <summary>
        /// Process bulk payroll
        /// </summary>
        public async Task<PayrollSummaryDto> ProcessBulkPayrollAsync(BulkPayrollDto bulk)
        {
            var employeeIds = bulk.EmployeeIds ?? new List<long>();
            
            // If no specific employees, get all from department or all active
            if (!employeeIds.Any())
            {
                List<EmployeeFullDto> employees;
                if (bulk.DepartmentId.HasValue)
                {
                    employees = await _employeeDataService.GetEmployeesByDepartmentAsync(bulk.DepartmentId.Value);
                }
                else
                {
                    employees = await _employeeDataService.GetAllActiveEmployeesAsync();
                }

                employeeIds = employees.Select(e => e.EmployeeId).ToList();
            }

            var summary = new PayrollSummaryDto
            {
                PayPeriod = bulk.PayPeriod,
                TotalEmployees = employeeIds.Count,
                ProcessedCount = 0,
                PendingCount = 0,
                PaidCount = 0,
                TotalGrossSalary = 0,
                TotalDeductions = 0,
                TotalNetSalary = 0,
                TotalGOSI = 0,
                TotalTax = 0,
                DepartmentSummaries = new List<DepartmentPayrollSummary>()
            };

            // Process each employee
            foreach (var employeeId in employeeIds)
            {
                try
                {
                    var calculation = new PayrollCalculationDto
                    {
                        EmployeeId = employeeId,
                        PayPeriod = bulk.PayPeriod,
                        IncludeOvertime = true,
                        ApplyDeductions = true,
                        CalculateGOSI = true,
                        CalculateTax = true
                    };

                    var payroll = await CalculatePayrollAsync(calculation);
                    
                    if (bulk.AutoApprove)
                    {
                        await ApprovePayrollAsync(payroll.PayrollId, "System");
                        summary.PaidCount++;
                    }
                    else
                    {
                        summary.PendingCount++;
                    }

                    summary.ProcessedCount++;
                    summary.TotalGrossSalary += payroll.BasicSalary + payroll.HousingAllowance + 
                                               payroll.TransportAllowance + payroll.OtherAllowances + 
                                               payroll.OvertimePay + payroll.Bonus + payroll.Commission;
                    summary.TotalDeductions += payroll.Deductions + payroll.GosiEmployee + payroll.Tax;
                    summary.TotalNetSalary += payroll.NetSalary;
                    summary.TotalGOSI += payroll.GosiEmployee + payroll.GosiEmployer;
                    summary.TotalTax += payroll.Tax;
                }
                catch (Exception ex)
                {
                    // Log error but continue processing
                    Console.WriteLine($"Error processing payroll for employee {employeeId}: {ex.Message}");
                }
            }

            // Get department summaries
            if (bulk.DepartmentId.HasValue)
            {
                var deptSummary = await GetDepartmentPayrollSummary(bulk.PayPeriod, bulk.DepartmentId.Value);
                summary.DepartmentSummaries.Add(deptSummary);
            }
            else
            {
                var departments = await Context.Queryable<SysDept>()
                    .Where(d => d.Status == 1)
                    .ToListAsync();
                
                foreach (var dept in departments)
                {
                    var deptSummary = await GetDepartmentPayrollSummary(bulk.PayPeriod, dept.DeptId);
                    if (deptSummary.EmployeeCount > 0)
                        summary.DepartmentSummaries.Add(deptSummary);
                }
            }

            return summary;
        }

        /// <summary>
        /// Approve payroll
        /// </summary>
        public async Task<bool> ApprovePayrollAsync(long payrollId, string approvedBy)
        {
            var payroll = await Context.Queryable<HrPayroll>()
                .FirstAsync(p => p.PayrollId == payrollId);
            
            if (payroll == null)
                return false;
            
            if (payroll.Status == "approved")
                return true;

            payroll.Status = "approved";
            payroll.Update_by = approvedBy;
            payroll.Update_time = DateTime.Now;
            
            return await Context.Updateable(payroll)
                .UpdateColumns(p => new { p.Status, p.Update_by, p.Update_time })
                .ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Process payment
        /// </summary>
        public async Task<bool> ProcessPaymentAsync(long payrollId, string paymentMethod, string bankReference)
        {
            var payroll = await Context.Queryable<HrPayroll>()
                .FirstAsync(p => p.PayrollId == payrollId);
            
            if (payroll == null || payroll.Status != "approved")
                return false;

            payroll.Status = "paid";
            payroll.PaymentDate = DateTime.Now;
            payroll.PaymentMethod = paymentMethod;
            payroll.BankReference = bankReference;
            payroll.Update_time = DateTime.Now;
            
            return await Context.Updateable(payroll)
                .UpdateColumns(p => new { p.Status, p.PaymentDate, p.PaymentMethod, p.BankReference, p.Update_time })
                .ExecuteCommandAsync() > 0;
        }

        /// <summary>
        /// Generate payslip
        /// </summary>
        public async Task<PayslipDto> GeneratePayslipAsync(long payrollId)
        {
            var payroll = await Context.Queryable<HrPayroll>()
                .FirstAsync(p => p.PayrollId == payrollId);
            
            if (payroll == null)
                return null;

            var employeeData = await _employeeDataService.GetEmployeeFullDataAsync(payroll.EmployeeId);

            if (employeeData == null)
                return null;

            // Get department and position names
            string departmentName = "";
            string positionTitle = "";

            if (employeeData.DepartmentId.HasValue)
            {
                var dept = await Context.Queryable<SysDept>()
                    .FirstAsync(d => d.DeptId == employeeData.DepartmentId.Value);
                departmentName = dept?.DeptName ?? "";
            }

            if (employeeData.PositionId.HasValue)
            {
                var position = await Context.Queryable<SysPost>()
                    .FirstAsync(p => p.PostId == employeeData.PositionId.Value);
                positionTitle = position?.PostName ?? "";
            }

            var employee = new
            {
                employeeData.EmployeeId,
                employeeData.EmployeeCode,
                employeeData.FirstName,
                employeeData.LastName,
                BankName = employeeData.BankName,
                AccountNumber = employeeData.BankAccountNumber,
                IBAN = employeeData.IBAN,
                AccountNumberEncrypted = _sensitive.Protect(employeeData.BankAccountNumber),
                IBANEncrypted = _sensitive.Protect(employeeData.IBAN),
                DepartmentName = departmentName,
                PositionTitle = positionTitle
            };

            // Calculate leave and late days
            var periodParts = payroll.PayPeriod.Split('-');
            var year = int.Parse(periodParts[0]);
            var month = int.Parse(periodParts[1]);
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var attendanceStats = await Context.Queryable<ZR.Model.HR.Attendance.AttendanceProcessing.HrAttendanceDay>()
                .Where(a => a.EmployeeId == payroll.EmployeeId
                    && a.AttendanceDate >= startDate
                    && a.AttendanceDate <= endDate)
                .GroupBy(a => a.EmployeeId)
                .Select(g => new
                {
                    LeaveDays = SqlFunc.AggregateCount(g.Status == "leave" ? 1 : 0),
                    LateDays = SqlFunc.AggregateCount(g.LateMinutes > 0 ? 1 : 0)
                })
                .FirstAsync();

            var payslip = new PayslipDto
            {
                PayrollId = payroll.PayrollId,
                EmployeeCode = employee.EmployeeCode,
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                Department = employee.DepartmentName,
                Position = employee.PositionTitle,
                PayPeriod = payroll.PayPeriod,
                PaymentDate = payroll.PaymentDate ?? DateTime.Now,
                
                // Earnings
                BasicSalary = payroll.BasicSalary,
                HousingAllowance = payroll.HousingAllowance,
                TransportAllowance = payroll.TransportAllowance,
                OtherAllowances = payroll.OtherAllowances,
                OvertimePay = payroll.OvertimePay,
                Bonus = payroll.Bonus,
                Commission = payroll.Commission,
                TotalEarnings = payroll.BasicSalary + payroll.HousingAllowance + 
                               payroll.TransportAllowance + payroll.OtherAllowances + 
                               payroll.OvertimePay + payroll.Bonus + payroll.Commission,
                
                // Deductions
                GOSIEmployee = payroll.GosiEmployee,
                Tax = payroll.Tax,
                LeaveDeduction = payroll.LeaveDeduction,
                LateDeduction = payroll.LateDeduction,
                OtherDeductions = payroll.Deductions - payroll.LeaveDeduction - payroll.LateDeduction,
                TotalDeductions = payroll.Deductions + payroll.GosiEmployee + payroll.Tax,
                
                // Summary
                NetSalary = payroll.NetSalary,
                WorkingDays = payroll.WorkingDays,
                ActualDays = payroll.ActualDays,
                LeaveDays = attendanceStats?.LeaveDays ?? 0,
                LateDays = attendanceStats?.LateDays ?? 0,
                
                // Bank Details
                BankName = employee.BankName,
                AccountNumber = _sensitive.MaskAccountNumber(employee.AccountNumber ?? _sensitive.Unprotect(employee.AccountNumberEncrypted)),
                IBAN = _sensitive.MaskIban(employee.IBAN ?? _sensitive.Unprotect(employee.IBANEncrypted))
            };

            return payslip;
        }

        /// <summary>
        /// Get GOSI compliance report
        /// </summary>
        public async Task<GOSIComplianceDto> GetGOSIComplianceReportAsync(string payPeriod)
        {
            // Get payroll data
            var payrolls = await Context.Queryable<HrPayroll>()
                .Where(p => p.PayPeriod == payPeriod && p.Status != "cancelled")
                .Select(p => new
                {
                    p.PayrollId,
                    p.EmployeeId,
                    p.BasicSalary,
                    p.HousingAllowance,
                    p.GosiEmployee,
                    p.GosiEmployer
                })
                .ToListAsync();

            // Get employee data using EmployeeDataService
            var employeeIds = payrolls.Select(p => p.EmployeeId).ToList();
            var employees = await _employeeDataService.GetEmployeesFullDataAsync(employeeIds);
            var employeeDict = employees.ToDictionary(e => e.EmployeeId);

            var report = new GOSIComplianceDto
            {
                PayPeriod = payPeriod,
                TotalEmployees = payrolls.Count,
                SaudiEmployees = employees.Count(e => e.IsSaudi),
                NonSaudiEmployees = employees.Count(e => !e.IsSaudi),
                TotalWages = payrolls.Sum(p => p.BasicSalary + p.HousingAllowance),
                TotalEmployeeContribution = payrolls.Sum(p => p.GosiEmployee),
                TotalEmployerContribution = payrolls.Sum(p => p.GosiEmployer),
                TotalGOSI = payrolls.Sum(p => p.GosiEmployee + p.GosiEmployer),
                EmployeeDetails = payrolls.Select(p =>
                {
                    var emp = employeeDict.GetValueOrDefault(p.EmployeeId);
                    return new EmployeeGOSIDetail
                    {
                        EmployeeId = p.EmployeeId,
                        EmployeeCode = emp?.EmployeeCode ?? "",
                        EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}" : "",
                        GOSINumber = emp?.GosiNumber ?? "",
                        Nationality = emp?.IsSaudi == true ? "Saudi" : "Non-Saudi",
                        BasicSalary = p.BasicSalary + p.HousingAllowance,
                        EmployeeContribution = p.GosiEmployee,
                        EmployerContribution = p.GosiEmployer,
                        Total = p.GosiEmployee + p.GosiEmployer
                    };
                }).ToList()
            };

            return report;
        }

        /// <summary>
        /// Generate bank transfer file
        /// </summary>
        public async Task<BankTransferDto> GenerateBankTransferFileAsync(string payPeriod, string bankCode)
        {
            // Get payroll data
            var payrolls = await Context.Queryable<HrPayroll>()
                .Where(p => p.PayPeriod == payPeriod && p.Status == "approved")
                .Select(p => new
                {
                    p.PayrollId,
                    p.EmployeeId,
                    p.NetSalary
                })
                .ToListAsync();

            // Get employee data using EmployeeDataService
            var employeeIds = payrolls.Select(p => p.EmployeeId).ToList();
            var employees = await _employeeDataService.GetEmployeesFullDataAsync(employeeIds);
            var employeeDict = employees.ToDictionary(e => e.EmployeeId);

            var transfer = new BankTransferDto
            {
                PayPeriod = payPeriod,
                BankCode = bankCode,
                ValueDate = DateTime.Now,
                TotalAmount = payrolls.Sum(p => p.NetSalary),
                TotalCount = payrolls.Count,
                Entries = payrolls.Select(p =>
                {
                    var emp = employeeDict.GetValueOrDefault(p.EmployeeId);
                    var iban = "";
                    if (emp != null && !string.IsNullOrEmpty(emp.IbanEncrypted))
                    {
                        iban = _sensitive.Unprotect(emp.IbanEncrypted);
                    }
                    return new BankTransferEntry
                    {
                        EmployeeCode = emp?.EmployeeCode ?? "",
                        EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}" : "",
                        IBAN = iban,
                        Amount = p.NetSalary,
                        Reference = $"SAL-{payPeriod}-{emp?.EmployeeCode}",
                        Description = $"Salary for {payPeriod}"
                    };
                }).ToList()
            };

            return transfer;
        }

        #region Helper Methods

        private PayrollDto MapToDto(HrPayroll payroll)
        {
            return new PayrollDto
            {
                PayrollId = payroll.PayrollId,
                EmployeeId = payroll.EmployeeId,
                PayPeriod = payroll.PayPeriod,
                BasicSalary = payroll.BasicSalary,
                HousingAllowance = payroll.HousingAllowance,
                TransportAllowance = payroll.TransportAllowance,
                OtherAllowances = payroll.OtherAllowances,
                OvertimePay = payroll.OvertimePay,
                Deductions = payroll.Deductions,
                GosiEmployee = payroll.GosiEmployee,
                GosiEmployer = payroll.GosiEmployer,
                Tax = payroll.Tax,
                NetSalary = payroll.NetSalary,
                PaymentDate = payroll.PaymentDate,
                PaymentMethod = payroll.PaymentMethod,
                BankReference = payroll.BankReference,
                Status = payroll.Status,
                WorkingDays = payroll.WorkingDays,
                ActualDays = payroll.ActualDays,
                LeaveDeduction = payroll.LeaveDeduction,
                LateDeduction = payroll.LateDeduction,
                Bonus = payroll.Bonus,
                Commission = payroll.Commission,
                Remarks = payroll.Remark
            };
        }

        private decimal CalculateLateDeduction(int totalLateMinutes, decimal hourlyRate)
        {
            // Deduct based on late minutes
            return (totalLateMinutes / 60m) * hourlyRate;
        }

        private decimal CalculateOvertimePay(decimal overtimeHours, decimal hourlyRate)
        {
            // Overtime at 1.5x normal rate
            return overtimeHours * hourlyRate * 1.5m;
        }

        private async Task<decimal> GetBonusForPeriod(long employeeId, string payPeriod)
        {
            // Implement bonus calculation logic
            // This could be based on performance reviews, targets, etc.
            return 0;
        }

        private async Task<decimal> GetCommissionForPeriod(long employeeId, string payPeriod)
        {
            // Implement commission calculation logic
            // This could be based on sales, targets, etc.
            return 0;
        }

        private (decimal employeeContribution, decimal employerContribution) CalculateGOSI(string nationality, decimal wage)
        {
            var isSaudi = nationality == "Saudi";
            var (employeeRate, employerRate) = _statutoryRateService.GetCurrentGOSIRatesAsync(isSaudi).GetAwaiter().GetResult();
            return (wage * employeeRate, wage * employerRate);
        }

        private decimal CalculateTax(decimal taxableIncome)
        {
            // Implement tax calculation based on Saudi tax laws
            // Currently, Saudi Arabia has no personal income tax for citizens
            // But may have Zakat (2.5%) for Saudi nationals
            // Non-Saudis may be subject to different rules
            return 0;
        }

        private async Task<DepartmentPayrollSummary> GetDepartmentPayrollSummary(string payPeriod, long departmentId)
        {
            var dept = await Context.Queryable<SysDept>()
                .FirstAsync(d => d.DeptId == departmentId);

            // Get employees in this department
            var deptEmployees = await _employeeDataService.GetEmployeesByDepartmentAsync(departmentId);
            var deptEmployeeIds = deptEmployees.Select(e => e.EmployeeId).ToList();

            // Get payrolls for these employees
            var payrolls = await Context.Queryable<HrPayroll>()
                .Where(p => p.PayPeriod == payPeriod && deptEmployeeIds.Contains(p.EmployeeId))
                .ToListAsync();

            return new DepartmentPayrollSummary
            {
                DepartmentId = departmentId,
                DepartmentName = dept?.DeptName ?? "",
                EmployeeCount = payrolls.Count,
                TotalGross = payrolls.Sum(p => p.BasicSalary + p.HousingAllowance +
                                              p.TransportAllowance + p.OtherAllowances +
                                              p.OvertimePay + p.Bonus + p.Commission),
                TotalNet = payrolls.Sum(p => p.NetSalary)
            };
        }

        #region WPS Bank File

        /// <summary>
        /// Generate Saudi WPS (Wages Protection System) bank file in SIF format
        /// </summary>
        public async Task<byte[]> GenerateWPSFileAsync(string payPeriod, string bankCode)
        {
            var transferData = await GenerateBankTransferFileAsync(payPeriod, bankCode);
            if (transferData == null || !transferData.Entries.Any())
                return Array.Empty<byte>();

            var sb = new System.Text.StringBuilder();

            // SIF Header record
            // Format: @HDR@,EMPLOYER_MOL_ID,BANK_CODE,PAY_MONTH,TOTAL_SALARIES,TOTAL_RECORDS,VALUE_DATE
            var molId = "MOL_ID"; // TODO: Read from company settings
            var valueDate = DateTime.Now.ToString("yyyy-MM-dd");
            sb.AppendLine($"@HDR@,{molId},{bankCode},{payPeriod},{transferData.TotalAmount:F2},{transferData.TotalCount},{valueDate}");

            // SIF Detail records
            // Format: EMPLOYEE_MOL_ID,EMPLOYEE_BANK_CODE,IBAN,SALARY_AMOUNT,ALLOWANCES
            var sequence = 1;
            foreach (var entry in transferData.Entries)
            {
                // Extract bank code from IBAN (positions 5-6 for Saudi IBANs)
                var employeeBankCode = !string.IsNullOrEmpty(entry.IBAN) && entry.IBAN.Length >= 6
                    ? entry.IBAN.Substring(4, 2)
                    : bankCode;

                sb.AppendLine(string.Join(",",
                    entry.EmployeeCode,        // Employee ID
                    employeeBankCode,          // Employee bank short code
                    entry.IBAN,                // IBAN
                    entry.Amount.ToString("F2"), // Salary amount
                    "0.00",                    // Allowances (already included in amount)
                    entry.Reference            // Reference
                ));
                sequence++;
            }

            // SIF Footer
            sb.AppendLine($"@SIF@,{transferData.TotalCount},{transferData.TotalAmount:F2}");

            return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        }

        #endregion

        #region End of Service Benefit

        /// <summary>
        /// Calculate End of Service Benefit per Saudi Labor Law
        /// First 5 years: (BasicSalary / 2) × years
        /// After 5 years: BasicSalary × additional years
        /// Resignation: less than 2yrs = 0, 2-5yrs = 1/3, 5-10yrs = 2/3, 10+ = full
        /// Termination: always full amount
        /// </summary>
        public async Task<ESBResultDto> CalculateESBAsync(long employeeId, string terminationType)
        {
            var employee = await _employeeDataService.GetEmployeeFullDataAsync(employeeId);
            if (employee == null)
                throw new Exception("Employee not found");

            if (!employee.HireDate.HasValue)
                throw new Exception("Employee hire date is not set");

            var calculationDate = DateTime.Now;
            var totalDays = (calculationDate - employee.HireDate.Value).TotalDays;
            var totalYears = (decimal)(totalDays / 365.25);

            // Get current basic salary from latest payroll or compensation
            var snapshot = await GetLatestPayrollSnapshotAsync(employeeId);
            var basicSalary = snapshot?.BasicSalary ?? 0m;

            if (basicSalary <= 0)
                throw new Exception("Employee has no salary record");

            // Calculate full ESB amount
            decimal first5YearsAmount = 0m;
            decimal after5YearsAmount = 0m;

            if (totalYears <= 5)
            {
                // First 5 years: half month salary per year
                first5YearsAmount = (basicSalary / 2m) * totalYears;
            }
            else
            {
                // First 5 years: half month salary per year (capped at 5)
                first5YearsAmount = (basicSalary / 2m) * 5m;
                // After 5 years: full month salary per additional year
                after5YearsAmount = basicSalary * (totalYears - 5m);
            }

            var fullESBAmount = first5YearsAmount + after5YearsAmount;

            // Apply eligibility factor based on termination type
            decimal eligibilityFactor = 1m;
            string eligibilityNote = "";
            var termType = (terminationType ?? "termination").ToLower();

            if (termType == "resignation")
            {
                if (totalYears < 2)
                {
                    eligibilityFactor = 0m;
                    eligibilityNote = "Resignation before 2 years of service: not eligible";
                }
                else if (totalYears < 5)
                {
                    eligibilityFactor = 1m / 3m;
                    eligibilityNote = "Resignation between 2-5 years: eligible for 1/3 of ESB";
                }
                else if (totalYears < 10)
                {
                    eligibilityFactor = 2m / 3m;
                    eligibilityNote = "Resignation between 5-10 years: eligible for 2/3 of ESB";
                }
                else
                {
                    eligibilityFactor = 1m;
                    eligibilityNote = "Resignation after 10+ years: eligible for full ESB";
                }
            }
            else
            {
                eligibilityFactor = 1m;
                eligibilityNote = "Termination by employer: eligible for full ESB";
            }

            var finalAmount = Math.Round(fullESBAmount * eligibilityFactor, 2);

            return new ESBResultDto
            {
                EmployeeId = employeeId,
                EmployeeCode = employee.EmployeeCode,
                EmployeeName = $"{employee.FirstName} {employee.LastName}",
                HireDate = employee.HireDate,
                CalculationDate = calculationDate,
                TerminationType = terminationType,
                BasicSalary = basicSalary,
                TotalServiceYears = Math.Round(totalYears, 2),
                First5YearsAmount = Math.Round(first5YearsAmount, 2),
                After5YearsAmount = Math.Round(after5YearsAmount, 2),
                FullESBAmount = Math.Round(fullESBAmount, 2),
                EligibilityFactor = Math.Round(eligibilityFactor, 4),
                FinalESBAmount = finalAmount,
                EligibilityNote = eligibilityNote
            };
        }

        #endregion

        #region HR-Finance Integration

        public async Task<long> GeneratePayroll(long employeeId)
        {
            var result = await Context.Ado.UseTranAsync(async () =>
            {
                var payPeriod = DateTime.Now.ToString("yyyy-MM");

                // Ensure payroll exists for this pay period
                await CalculatePayrollAsync(employeeId, payPeriod);

                var payroll = await GetEmployeePayrollAsync(employeeId, payPeriod);
                if (payroll == null)
                    throw new Exception($"Payroll generation failed for employee {employeeId} period {payPeriod}");

                if (payroll.APInvoiceId.HasValue)
                    return payroll.PayrollId;

                // Base currency
                var baseCurrency = await Context.Queryable<Currency>()
                    .FirstAsync(c => c.IsBaseCurrency && c.IsActive);

                payroll.CurrencyId = baseCurrency?.Id;
                payroll.ExchangeRate = 1.0m;

                // Ensure Payroll Clearing vendor exists
                var vendor = await Context.Queryable<Vendor>()
                    .FirstAsync(v => v.VendorCode == "PAYROLL_CLEARING");

                if (vendor == null)
                {
                    vendor = new Vendor
                    {
                        Id = Guid.NewGuid(),
                        VendorCode = "PAYROLL_CLEARING",
                        NameEn = "Payroll Clearing",
                        NameAr = "تسوية الرواتب",
                        IsActive = true,
                        Create_time = DateTime.Now
                    };
                    await Context.Insertable(vendor).ExecuteCommandAsync();
                }

                // Create AP invoice
                var apInvoice = new APInvoice
                {
                    Id = Guid.NewGuid(),
                    VendorId = vendor.Id,
                    InvoiceNumber = $"HRPAY-{payroll.PayrollId}",
                    InvoiceDate = DateTime.Now,
                    DueDate = DateTime.Now,
                    Currency = baseCurrency?.Code ?? "SAR",
                    ExchangeRate = payroll.ExchangeRate,
                    Subtotal = payroll.NetSalary,
                    TaxAmount = 0,
                    TotalAmount = payroll.NetSalary,
                    PaidAmount = 0,
                    RemainingAmount = payroll.NetSalary,
                    Status = InvoiceStatus.Draft,
                    IsTaxInclusive = false,
                    Description = $"HR Payroll {payroll.PayPeriod} Employee {payroll.EmployeeId}",
                    ReferenceNumber = $"HR:{payroll.PayrollId}"
                };

                await Context.Insertable(apInvoice).ExecuteCommandAsync();

                // Link payroll -> finance
                payroll.APInvoiceId = apInvoice.Id;
                payroll.Update_time = DateTime.Now;
                await Context.Updateable(payroll)
                    .UpdateColumns(p => new { p.APInvoiceId, p.CurrencyId, p.ExchangeRate, p.Update_time })
                    .Where(p => p.PayrollId == payroll.PayrollId)
                    .ExecuteCommandAsync();

                // Insert reconciliation entry
                var recon = new HrFinanceReconciliation
                {
                    ReconciliationId = Guid.NewGuid(),
                    PayrollId = payroll.PayrollId,
                    APInvoiceId = apInvoice.Id,
                    ReconciledAmount = apInvoice.TotalAmount,
                    Status = HrFinanceReconciliationStatus.Pending,
                    AIInsight = null,
                    Create_time = DateTime.Now
                };
                await Context.Insertable(recon).ExecuteCommandAsync();

                // Audit log
                var audit = new HrFinancialAuditLog
                {
                    LogId = Guid.NewGuid(),
                    Action = "GeneratePayroll",
                    UserId = 0,
                    Timestamp = DateTime.Now,
                    Changes = $"{{\"employeeId\":{employeeId},\"payPeriod\":\"{payPeriod}\",\"payrollId\":{payroll.PayrollId},\"apInvoiceId\":\"{apInvoice.Id}\"}}",
                    Create_time = DateTime.Now
                };
                await Context.Insertable(audit).ExecuteCommandAsync();

                return payroll.PayrollId;
            });

            if (!result.IsSuccess)
                throw result.ErrorException;

            return (long)result.Data;
        }

        #endregion

        #endregion
    }
}
