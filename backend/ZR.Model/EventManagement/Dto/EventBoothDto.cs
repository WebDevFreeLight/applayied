using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.EventManagement.Dto
{

    // ==================== Event Booth ====================
    public class EventBoothQueryDto : PagerInfo
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? EventId { get; set; }
        public string BoothCode { get; set; }
        public string BoothName { get; set; }
        public string BoothType { get; set; }
        public string RentalStatus { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TenantId { get; set; }
    }
    public class AssignBoothToTenantRequest
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long BoothId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long TenantId { get; set; }

        // Contract Creation Flag
        public bool CreateContract { get; set; }

        // Basic Contract Information
        public string ContractNumber { get; set; }
        public DateTime? ContractDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ContractType { get; set; }

        // Financial Information
        public decimal? TotalAmount { get; set; }
        public decimal? RentalAmount { get; set; }
        public string Currency { get; set; }
        public decimal? VATPercentage { get; set; }
        public decimal? DepositAmount { get; set; }
        public decimal? RentalPricePerMeter { get; set; }
        public decimal? AdministrativeFees { get; set; }
        public decimal? WasteRemovalFees { get; set; }
        public decimal? ElectricityFees { get; set; }
        public decimal? WaterFees { get; set; }
        public decimal? POSServiceFees { get; set; }
        public decimal? EngineeringSupervisionFees { get; set; }
        public decimal? TotalContractValueIncludingVAT { get; set; }

        // Payment Information
        public string PaymentTerms { get; set; }
        public string PaymentMethod { get; set; }

        // Organizer Information (First Party)
        public string OrganizerCompanyName { get; set; }
        public string OrganizerCompanyNameAr { get; set; }
        public string OrganizerCommercialRegNumber { get; set; }
        public string OrganizerRepresentativeName { get; set; }
        public string OrganizerPhone { get; set; }
        public string OrganizerEmail { get; set; }
        public string OrganizerAddress { get; set; }

        // Tenant Extended Information (Second Party)
        public string TenantCompanyNameAr { get; set; }
        public string TenantCommercialRegNumber { get; set; }
        public string TenantRepresentativeName { get; set; }
        public string TenantNationalId { get; set; }
        public string TenantAddress { get; set; }
        public string TenantAddressAr { get; set; }
        public string TenantNationality { get; set; }

        // Site/Booth Details
        public string UnitNumber { get; set; }
        public string ProjectLocation { get; set; }
        public string ProjectLocationAr { get; set; }
        public string BusinessActivityType { get; set; }
        public string BusinessActivityTypeAr { get; set; }
        public string BrandName { get; set; }
        public string BrandNameAr { get; set; }
        public string OperationalPeriod { get; set; }
        public string OperationalPeriodAr { get; set; }
        public string ContractDuration { get; set; }
        public string ContractDurationAr { get; set; }
        public DateTime? DeliveryDate { get; set; }

        // Contract Terms & Conditions
        public int? EvacuationPeriodDays { get; set; }
        public decimal? LateEvacuationPenaltyPerDay { get; set; }
        public decimal? CancellationPenaltyPercentage { get; set; }
        public int? DisputeResolutionPeriodDays { get; set; }
        public string CompetentCourt { get; set; }
        public string CompetentCourtAr { get; set; }

        // Additional Contract Information
        public string PreambleText { get; set; }
        public string PreambleTextAr { get; set; }
        public string ContractTemplate { get; set; }
        public string TermsAndConditions { get; set; }
        public string SpecialConditions { get; set; }
        public string Notes { get; set; }
    }

    public class EventBoothDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long BoothId { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        [Required(ErrorMessage = "Event ID is required")]
        public long EventId { get; set; }
        public string BoothCode { get; set; }

        [Required(ErrorMessage = "Booth name is required")]
        public string BoothName { get; set; }
        public string BoothNameAr { get; set; }
        public string BoothType { get; set; }
        public decimal SizeSqm { get; set; }
        public decimal RentalPrice { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long? TenantId { get; set; }
        public string RentalStatus { get; set; }
        public bool HasElectricity { get; set; }
        public bool HasInternet { get; set; }

        [JsonConverter(typeof(ValueToStringConverter))]
        public long ContractId { get; set; }

        /// <summary>
        /// Description - ?????
        /// </summary>
        public string Description { get; set; }






         

        /// <summary>
        /// Zone ID - ???? ???????
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long? ZoneId { get; set; } 



        /// <summary>
        /// Booth Category - ??? ?????
        /// </summary>
        public string BoothCategory { get; set; } 

        /// <summary>
        /// Currency - ??????
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// Rental Period Type - ??? ???? ??????? (daily, weekly, full-event)
        /// </summary>
        public string RentalPeriodType { get; set; }

        /// <summary>
        /// Has Water - ????? ??? ???
        /// </summary>
        public bool HasWater { get; set; }
         

        /// <summary>
        /// Furniture Included - ???? ??????
        /// </summary>
        public bool FurnitureIncluded { get; set; }

        /// <summary>
        /// Furniture Description - ??? ??????
        /// </summary>
        public string FurnitureDescription { get; set; }

        /// <summary>
        /// Additional Amenities - ????? ??????
        /// </summary>
        public string AdditionalAmenities { get; set; }

        /// <summary>
        /// Setup Date - ????? ???????
        /// </summary>
        public DateTime? SetupDate { get; set; }

        /// <summary>
        /// Teardown Date - ????? ???????
        /// </summary>
        public DateTime? TeardownDate { get; set; }

        /// <summary>
        /// Floor Plan Image URL - ???? ???? ??????
        /// </summary>
        public string FloorPlanImageUrl { get; set; }

     


    }

}

