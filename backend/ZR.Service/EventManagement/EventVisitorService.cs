using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;
using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Extensions;
using Newtonsoft.Json;
using ZR.Model.EventManagement;
using ZR.Model.EventManagement.Dto;
using ZR.Repository;
using ZR.Service.EventManagement.IEventManagementService;

namespace ZR.Service.EventManagement
{
    /// <summary>
    /// Event Visitor - الزوار
    /// Service
    /// </summary>
    [AppService(ServiceType = typeof(IEventVisitorService), ServiceLifetime = LifeTime.Transient)]
    public class EventVisitorService : BaseService<EventVisitor>, IEventVisitorService
    {
        public PagedInfo<EventVisitorDto> GetList(EventVisitorQueryDto parm)
        {
            var predicate = QueryExp(parm);
            return Queryable()
                .Where(predicate.ToExpression())
                .ToPage<EventVisitor, EventVisitorDto>(parm);
        }

        public EventVisitor GetInfo(long visitorId)
        {
            return Queryable().Where(x => x.VisitorId == visitorId).First();
        }

        public EventVisitor AddEventVisitor(EventVisitor model)
        {
            ValidateVisitorInput(model);

            try
            {
                // Check if email already exists (globally, not per event)
                var emailExists = Context.Queryable<EventVisitor>()
                    .Where(x => x.Email == model.Email)
                    .Any();
                if (emailExists)
                {
                    throw new CustomException("Email already registered");
                }

                // Generate visitor code if not provided
                if (string.IsNullOrWhiteSpace(model.VisitorCode))
                {
                    var existingCount = Context.Queryable<EventVisitor>().Count();
                    var sequence = existingCount + 1;
                    model.VisitorCode = GenerateVisitorCode(sequence);
                }

                // Set default verification status if not provided
                if (string.IsNullOrWhiteSpace(model.VerificationStatus))
                {
                    model.VerificationStatus = "pending";
                }

                var generatedId = Insertable(model).ExecuteReturnSnowflakeId();
                model.VisitorId = generatedId;

                return model;
            }
            catch
            {
                throw;
            }
        }

        public int UpdateEventVisitor(EventVisitor model)
        {
            return Update(model, true, "Update Event Visitor");
        }

        /// <summary>
        /// GetVisitorsByEventId - DEPRECATED: Visitors are no longer tied to events.
        /// Use ticket_checkins table to find visitors by event through their tickets instead.
        /// </summary>
        [Obsolete("Visitors are no longer tied to events. Use ticket_checkins table to find visitors by event through their tickets.")]
        public List<EventVisitor> GetVisitorsByEventId(long eventId)
        {
            throw new NotImplementedException("Visitors are no longer tied to events. Use ticket_checkins table to find visitors by event through their tickets.");
        }

        /// <summary>
        /// CheckInVisitor - DEPRECATED: Check-ins are now only linked to tickets.
        /// Use TicketCheckinService to check in tickets instead.
        /// </summary>
        [Obsolete("Check-ins are now only linked to tickets. Use TicketCheckinService to check in tickets instead.")]
        public int CheckInVisitor(long visitorId, long gateId)
        {
            throw new NotImplementedException("Check-ins are now only linked to tickets. Use TicketCheckinService to check in tickets instead.");
        }

        /// <summary>
        /// CheckOutVisitor - DEPRECATED: Check-ins are now only linked to tickets.
        /// Use TicketCheckinService to check out tickets instead.
        /// </summary>
        [Obsolete("Check-ins are now only linked to tickets. Use TicketCheckinService to check out tickets instead.")]
        public int CheckOutVisitor(long visitorId, long gateId)
        {
            throw new NotImplementedException("Check-ins are now only linked to tickets. Use TicketCheckinService to check out tickets instead.");
        }

        /// <summary>
        /// GetVisitorByQRCode - DEPRECATED: QRCode field removed from visitor table.
        /// QR codes should be generated dynamically or stored in a separate table.
        /// </summary>
        [Obsolete("QRCode field removed from visitor table. QR codes should be generated dynamically or stored in a separate table.")]
        public EventVisitor GetVisitorByQRCode(string qrCode)
        {
            throw new NotImplementedException("QRCode field removed from visitor table. QR codes should be generated dynamically or stored in a separate table.");
        }

