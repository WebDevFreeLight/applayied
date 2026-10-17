using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZR.Model.EventManagement.Dto
{
    // ==================== Event Tenant ====================
    public class EventTenantQueryDto : PagerInfo
    {
        public string CompanyName { get; set; }
        public string CommercialRegNumber { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
    }

    public class EventTenantDto
    {
        [JsonConverter(typeof(ValueToStringConverter))]
        public long TenantId { get; set; }

        [Required(ErrorMessage = "Company name is required")]
        public string CompanyName { get; set; }
        public string CompanyNameAr { get; set; }

        [Required(ErrorMessage = "Commercial registration number is required")]
        public string CommercialRegistration { get; set; }
        public string TaxNumber { get; set; }
 


        public string ContactName
        {
            get
            {
                // Return English name by default, Arabic as fallback
                return !string.IsNullOrWhiteSpace(ContactPersonName)
                    ? ContactPersonName
                    : CompanyNameAr ?? string.Empty;
            }
            set
            {
                ContactPersonName = value;
                // Optionally set Arabic name too if needed
                if (string.IsNullOrEmpty(CompanyNameAr))
                    CompanyNameAr = value;
            }
        }





        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Status { get; set; }

















         
        /// <summary>
        /// Event ID - ???? ????????
        /// </summary>
        [JsonConverter(typeof(ValueToStringConverter))]
        public long EventId { get; set; }

        /// <summary>
        /// Tenant Code - ??? ????????
        /// </summary>
        public string TenantCode { get; set; }
         

        /// <summary>
        /// Business Type - ??? ?????? ???????
        /// </summary>
        public string BusinessType { get; set; }

        /// <summary>
        /// Contact Person Name - ??? ??? ???????
        /// </summary>
        public string ContactPersonName { get; set; }

        /// <summary>
        /// Contact Position - ???? ??? ???????
        /// </summary>
        public string ContactPosition { get; set; } 

        /// <summary>
        /// Mobile - ??????
        /// </summary>
        public string Mobile { get; set; } 

        /// <summary>
        /// Country - ??????
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Website - ?????? ??????????
        /// </summary>
        public string Website { get; set; }

        /// <summary>
        /// Logo URL - ???? ??????
        /// </summary>
        public string LogoUrl { get; set; }

        /// <summary>
        /// Bank Name - ??? ?????
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// Account Number - ??? ??????
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// IBAN - ??? ???????
        /// </summary>
        public string IBAN { get; set; }

        /// <summary>
        /// Registration Date - ????? ???????
        /// </summary>
        public DateTime RegistrationDate { get; set; }
 

        /// <summary>
        /// Rating - ???????
        /// </summary>
        public decimal? Rating { get; set; }

        /// <summary>
        /// Total Booths Rented - ?????? ??????? ???????
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public int TotalBoothsRented { get; set; }

        /// <summary>
        /// Notes - ???????
        /// </summary>
        public string Notes { get; set; }

        ///// <summary>
        ///// Is Active - ???
        ///// </summary>
        //public bool IsActive { get; set; }

    }

}

