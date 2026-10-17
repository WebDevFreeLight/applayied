using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using ZR.Model.Boulevard;
using ZR.Model.Customers;
using ZR.Model.Operations;
using ZR.Model.Operations.Dto;
using ZR.Repository;

namespace ZR.Service.Operations
{
    [AppService(ServiceType = typeof(IOpsPermitService), ServiceLifetime = LifeTime.Transient)]
    public class OpsPermitService : BaseService<OpsPermit>, IOpsPermitService
    {
        public PagedInfo<OpsPermitDto> GetList(OpsPermitQueryDto parm)
        {
            var response = Context.Queryable<OpsPermit, OpsPermitType, BlvBoulevardSeasons, BlvBoulevardShops>(
                    (p, pt, s, sh) => new JoinQueryInfos(
                        JoinType.Left, p.PermitTypeId == pt.PermitTypeId,
                        JoinType.Left, p.SeasonId == s.Id,
                        JoinType.Left, p.ShopId == sh.Id))
                .WhereIF(!string.IsNullOrEmpty(parm.PermitNumber), (p, pt, s, sh) => p.PermitNumber.Contains(parm.PermitNumber))
                .WhereIF(parm.SeasonId != null, (p, pt, s, sh) => p.SeasonId == parm.SeasonId)
                .WhereIF(parm.ZoneId != null, (p, pt, s, sh) => p.ZoneId == parm.ZoneId)
                .WhereIF(parm.ShopId != null, (p, pt, s, sh) => p.ShopId == parm.ShopId)
                .WhereIF(!string.IsNullOrEmpty(parm.CustomerId), (p, pt, s, sh) => p.CustomerId == parm.CustomerId)
                .WhereIF(parm.WorkerId != null, (p, pt, s, sh) => p.WorkerId == parm.WorkerId)
                .WhereIF(parm.PermitTypeId != null, (p, pt, s, sh) => p.PermitTypeId == parm.PermitTypeId)
                .WhereIF(!string.IsNullOrEmpty(parm.Status), (p, pt, s, sh) => p.Status == parm.Status)
                .WhereIF(!string.IsNullOrEmpty(parm.ApplicantName), (p, pt, s, sh) => p.ApplicantName.Contains(parm.ApplicantName))
                .WhereIF(!string.IsNullOrEmpty(parm.Direction), (p, pt, s, sh) => p.Direction == parm.Direction)
                .WhereIF(!string.IsNullOrEmpty(parm.WorkCondition), (p, pt, s, sh) => p.WorkCondition == parm.WorkCondition)
                .WhereIF(parm.BeginValidFrom != null, (p, pt, s, sh) => p.ValidFrom >= parm.BeginValidFrom)
                .WhereIF(parm.EndValidFrom != null, (p, pt, s, sh) => p.ValidFrom <= parm.EndValidFrom)
                .WhereIF(parm.BeginValidUntil != null, (p, pt, s, sh) => p.ValidUntil >= parm.BeginValidUntil)
                .WhereIF(parm.EndValidUntil != null, (p, pt, s, sh) => p.ValidUntil <= parm.EndValidUntil)
                .WhereIF(parm.BeginOpeningDate != null, (p, pt, s, sh) => p.OpeningDate >= parm.BeginOpeningDate)
                .WhereIF(parm.EndOpeningDate != null, (p, pt, s, sh) => p.OpeningDate <= parm.EndOpeningDate)
                .OrderBy((p, pt, s, sh) => p.Create_time, OrderByType.Desc)
                .Select((p, pt, s, sh) => new OpsPermitDto
                {
                    PermitId = p.PermitId,
                    PermitNumber = p.PermitNumber,
                    SeasonId = p.SeasonId,
                    ZoneId = p.ZoneId,
                    CustomerId = p.CustomerId,
                    WorkerId = p.WorkerId,
                    PermitTypeId = p.PermitTypeId,
                    ApplicantName = p.ApplicantName,
                    ApplicantNameAr = p.ApplicantNameAr,
                    NationalId = p.NationalId,
                    ContactPhone = p.ContactPhone,
                    Email = p.Email,
                    CompanyName = p.CompanyName,
                    ValidFrom = p.ValidFrom,
                    ValidUntil = p.ValidUntil,
                    IssueDate = p.IssueDate,
                    IssuedBy = p.IssuedBy,
                    Purpose = p.Purpose,
                    AccessLevel = p.AccessLevel,
                    VehiclePlateNumber = p.VehiclePlateNumber,
                    VehicleType = p.VehicleType,
                    PhotoUrl = p.PhotoUrl,
                    AttachmentUrl = p.AttachmentUrl,
                    QRCode = p.QRCode,
                    Status = p.Status,
                    ApprovedBy = p.ApprovedBy,
                    ApprovalDate = p.ApprovalDate,
                    RejectionReason = p.RejectionReason,
                    Notes = p.Notes,
                    // Sela fields
                    ShopId = p.ShopId,
                    GateNumber = p.GateNumber,
                    Direction = p.Direction,
                    OpeningDate = p.OpeningDate,
                    ClosingDate = p.ClosingDate,
                    WorkTypeIds = p.WorkTypeIds,
                    WorkCategoryIds = p.WorkCategoryIds,
                    WorkCondition = p.WorkCondition,
                    WorkDescription = p.WorkDescription,
                    // Audit
                    CreateBy = p.Create_by,
                    CreateTime = p.Create_time,
                    // Navigation
                    PermitTypeName = pt.TypeName,
                    SeasonName = s.Name,
                    ShopName = sh.Name
                })
                .ToPage(parm);

            return response;
        }

