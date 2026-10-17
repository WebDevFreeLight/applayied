using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Attribute;
using Mapster;
using SqlSugar;
using ZR.Model;
using ZR.Model.Finance.ARAP;
using ZR.Model.Finance.ARAP.Dto;
using ZR.Repository.Finance.ARAP;
using ZR.Service.IService.Finance.ARAP;

namespace ZR.Service.Finance.ARAP
{
    /// <summary>
    /// Vendor Service - خدمة الموردين
    /// </summary>
    [AppService(ServiceType = typeof(IVendorService), ServiceLifetime = LifeTime.Transient)]
    public class VendorService : BaseService<Vendor>, IVendorService
    {
        private readonly IVendorRepository _vendorRepository;
        private readonly IAPInvoiceRepository _invoiceRepository;

        public VendorService(
            IVendorRepository vendorRepository,
            IAPInvoiceRepository invoiceRepository)
        {
            _vendorRepository = vendorRepository;
            _invoiceRepository = invoiceRepository;
        }

        /// <summary>
        /// Get paginated list of vendors
        /// </summary>
        public PagedInfo<VendorListDto> GetList(VendorQueryDto query, PagerInfo pager)
        {
            var searchModel = new Vendor
            {
                VendorCode = query.VendorCode,
                NameAr = query.NameAr,
                NameEn = query.NameEn,
                CompanyID = query.CompanyId
            };

            var result = _vendorRepository.GetList(searchModel, pager);
            var dtoList = result.Result.Select(MapToDto).ToList();

            return new PagedInfo<VendorListDto>
            {
                TotalNum = result.TotalNum,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                Result = dtoList
            };
        }

        /// <summary>
        /// Get vendor by ID
        /// </summary>
        public VendorDetailDto GetById(Guid id)
        {
            var vendor = _vendorRepository.GetById(id);
            if (vendor == null) return null;

            return MapToDetailDto(vendor);
        }

        /// <summary>
        /// Create vendor
        /// </summary>
        public Vendor Create(CreateVendorDto dto, string userName)
        {
            var model = dto.Adapt<Vendor>();
            model.Id = Guid.NewGuid();
            model.Create_by = userName;
            model.Create_time = DateTime.Now;

            _vendorRepository.Insert(model);
            return model;
        }

        /// <summary>
        /// Update vendor
        /// </summary>
        public int Update(UpdateVendorDto dto, string userName)
        {
            var model = dto.Adapt<Vendor>();
            model.Update_by = userName;
            model.Update_time = DateTime.Now;

            return Update(model, it => new
            {
                it.VendorCode,
                it.NameAr,
                it.NameEn,
                it.ContactPerson,
                it.Email,
                it.Phone,
                it.GLAccountId,
                it.IsActive,
                it.Update_by,
                it.Update_time
            }, f => f.Id == model.Id);
        }

        /// <summary>
        /// Delete vendors
        /// </summary>
        public int Delete(Guid[] ids)
        {
            // Check if vendor has invoices
            foreach (var id in ids)
            {
                var invoices = _invoiceRepository.GetByVendorId(id);
                if (invoices.Any())
                    return 0; // Has invoices
            }

            return Context.Updateable<Vendor>()
                .SetColumns(it => it.IsActive == false)
                .Where(it => ids.Contains(it.Id))
                .ExecuteCommand();
        }

        /// <summary>
        /// Check if vendor code exists
        /// </summary>
        public bool CodeExists(string code, Guid? excludeId = null)
        {
            return _vendorRepository.CodeExists(code, excludeId);
        }

        /// <summary>
        /// Generate next vendor code - توليد رمز المورد التالي
        /// Format: VEN-YYYY-NNNNN
        /// </summary>
        public string GenerateNextVendorCode()
        {
            var year = DateTime.Now.Year;
            var prefix = $"VEN-{year}-";

            var lastVendor = Context.Queryable<Vendor>()
                .Where(x => x.VendorCode.StartsWith(prefix))
                .OrderByDescending(x => x.VendorCode)
                .First();

            if (lastVendor == null)
            {
                return $"{prefix}00001";
            }

            var lastNumber = lastVendor.VendorCode.Replace(prefix, "");
            if (int.TryParse(lastNumber, out int num))
            {
                return $"{prefix}{(num + 1):D5}";
            }

            return $"{prefix}00001";
        }

        /// <summary>
        /// Get active vendors for dropdown
        /// </summary>
        public List<VendorSelectDto> GetActiveVendors()
        {
            var vendors = Context.Queryable<Vendor>()
                .Where(v => v.IsActive)
                .OrderBy(v => v.VendorCode)
                .ToList();

            return vendors.Select(v => new VendorSelectDto
            {
                Id = v.Id,
                VendorCode = v.VendorCode,
                Name = v.NameEn, // Default to English name
                DefaultCurrencyCode = null, // Can be enhanced later with currency lookup
                DefaultTaxGroupId = null, // Can be enhanced later
                PaymentTerms = null // Can be enhanced later
            }).ToList();
        }

        private VendorListDto MapToDto(Vendor vendor)
        {
            return vendor.Adapt<VendorListDto>();
        }

        private VendorDetailDto MapToDetailDto(Vendor vendor)
        {
            return vendor.Adapt<VendorDetailDto>();
        }
    }
}


