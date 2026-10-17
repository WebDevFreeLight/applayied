using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using Mapster;
using SqlSugar;
using ZR.Model;
using ZR.Model.Customers;
using ZR.Model.Customers.Dto;
using ZR.Repository.Customers;
using ZR.Repository.Finance.ARAP;
using ZR.Service.IService.Customers;

namespace ZR.Service.Customers
{
    /// <summary>
    /// Customer Service - خدمة العملاء
    /// </summary>
    [AppService(ServiceType = typeof(ICustomerService), ServiceLifetime = LifeTime.Transient)]
    public class CustomerService : BaseService<Customer>, ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IARInvoiceRepository _invoiceRepository;

        public CustomerService(
            ICustomerRepository customerRepository,
            IARInvoiceRepository invoiceRepository)
        {
            _customerRepository = customerRepository;
            _invoiceRepository = invoiceRepository;
        }

        /// <summary>
        /// Get paginated list of customers
        /// </summary>
        public PagedInfo<CustomerListDto> GetList(CustomerQueryDto query, PagerInfo pager)
        {
            var searchModel = new Customer
            {
                CustomerCode = query.CustomerCode,
                NameAr = query.NameAr,
                NameEn = query.NameEn,
                CompanyID = query.CompanyId
            };

            var result = _customerRepository.GetList(searchModel, pager, query.IsActive);
            var dtoList = result.Result.Select(MapToDto).ToList();

            return new PagedInfo<CustomerListDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                Result = dtoList
            };
        }

        /// <summary>
        /// Get customer by ID
        /// </summary>
        public CustomerDetailDto GetById(Guid id)
        {
            var customer = _customerRepository.GetById(id);
            if (customer == null) return null;

            return MapToDetailDto(customer);
        }

        /// <summary>
        /// Create customer
        /// </summary>
        public Customer Create(CreateCustomerDto dto, string userName)
        {
            var model = dto.Adapt<Customer>();
            model.Id = Guid.NewGuid();
            model.IsActive = dto.IsActive; // Explicitly set — Mapster may default bool to false
            model.Create_by = userName;
            model.Create_time = DateTime.Now;
            model.CompanyID = dto.CompanyId;

            _customerRepository.Insert(model);
            return model;
        }

        /// <summary>
        /// Update customer
        /// </summary>
        public int Update(UpdateCustomerDto dto, string userName)
        {
            var model = dto.Adapt<Customer>();
            model.Update_by = userName;
            model.Update_time = DateTime.Now;

            return Update(model, it => new
            {
                it.CustomerCode,
                it.NameAr,
                it.NameEn,
                it.ContactPerson,
                it.Email,
                it.Phone,
                it.CreditLimit,
                it.GLAccountId,
                it.IsActive,
                it.AddressAr,
                it.AddressEn,
                it.TaxNumber,
                it.DefaultCurrencyId,
                it.DefaultTaxGroupId,
                it.PaymentTermsId,
                it.CreditDays,
                it.BrandName,
                it.BrandImage,
                it.IdentityImage,
                it.TaxCertificate,
                it.CommercialRegister,
                it.CustomerProfile,
                it.WebsiteUrl,
                it.Update_by,
                it.Update_time
            }, f => f.Id == model.Id);
        }

        /// <summary>
        /// Delete customers
        /// </summary>
        public int Delete(Guid[] ids)
        {
            // Check if customer has invoices
            foreach (var id in ids)
            {
                var invoices = _invoiceRepository.GetByCustomerId(id);
                if (invoices.Any())
                    return 0; // Has invoices
            }

            return Context.Updateable<Customer>()
                .SetColumns(it => it.IsActive == false)
                .Where(it => ids.Contains(it.Id))
                .ExecuteCommand();
        }

        /// <summary>
        /// Check if customer code exists
        /// </summary>
        public bool CodeExists(string code, Guid? excludeId = null)
        {
            return _customerRepository.CodeExists(code, excludeId);
        }

        /// <summary>
        /// Generate next customer code - توليد رمز العميل التالي
        /// Format: CUS-YYYY-NNNNN
        /// </summary>
        public string GenerateNextCustomerCode()
        {
            var year = DateTime.Now.Year;
            var prefix = $"CUS-{year}-";

            var lastCustomer = Context.Queryable<Customer>()
                .Where(x => x.CustomerCode.StartsWith(prefix))
                .OrderByDescending(x => x.CustomerCode)
                .First();

            if (lastCustomer == null)
            {
                return $"{prefix}00001";
            }

            var lastNumber = lastCustomer.CustomerCode.Replace(prefix, "");
            if (int.TryParse(lastNumber, out int num))
            {
                return $"{prefix}{(num + 1):D5}";
            }

            return $"{prefix}00001";
        }

        /// <summary>
        /// Validate credit limit
        /// </summary>
        public bool ValidateCreditLimit(Guid customerId, decimal invoiceAmount)
        {
            var customer = _customerRepository.GetById(customerId);
            if (customer == null || customer.CreditLimit == 0)
                return true; // No limit or customer not found

            // Get total outstanding invoices
            var invoices = _invoiceRepository.GetByCustomerId(customerId);
            var totalOutstanding = invoices.Sum(x => x.RemainingAmount);

            return (totalOutstanding + invoiceAmount) <= customer.CreditLimit;
        }

        /// <summary>
        /// Get active customers for dropdown
        /// </summary>
        public List<CustomerSelectDto> GetActiveCustomers()
        {
            var customers = Context.Queryable<Customer>()
                .Where(c => c.IsActive)
                .OrderBy(c => c.CustomerCode)
                .ToList();

            return customers.Select(c => new CustomerSelectDto
            {
                Id = c.Id,
                CustomerCode = c.CustomerCode,
                Name = c.NameEn, // Default to English name
                DefaultCurrencyCode = null, // Can be enhanced later with currency lookup
                DefaultTaxGroupId = null, // Can be enhanced later
                PaymentTerms = null, // Can be enhanced later
                CreditLimit = c.CreditLimit,
                CurrentBalance = null // Can be calculated later from AR invoices
            }).ToList();
        }

        /// <summary>
        /// Check if customer profile is 100% complete
        /// Required fields: NameAr, NameEn, ContactPerson, Phone, Email,
        /// AddressAr, AddressEn, TaxNumber, CreditLimit, CreditDays,
        /// BrandName, BrandImage, IdentityImage, TaxCertificate, CommercialRegister,
        /// CustomerProfile OR WebsiteUrl (either one satisfies the profile/website requirement)
        /// </summary>
        public bool IsProfileComplete(Guid customerId)
        {
            return GetProfileCompletionPercentage(customerId) >= 100;
        }

        public int GetProfileCompletionPercentage(Guid customerId)
        {
            var customer = _customerRepository.GetById(customerId);
            if (customer == null) return 0;

            return CalculateProfileCompletion(customer);
        }

        public Customer GetByLeadId(long leadId)
        {
            return Context.Queryable<Customer>()
                .Where(c => c.LeadId.HasValue && c.LeadId.Value == leadId)
                .First();
        }

        private int CalculateProfileCompletion(Customer c)
        {
            int total = 16;
            int filled = 0;

            if (!string.IsNullOrWhiteSpace(c.NameAr)) filled++;
            if (!string.IsNullOrWhiteSpace(c.NameEn)) filled++;
            if (!string.IsNullOrWhiteSpace(c.ContactPerson)) filled++;
            if (!string.IsNullOrWhiteSpace(c.Phone)) filled++;
            if (!string.IsNullOrWhiteSpace(c.Email)) filled++;
            if (!string.IsNullOrWhiteSpace(c.AddressAr)) filled++;
            if (!string.IsNullOrWhiteSpace(c.AddressEn)) filled++;
            if (!string.IsNullOrWhiteSpace(c.TaxNumber)) filled++;
            if (c.CreditLimit > 0) filled++;
            if (c.CreditDays > 0) filled++;
            if (!string.IsNullOrWhiteSpace(c.BrandName)) filled++;
            if (!string.IsNullOrWhiteSpace(c.BrandImage)) filled++;
            if (!string.IsNullOrWhiteSpace(c.IdentityImage)) filled++;
            if (!string.IsNullOrWhiteSpace(c.TaxCertificate)) filled++;
            if (!string.IsNullOrWhiteSpace(c.CommercialRegister)) filled++;
            // CustomerProfile OR WebsiteUrl - either one satisfies this requirement
            if (!string.IsNullOrWhiteSpace(c.CustomerProfile) || !string.IsNullOrWhiteSpace(c.WebsiteUrl)) filled++;

            return (int)Math.Round((double)filled / total * 100);
        }

        private CustomerListDto MapToDto(Customer customer)
        {
            return customer.Adapt<CustomerListDto>();
        }

        private CustomerDetailDto MapToDetailDto(Customer customer)
        {
            return customer.Adapt<CustomerDetailDto>();
        }
    }
}