        public OpsPermit GetInfo(long PermitId)
        {
            return Queryable().Where(x => x.PermitId == PermitId).First();
        }

        public OpsPermit AddOpsPermit(OpsPermit model)
        {
            var year = DateTime.Now.Year;
            var count = Queryable().Where(x => x.Create_time.Year == year).Count() + 1;
            model.PermitNumber = $"PRM-{year}-{count:D5}";
            model.IssueDate = DateTime.Now;
            model.QRCode = $"PERMIT:{model.PermitNumber}:{Guid.NewGuid():N}";

            Insertable(model).ExecuteReturnSnowflakeId();
            return model;
        }

        public int UpdateOpsPermit(OpsPermit model)
        {
            return Update(model, true, "Update permit");
        }

        public PagedInfo<OpsPermitDto> ExportList(OpsPermitQueryDto parm)
        {
            parm.PageNum = 1;
            parm.PageSize = 100000;
            return GetList(parm);
        }

        public List<OpsPermitDto> GetByCustomerId(string customerId)
        {
            return Queryable()
                .Where(x => x.CustomerId == customerId)
                .OrderBy(x => x.Create_time, OrderByType.Desc)
                .Select(x => new OpsPermitDto
                {
                    PermitId = x.PermitId,
                    PermitNumber = x.PermitNumber,
                    ApplicantName = x.ApplicantName,
                    Status = x.Status,
                    ValidFrom = x.ValidFrom,
                    ValidUntil = x.ValidUntil
                })
                .ToList();
        }

        public List<OpsPermitDto> GetBySeasonId(long seasonId)
        {
            return Queryable()
                .Where(x => x.SeasonId == seasonId)
                .OrderBy(x => x.Create_time, OrderByType.Desc)
                .Select(x => new OpsPermitDto
                {
                    PermitId = x.PermitId,
                    PermitNumber = x.PermitNumber,
                    ApplicantName = x.ApplicantName,
                    Status = x.Status,
                    ValidFrom = x.ValidFrom,
                    ValidUntil = x.ValidUntil
                })
                .ToList();
        }

        public List<OpsPermitDto> GetByWorkerId(long workerId)
        {
            return Queryable()
                .Where(x => x.WorkerId == workerId)
                .OrderBy(x => x.Create_time, OrderByType.Desc)
                .Select(x => new OpsPermitDto
                {
                    PermitId = x.PermitId,
                    PermitNumber = x.PermitNumber,
                    ApplicantName = x.ApplicantName,
                    Status = x.Status,
                    ValidFrom = x.ValidFrom,
                    ValidUntil = x.ValidUntil
                })
                .ToList();
        }

