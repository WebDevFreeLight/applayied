using Infrastructure.Enums;
using Microsoft.AspNetCore.Http;

namespace ZR.Model.Dto
{
    public class UploadDto
    {
        /// <summary>
        /// Custom file name
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Storage directory
        /// </summary>
        public string FileDir { get; set; }
        /// <summary>
        /// File name generation type: 1 Original file name 2 Custom 3 Auto-generate
        /// </summary>
        public int FileNameType { get; set; }
        public string ClassifyType { get; set; }
        /// <summary>
        /// Category ID
        /// </summary>
        public int CategoryId { get; set; }
        /// <summary>
        /// Compression quality between 0-100
        /// </summary>
        public int Quality { get; set; }
        public string UserName { get; set; }


        public IFormFile file { get; set; }

        public IFormFileCollection files { get; set; }



        // You might want to add these as properties if they're needed:
        public StoreType StoreType { get; set; } = StoreType.LOCAL;

    }
}

