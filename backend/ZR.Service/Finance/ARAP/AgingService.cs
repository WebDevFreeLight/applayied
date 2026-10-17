using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.ARAP.Dto;
using ZR.Model.Finance.ARAP.Enums;
using ZR.Model.Customers;
using ZR.Model.Customers.Dto;
using ZR.Repository.Finance.ARAP;
using ZR.Repository.Customers;
using ZR.Service.IService.Finance.ARAP;
using ZR.Service.IService.Customers;

namespace ZR.Service.Finance.ARAP
{
    /// <summary>
    /// Aging Service - خدمة التقادم
    /// </summary>
    [AppService(ServiceType = typeof(IAgingService), ServiceLifetime = LifeTime.Transient)]
    public class AgingService : IAgingService
    {
        private readonly IARInvoiceRepository _arInvoiceRepository;
        private readonly IAPInvoiceRepository _apInvoiceRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IVendorRepository _vendorRepository;

        public AgingService(
            IARInvoiceRepository arInvoiceRepository,
            IAPInvoiceRepository apInvoiceRepository,
            ICustomerRepository customerRepository,
            IVendorRepository vendorRepository)
        {
            _arInvoiceRepository = arInvoiceRepository;
            _apInvoiceRepository = apInvoiceRepository;
            _customerRepository = customerRepository;
            _vendorRepository = vendorRepository;
        }

        /// <summary>
        /// Get customer aging report
        /// </summary>
        public List<AgingReportDto> GetCustomerAgingReport(AgingQueryDto query)
        {
            var asOfDate = query.AsOfDate ?? DateTime.Now;
            var invoices = new List<ARInvoice>();

            if (query.CustomerId.HasValue)
            {
                invoices = _arInvoiceRepository.GetByCustomerId(query.CustomerId.Value);
            }
            else
            {
                var searchModel = new ARInvoice { CompanyID = query.CompanyId };
                var pager = new ZR.Model.PagerInfo { PageSize = 10000 };
                var result = _arInvoiceRepository.GetList(searchModel, pager);
                invoices = result.Result.ToList();
            }

            var report = invoices
                .Where(x => x.RemainingAmount > 0)
                .Select(invoice => CalculateAging(invoice, asOfDate, true))
                .ToList();

            return report;
        }

        /// <summary>
        /// Get vendor aging report
        /// </summary>
        public List<AgingReportDto> GetVendorAgingReport(AgingQueryDto query)
        {
            var asOfDate = query.AsOfDate ?? DateTime.Now;
            var invoices = new List<APInvoice>();

            if (query.VendorId.HasValue)
            {
                invoices = _apInvoiceRepository.GetByVendorId(query.VendorId.Value);
            }
            else
            {
                var searchModel = new APInvoice { CompanyID = query.CompanyId };
                var pager = new ZR.Model.PagerInfo { PageSize = 10000 };
                var result = _apInvoiceRepository.GetList(searchModel, pager);
                invoices = result.Result.ToList();
            }

            var report = invoices
                .Where(x => x.RemainingAmount > 0)
                .Select(invoice => CalculateAging(invoice, asOfDate, false))
                .ToList();

            return report;
        }

        /// <summary>
        /// Get customer aging summary
        /// </summary>
        public List<AgingSummaryDto> GetCustomerAgingSummary(AgingQueryDto query)
        {
            var report = GetCustomerAgingReport(query);

            var summary = report
                .GroupBy(x => x.CustomerVendorId)
                .Select(g => new AgingSummaryDto
                {
                    CustomerVendorId = g.Key,
                    CustomerVendorName = g.First().CustomerVendorName,
                    TotalOutstanding = g.Sum(x => x.RemainingAmount),
                    Age0To30 = g.Sum(x => x.Age0To30),
                    Age31To60 = g.Sum(x => x.Age31To60),
                    Age61To90 = g.Sum(x => x.Age61To90),
                    Age90Plus = g.Sum(x => x.Age90Plus),
                    Invoices = g.ToList()
                })
                .ToList();

            return summary;
        }

        /// <summary>
        /// Get vendor aging summary
        /// </summary>
        public List<AgingSummaryDto> GetVendorAgingSummary(AgingQueryDto query)
        {
            var report = GetVendorAgingReport(query);

            var summary = report
                .GroupBy(x => x.CustomerVendorId)
                .Select(g => new AgingSummaryDto
                {
                    CustomerVendorId = g.Key,
                    CustomerVendorName = g.First().CustomerVendorName,
                    TotalOutstanding = g.Sum(x => x.RemainingAmount),
                    Age0To30 = g.Sum(x => x.Age0To30),
                    Age31To60 = g.Sum(x => x.Age31To60),
                    Age61To90 = g.Sum(x => x.Age61To90),
                    Age90Plus = g.Sum(x => x.Age90Plus),
                    Invoices = g.ToList()
                })
                .ToList();

            return summary;
        }

        private AgingReportDto CalculateAging(ARInvoice invoice, DateTime asOfDate, bool isCustomer)
        {
            var daysOverdue = (asOfDate - invoice.DueDate).Days;
            var remainingAmount = invoice.RemainingAmount;

            var dto = new AgingReportDto
            {
                CustomerVendorId = isCustomer ? invoice.CustomerId : Guid.Empty,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                TotalAmount = invoice.TotalAmount,
                RemainingAmount = remainingAmount,
                DaysOverdue = Math.Max(0, daysOverdue),
                Status = invoice.Status
            };

            if (isCustomer)
            {
                var customer = _customerRepository.GetById(invoice.CustomerId);
                dto.CustomerVendorName = customer != null ? customer.NameEn : string.Empty;
            }

            // Calculate aging buckets
            if (daysOverdue <= 30)
            {
                dto.Age0To30 = remainingAmount;
            }
            else if (daysOverdue <= 60)
            {
                dto.Age31To60 = remainingAmount;
            }
            else if (daysOverdue <= 90)
            {
                dto.Age61To90 = remainingAmount;
            }
            else
            {
                dto.Age90Plus = remainingAmount;
            }

            return dto;
        }

        private AgingReportDto CalculateAging(APInvoice invoice, DateTime asOfDate, bool isVendor)
        {
            var daysOverdue = (asOfDate - invoice.DueDate).Days;
            var remainingAmount = invoice.RemainingAmount;

            var dto = new AgingReportDto
            {
                CustomerVendorId = isVendor ? invoice.VendorId : Guid.Empty,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                TotalAmount = invoice.TotalAmount,
                RemainingAmount = remainingAmount,
                DaysOverdue = Math.Max(0, daysOverdue),
                Status = invoice.Status
            };

            if (isVendor)
            {
                var vendor = _vendorRepository.GetById(invoice.VendorId);
                dto.CustomerVendorName = vendor != null ? vendor.NameEn : string.Empty;
            }

            // Calculate aging buckets
            if (daysOverdue <= 30)
            {
                dto.Age0To30 = remainingAmount;
            }
            else if (daysOverdue <= 60)
            {
                dto.Age31To60 = remainingAmount;
            }
            else if (daysOverdue <= 90)
            {
                dto.Age61To90 = remainingAmount;
            }
            else
            {
                dto.Age90Plus = remainingAmount;
            }

            return dto;
        }
    }
}