        public OpsPermitStatsDto GetStats()
        {
            var today = DateTime.Today;
            var all = Queryable().ToList();
            return new OpsPermitStatsDto
            {
                Total = all.Count,
                Pending = all.Count(x => x.Status == "pending"),
                Approved = all.Count(x => x.Status == "approved"),
                Active = all.Count(x => x.Status == "active"),
                Rejected = all.Count(x => x.Status == "rejected"),
                Expired = all.Count(x => x.Status == "expired"),
                Cancelled = all.Count(x => x.Status == "cancelled"),
                TodayCount = all.Count(x => x.Create_time.Date == today)
            };
        }

        public PagedInfo<OpsPermitDto> GetDigitalPermits(OpsPermitQueryDto parm)
        {
            parm.Status = "approved";
            return GetList(parm);
        }

        public int ApprovePermit(long permitId, long approvedBy)
        {
            var permit = GetInfo(permitId);
            if (permit == null) return 0;
            permit.Status = "approved";
            permit.ApprovedBy = approvedBy;
            permit.ApprovalDate = DateTime.Now;
            permit.Update_time = DateTime.Now;
            return Update(permit, true, "Approve permit");
        }

        public int RejectPermit(long permitId, string reason)
        {
            var permit = GetInfo(permitId);
            if (permit == null) return 0;
            permit.Status = "rejected";
            permit.RejectionReason = reason;
            permit.Update_time = DateTime.Now;
            return Update(permit, true, "Reject permit");
        }

        public List<OpsPermitMonthlyReportDto> GetMonthlyReport(int year)
        {
            var permits = Queryable()
                .Where(x => x.Create_time.Year == year)
                .ToList();

            var report = new List<OpsPermitMonthlyReportDto>();
            for (int m = 1; m <= 12; m++)
            {
                var monthPermits = permits.Where(x => x.Create_time.Month == m).ToList();
                report.Add(new OpsPermitMonthlyReportDto
                {
                    Month = m,
                    MonthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(m),
                    Total = monthPermits.Count,
                    Approved = monthPermits.Count(x => x.Status == "approved" || x.Status == "active"),
                    Rejected = monthPermits.Count(x => x.Status == "rejected"),
                    Pending = monthPermits.Count(x => x.Status == "pending")
                });
            }
            return report;
        }

        public List<OpsPermitDistributionDto> GetPermitTypeDistribution()
        {
            var permits = Context.Queryable<OpsPermit, OpsPermitType>(
                    (p, pt) => new JoinQueryInfos(JoinType.Left, p.PermitTypeId == pt.PermitTypeId))
                .GroupBy((p, pt) => new { pt.PermitTypeId, pt.TypeName })
                .Select((p, pt) => new OpsPermitDistributionDto
                {
                    PermitTypeName = pt.TypeName,
                    Count = SqlFunc.AggregateCount(p.PermitId)
                })
                .ToList();

            var total = permits.Sum(x => x.Count);
            foreach (var item in permits)
            {
                item.Percentage = total > 0 ? Math.Round((decimal)item.Count / total * 100, 1) : 0;
            }
            return permits;
        }