        private static Expressionable<EventVisitor> QueryExp(EventVisitorQueryDto parm)
        {
            var predicate = Expressionable.Create<EventVisitor>();

            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.VisitorCode), it => it.VisitorCode.Contains(parm.VisitorCode));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.FirstName), it => it.FirstName.Contains(parm.FirstName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.LastName), it => it.LastName.Contains(parm.LastName));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.FirstNameAr), it => it.FirstNameAr.Contains(parm.FirstNameAr));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.LastNameAr), it => it.LastNameAr.Contains(parm.LastNameAr));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Email), it => it.Email.Contains(parm.Email));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.PhoneNumber), it => it.PhoneNumber.Contains(parm.PhoneNumber));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.NationalId), it => it.NationalId.Contains(parm.NationalId));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.PassportNumber), it => it.PassportNumber.Contains(parm.PassportNumber));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.IdentificationNumber), it => it.IdentificationNumber.Contains(parm.IdentificationNumber));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.IdentificationType), it => it.IdentificationType == parm.IdentificationType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Nationality), it => it.Nationality.Contains(parm.Nationality));
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.NationalityCode), it => it.NationalityCode == parm.NationalityCode);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.Gender), it => it.Gender == parm.Gender);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.VisitorType), it => it.VisitorType == parm.VisitorType);
            predicate = predicate.AndIF(!string.IsNullOrEmpty(parm.VerificationStatus), it => it.VerificationStatus == parm.VerificationStatus);

            return predicate;
        }

        private static readonly Regex PhoneRegex = new Regex(@"^[0-9+\-()\s]{6,20}$", RegexOptions.Compiled);

        private void ValidateVisitorInput(EventVisitor model)
        {
            if (model == null)
            {
                throw new CustomException("Visitor payload is required");
            }

            model.FirstName = model.FirstName?.Trim();
            model.LastName = model.LastName?.Trim();
            model.FirstNameAr = model.FirstNameAr?.Trim();
            model.LastNameAr = model.LastNameAr?.Trim();
            model.Email = model.Email?.Trim().ToLowerInvariant();
            model.PhoneNumber = model.PhoneNumber?.Trim();
            model.NationalId = model.NationalId?.Trim();
            model.PassportNumber = model.PassportNumber?.Trim();
            model.IdentificationNumber = model.IdentificationNumber?.Trim();
            model.IdentificationType = model.IdentificationType?.Trim();
            model.Nationality = model.Nationality?.Trim();
            model.NationalityCode = model.NationalityCode?.Trim();
            model.VisitorType = model.VisitorType?.Trim();
            model.VerificationStatus = model.VerificationStatus?.Trim();

            if (string.IsNullOrWhiteSpace(model.FirstName))
            {
                throw new CustomException("First name is required");
            }

            if (string.IsNullOrWhiteSpace(model.LastName))
            {
                throw new CustomException("Last name is required");
            }

            if (!string.IsNullOrWhiteSpace(model.Email) && !new EmailAddressAttribute().IsValid(model.Email))
            {
                throw new CustomException("Invalid email format");
            }

            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                throw new CustomException("Phone number is required");
            }

            if (model.PhoneNumber.Length > 20 || !PhoneRegex.IsMatch(model.PhoneNumber))
            {
                throw new CustomException("Invalid phone number format");
            }

            // Validate that at least one identification field is provided
            if (string.IsNullOrWhiteSpace(model.NationalId) && 
                string.IsNullOrWhiteSpace(model.PassportNumber) && 
                string.IsNullOrWhiteSpace(model.IdentificationNumber))
            {
                throw new CustomException("At least one identification field (National ID, Passport Number, or Identification Number) is required");
            }

            if (model.DateOfBirth.HasValue)
            {
                var dob = model.DateOfBirth.Value.Date;
                if (dob > DateTime.Today)
                {
                    throw new CustomException("Date of birth cannot be in the future");
                }

                var age = CalculateAge(dob, DateTime.Today);
                if (age < 16)
                {
                    throw new CustomException("Visitor must be at least 16 years old");
                }

                if (age > 120)
                {
                    throw new CustomException("Date of birth is invalid");
                }
            }
        }

        private static int CalculateAge(DateTime birthDate, DateTime referenceDate)
        {
            var age = referenceDate.Year - birthDate.Year;
            if (birthDate.Date > referenceDate.AddYears(-age))
            {
                age--;
            }
            return age;
        }

        private static string GenerateVisitorCode(int sequence)
        {
            return $"VIS-{DateTime.Now:yyyyMMdd}-{sequence:D6}";
        }
    }
}