        /// <summary>
        /// Create permit with workers and safety checks in a single transaction
        /// </summary>
        public OpsPermitDto AddPermitFull(OpsPermitCreateRequest request, HttpContext httpContext)
        {
            // Generate permit number
            var year = DateTime.Now.Year;
            var count = Queryable().Where(x => x.Create_time.Year == year).Count() + 1;

            // Auto-populate company name from customer
            string companyName = null;
            string applicantName = "Work Permit";
            if (!string.IsNullOrEmpty(request.CustomerId) && Guid.TryParse(request.CustomerId, out var custGuid))
            {
                var customer = Context.Queryable<Customer>()
                    .Where(c => c.Id == custGuid)
                    .First();
                if (customer != null)
                {
                    companyName = customer.NameEn ?? customer.NameAr;
                    applicantName = customer.NameEn ?? customer.NameAr ?? "Work Permit";
                }
            }

            var permit = new OpsPermit
            {
                PermitNumber = $"PRM-{year}-{count:D5}",
                SeasonId = request.SeasonId,
                ZoneId = request.ZoneId,
                ShopId = request.ShopId,
                CustomerId = request.CustomerId,
                PermitTypeId = request.PermitTypeId,
                ApplicantName = applicantName,
                CompanyName = companyName,
                GateNumber = request.GateNumber,
                Direction = request.Direction,
                OpeningDate = request.OpeningDate,
                ClosingDate = request.ClosingDate,
                WorkTypeIds = request.WorkTypeIds,
                WorkCategoryIds = request.WorkCategoryIds,
                WorkCondition = request.WorkCondition,
                WorkDescription = request.WorkDescription,
                VehiclePlateNumber = request.VehiclePlateNumber,
                VehicleType = request.VehicleType,
                Notes = request.Notes,
                ValidFrom = request.OpeningDate ?? DateTime.Now,
                ValidUntil = request.ClosingDate ?? DateTime.Now.AddDays(1),
                IssueDate = DateTime.Now,
                QRCode = $"PERMIT:PRM-{year}-{count:D5}:{Guid.NewGuid():N}",
                Status = "pending"
            }.ToCreate(httpContext);

            // Use transaction
            var result = Context.Ado.UseTran(() =>
            {
                // Insert permit
                Context.Insertable(permit).ExecuteReturnSnowflakeId();

                // Insert workers
                if (request.WorkerIds != null && request.WorkerIds.Count > 0)
                {
                    var workers = request.WorkerIds.Select(wid => new OpsPermitWorker
                    {
                        Id = SnowFlakeSingle.Instance.NextId(),
                        PermitId = permit.PermitId,
                        WorkerId = wid
                    }.ToCreate(httpContext)).ToList();

                    Context.Insertable(workers).ExecuteCommand();
                }

                // Insert safety checks
                if (request.SafetyChecks != null && request.SafetyChecks.Count > 0)
                {
                    var checks = request.SafetyChecks.Select(sc => new OpsPermitSafetyCheck
                    {
                        Id = SnowFlakeSingle.Instance.NextId(),
                        PermitId = permit.PermitId,
                        PrecautionId = sc.PrecautionId,
                        IsChecked = sc.IsChecked,
                        Notes = sc.Notes
                    }.ToCreate(httpContext)).ToList();

                    Context.Insertable(checks).ExecuteCommand();
                }
            });

            if (!result.IsSuccess)
            {
                throw new Exception($"Failed to create permit: {result.ErrorMessage}");
            }

            return GetPermitDetail(permit.PermitId);
        }

        /// <summary>
        /// Get full permit detail with workers and safety checks
        /// </summary>
        public OpsPermitDto GetPermitDetail(long permitId)
        {
            // Get permit with joins
            var dto = Context.Queryable<OpsPermit, OpsPermitType, BlvBoulevardSeasons, BlvBoulevardShops, BlvBoulevardSeasonZones>(
                    (p, pt, s, sh, z) => new JoinQueryInfos(
                        JoinType.Left, p.PermitTypeId == pt.PermitTypeId,
                        JoinType.Left, p.SeasonId == s.Id,
                        JoinType.Left, p.ShopId == sh.Id,
                        JoinType.Left, p.ZoneId == z.Id))
                .Where((p, pt, s, sh, z) => p.PermitId == permitId)
                .Select((p, pt, s, sh, z) => new OpsPermitDto
                {
                    PermitId = p.PermitId,
                    PermitNumber = p.PermitNumber,
                    SeasonId = p.SeasonId,
                    ZoneId = p.ZoneId,
                    CustomerId = p.CustomerId,
                    WorkerId = p.WorkerId,
                    PermitTypeId = p.PermitTypeId,
                    ApplicantName = p.ApplicantName,
                    ApplicantNameAr = p.ApplicantNameAr,
                    NationalId = p.NationalId,
                    ContactPhone = p.ContactPhone,
                    Email = p.Email,
                    CompanyName = p.CompanyName,
                    ValidFrom = p.ValidFrom,
                    ValidUntil = p.ValidUntil,
                    IssueDate = p.IssueDate,
                    IssuedBy = p.IssuedBy,
                    Purpose = p.Purpose,
                    AccessLevel = p.AccessLevel,
                    VehiclePlateNumber = p.VehiclePlateNumber,
                    VehicleType = p.VehicleType,
                    PhotoUrl = p.PhotoUrl,
                    AttachmentUrl = p.AttachmentUrl,
                    QRCode = p.QRCode,
                    Status = p.Status,
                    ApprovedBy = p.ApprovedBy,
                    ApprovalDate = p.ApprovalDate,
                    RejectionReason = p.RejectionReason,
                    Notes = p.Notes,
                    ShopId = p.ShopId,
                    GateNumber = p.GateNumber,
                    Direction = p.Direction,
                    OpeningDate = p.OpeningDate,
                    ClosingDate = p.ClosingDate,
                    WorkTypeIds = p.WorkTypeIds,
                    WorkCategoryIds = p.WorkCategoryIds,
                    WorkCondition = p.WorkCondition,
                    WorkDescription = p.WorkDescription,
                    CreateBy = p.Create_by,
                    CreateTime = p.Create_time,
                    PermitTypeName = pt.TypeName,
                    SeasonName = s.Name,
                    ShopName = sh.Name,
                    ZoneName = z.Name
                })
                .First();

            if (dto == null) return null;

            // Get workers
            dto.Workers = Context.Queryable<OpsPermitWorker, OpsCustomerWorker>(
                    (pw, w) => new JoinQueryInfos(JoinType.Left, pw.WorkerId == w.WorkerId))
                .Where((pw, w) => pw.PermitId == permitId)
                .Select((pw, w) => new OpsPermitWorkerDto
                {
                    Id = pw.Id,
                    PermitId = pw.PermitId,
                    WorkerId = pw.WorkerId,
                    WorkerName = w.FullName,
                    WorkerNameAr = w.FullNameAr,
                    NationalId = w.NationalId,
                    Phone = w.Phone,
                    IqamaImageUrl = w.IqamaImageUrl
                })
                .ToList();

            // Get safety checks
            dto.SafetyChecks = Context.Queryable<OpsPermitSafetyCheck, OpsSafetyPrecaution>(
                    (sc, sp) => new JoinQueryInfos(JoinType.Left, sc.PrecautionId == sp.PrecautionId))
                .Where((sc, sp) => sc.PermitId == permitId)
                .Select((sc, sp) => new OpsPermitSafetyCheckDto
                {
                    Id = sc.Id,
                    PermitId = sc.PermitId,
                    PrecautionId = sc.PrecautionId,
                    IsChecked = sc.IsChecked,
                    Notes = sc.Notes,
                    PrecautionDescription = sp.Description,
                    PrecautionDescriptionAr = sp.DescriptionAr,
                    PrecautionCode = sp.Code
                })
                .ToList();

            // Resolve work type names
            if (!string.IsNullOrEmpty(dto.WorkTypeIds))
            {
                var typeIds = dto.WorkTypeIds.Split(',').Select(long.Parse).ToList();
                dto.WorkTypeNames = Context.Queryable<OpsWorkType>()
                    .Where(x => typeIds.Contains(x.WorkTypeId))
                    .Select(x => x.Name)
                    .ToList();
            }

            // Resolve work category names
            if (!string.IsNullOrEmpty(dto.WorkCategoryIds))
            {
                var catIds = dto.WorkCategoryIds.Split(',').Select(long.Parse).ToList();
                dto.WorkCategoryNames = Context.Queryable<OpsWorkCategory>()
                    .Where(x => catIds.Contains(x.WorkCategoryId))
                    .Select(x => x.Name)
                    .ToList();
            }

            return dto;
        }
    }
}
