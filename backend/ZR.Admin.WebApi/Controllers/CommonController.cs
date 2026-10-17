using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using MiniExcelLibs;
using ZR.Infrastructure.IPTools;
using ZR.Model.Dto;
using ZR.Model.System;
using ZR.ServiceCore.Resources;
using ZR.Infrastructure.Helper; // SECURITY: Added for SecureFileValidator  

namespace ZR.Admin.WebApi.Controllers
{
    /// <summary>
    /// Common module 0
    /// </summary>
    [Route("[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class CommonController : BaseController
    {
        private OptionsSetting OptionsSetting;
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IWebHostEnvironment WebHostEnvironment;
        private ISysFileService SysFileService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        /// <summary>
        /// xbvbcvbcvبتاتااتبل
        /// </summary>
        /// <param name="stringLocalizer"></param>
        /// <param name="options"></param>
        /// <param name="webHostEnvironment"></param>
        /// <param name="fileService"></param>
        public CommonController(
            IStringLocalizer<SharedResource> stringLocalizer,
            IOptions<OptionsSetting> options,
            IWebHostEnvironment webHostEnvironment,
            ISysFileService fileService)
        {
            WebHostEnvironment = webHostEnvironment;
            SysFileService = fileService;
            OptionsSetting = options.Value;
            _localizer = stringLocalizer;
        }

        /// <summary>
        /// home
        /// </summary>
        /// <returns></returns>
        [Route("/")]
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            var hello = _localizer["hello"].Value;





        //if (WebHostEnvironment.IsProduction())
        //    {
        //        var physicalPath = Path.Combine(WebHostEnvironment.WebRootPath, "forproductionhost", "index.html");



        //        return PhysicalFile(physicalPath, "text/html", true);
        //    }






            return Ok($"{hello}Seeing this page means you have successfully started this project:)\n\n" +
                "If you find this project useful, buy the author a coffee as a reward\n??http://www.izhaorui.cn/vip\n");
        }

        /// <summary>
        /// Query IP information
        /// </summary>
        /// <returns></returns>
        [Route("/ip")]
        [HttpGet]
        [AllowAnonymous]
        public IActionResult IPInfo(string ip)
        {
            if (ip.IsEmpty()) return ToResponse(ResultCode.CUSTOM_ERROR, "IP exception");

            var region = IpTool.GetRegion(ip);

            return SUCCESS(region);
        }

        /// <summary>
        /// Enterprise message test
        /// </summary>
        /// <param name="msg">Message to send</param>
        /// <param name="toUser">Person to send to @all for everyone, xxx for individual</param>
        /// <returns></returns>
        [Route("/sendMsg")]
        [HttpGet]
        [Log(Title = "Enterprise message test")]
        public IActionResult SendMsg(string msg, string toUser = "")
        {
            WxNoticeHelper.SendMsg("Message test", msg, toUser, WxNoticeHelper.MsgType.markdown);
            return SUCCESS(msg);
        }

        #region Upload

        /// <summary>
        /// Store file
        /// </summary>
        /// <param name="uploadDto">Custom file name and file data</param>
        /// <param name="storeType">Upload type 1. Save locally 2. Save to Aliyun</param>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "common")]
        public async Task<IActionResult> UploadFile([FromForm] UploadDto uploadDto)
        {
            var file = uploadDto.file;


            StoreType storeType = StoreType.LOCAL;
            if (uploadDto!=null )
            {
                if (uploadDto.StoreType!=null)
                {
                    storeType = StoreType.LOCAL;
                }
            }


            if (file == null) throw new CustomException(ResultCode.PARAM_ERROR, "Upload file cannot be empty");

            // SECURITY FIX: Use whitelist-based validation with magic byte verification
            var validationResult = SecureFileValidator.ValidateFile(file, OptionsSetting.Upload.MaxSize * 1024 * 1024);
            if (!validationResult.IsValid)
            {
                logger.Warn($"File upload rejected: {validationResult.ErrorMessage} - Filename: {file.FileName}, User: {HttpContext.GetName()}");
                return ToResponse(ResultCode.CUSTOM_ERROR, $"Upload failed: {validationResult.ErrorMessage}");
            }

            SysFile sysfile = new();
            IFormFile formFile = file;
            string fileExt = Path.GetExtension(formFile.FileName);//File extension
            double fileSize = Math.Round(formFile.Length / 1024.0, 2);//File size in KB

            // SECURITY: Use secure filename from validator
            if (uploadDto.FileNameType == 1)
            {
                uploadDto.FileName = Path.GetFileNameWithoutExtension(formFile.FileName);
            }
            else if (uploadDto.FileNameType == 3)
            {
                uploadDto.FileName = validationResult.SecureFileName.Replace(fileExt, ""); // Use crypto-secure name
            }
            switch (storeType)
            {
                case StoreType.LOCAL:
                    // SECURITY: Store uploads OUTSIDE wwwroot in secure-uploads directory
                    string savePath = Path.Combine(WebHostEnvironment.ContentRootPath, "secure-uploads");
                    if (uploadDto.FileDir.IsEmpty())
                    {
                        uploadDto.FileDir = ""; // Don't use wwwroot path
                    }
                    sysfile = await SysFileService.SaveFileToLocal(savePath, uploadDto, HttpContext.GetName(), formFile);
                    break;
                case StoreType.REMOTE:
                    break;
                case StoreType.ALIYUN:
                    int AlimaxContentLength = OptionsSetting.ALIYUN_OSS.MaxSize;
                    if (OptionsSetting.ALIYUN_OSS.REGIONID.IsEmpty())
                    {
                        return ToResponse(ResultCode.CUSTOM_ERROR, "Configuration file missing");
                    }
                    if ((fileSize / 1024) > AlimaxContentLength)
                    {
                        return ToResponse(ResultCode.CUSTOM_ERROR, "Upload file too large, cannot exceed " + AlimaxContentLength + " MB");
                    }
                    sysfile = new(formFile.FileName, uploadDto.FileName, fileExt, fileSize + "kb", uploadDto.FileDir, HttpContext.GetName())
                    {
                        StoreType = (int)StoreType.ALIYUN,
                        FileType = formFile.ContentType,
                        ClassifyType = uploadDto.ClassifyType,
                        CategoryId = uploadDto.CategoryId,
                    };
                    sysfile = await SysFileService.SaveFileToAliyun(sysfile, uploadDto, formFile);

                    if (sysfile.Id <= 0) { 
                        return ToResponse(ApiResult.Error("Aliyun connection failed"));
                    
                    
                    }
                    break;
                case StoreType.TENCENT:
                    break;
                case StoreType.QINIU:
                    break;
                default:
                    break;
            }
            return SUCCESS(new
            {
                url = sysfile.AccessUrl,
                fileName = sysfile.FileName,
                fileId = sysfile.Id.ToString()
            });
        }











        /// <summary>
        /// Store file - SECURED: Requires authentication
        /// </summary>
        /// <param name="uploadDto">Custom file name and file data</param>
        /// <param name="storeType">Upload type 1. Save locally 2. Save to Aliyun</param>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "common")]
        // SECURITY FIX: Removed [AllowAnonymous] - authentication now required
        public async Task<IActionResult> UploadFile2([FromForm] UploadDto uploadDto, StoreType storeType = StoreType.LOCAL)
        {
            var file = uploadDto.file;

            if (file == null) throw new CustomException(ResultCode.PARAM_ERROR, "Upload file cannot be empty");

            // SECURITY FIX: Use whitelist-based validation with magic byte verification
            var validationResult = SecureFileValidator.ValidateFile(file, OptionsSetting.Upload.MaxSize * 1024 * 1024);
            if (!validationResult.IsValid)
            {
                logger.Warn($"File upload rejected: {validationResult.ErrorMessage} - Filename: {file.FileName}, User: {HttpContext.GetName()}");
                return ToResponse(ResultCode.CUSTOM_ERROR, $"Upload failed: {validationResult.ErrorMessage}");
            }

            SysFile sysfile = new();
            IFormFile formFile = file;
            string fileExt = Path.GetExtension(formFile.FileName);//File extension
            double fileSize = Math.Round(formFile.Length / 1024.0, 2);//File size in KB

            // SECURITY: Use secure filename from validator
            if (uploadDto.FileNameType == 1)
            {
                uploadDto.FileName = Path.GetFileNameWithoutExtension(formFile.FileName);
            }
            else if (uploadDto.FileNameType == 3)
            {
                uploadDto.FileName = validationResult.SecureFileName.Replace(fileExt, ""); // Use crypto-secure name
            }
            switch (storeType)
            {
                case StoreType.LOCAL:
                    // SECURITY: Store uploads OUTSIDE wwwroot in secure-uploads directory
                    string savePath = Path.Combine(WebHostEnvironment.ContentRootPath, "secure-uploads");
                    if (uploadDto.FileDir.IsEmpty())
                    {
                        uploadDto.FileDir = ""; // Don't use wwwroot path
                    }
                    sysfile = await SysFileService.SaveFileToLocal(savePath, uploadDto, HttpContext.GetName(), formFile);
                    break;
                case StoreType.REMOTE:
                    break;
                case StoreType.ALIYUN:
                    int AlimaxContentLength = OptionsSetting.ALIYUN_OSS.MaxSize;
                    if (OptionsSetting.ALIYUN_OSS.REGIONID.IsEmpty())
                    {
                        return ToResponse(ResultCode.CUSTOM_ERROR, "Configuration file missing");
                    }
                    if ((fileSize / 1024) > AlimaxContentLength)
                    {
                        return ToResponse(ResultCode.CUSTOM_ERROR, "Upload file too large, cannot exceed " + AlimaxContentLength + " MB");
                    }
                    sysfile = new(formFile.FileName, uploadDto.FileName, fileExt, fileSize + "kb", uploadDto.FileDir, HttpContext.GetName())
                    {
                        StoreType = (int)StoreType.ALIYUN,
                        FileType = formFile.ContentType,
                        ClassifyType = uploadDto.ClassifyType,
                        CategoryId = uploadDto.CategoryId,
                    };
                    sysfile = await SysFileService.SaveFileToAliyun(sysfile, uploadDto, formFile);

                    if (sysfile.Id <= 0) { return ToResponse(ApiResult.Error("Aliyun connection failed")); }
                    break;
                case StoreType.TENCENT:
                    break;
                case StoreType.QINIU:
                    break;
                default:
                    break;
            }
            return SUCCESS(new
            {
                url = sysfile.AccessUrl,
                fileName = sysfile.FileName,
                fileId = sysfile.Id.ToString()
            });
        }

        /// <summary>
        /// Batch upload files (supports images, documents and other types)
        /// SECURED: Requires authentication, uses whitelist validation
        /// </summary>
        /// <param name="model">Upload model containing files and metadata</param>
        /// <returns></returns>
        [HttpPost]
        [ActionPermissionFilter(Permission = "common")]
        // SECURITY FIX: Removed [AllowAnonymous] - authentication now required
        public async Task<IActionResult> UploadFiles([FromForm] UploadDto model)
        {

            var classifyType = model.ClassifyType;
            var categoryId = model.CategoryId;
            var storeType = model.StoreType;
            var files = model.files;
            if (files == null || files.Count == 0)
            {
                return ToResponse(ResultCode.PARAM_ERROR, "Upload files cannot be empty");
            }
            string AccessUrl = "";


            var uploadResults = new List<UploadResult>();

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                string fileExt = Path.GetExtension(file.FileName);
                double fileSize = Math.Round(file.Length / 1024.0, 2);

                // SECURITY FIX: Use whitelist-based validation with magic byte verification
                var batchValidation = SecureFileValidator.ValidateFile(file, OptionsSetting.Upload.MaxSize * 1024 * 1024);
                if (!batchValidation.IsValid)
                {
                    logger.Warn($"Batch file upload rejected: {batchValidation.ErrorMessage} - Filename: {file.FileName}, User: {HttpContext.GetName()}");
                    uploadResults.Add(new()
                    {
                        Success = false,
                        FileName = file.FileName,
                        Error = batchValidation.ErrorMessage
                    });
                    continue;
                }

                try
                {
                    var uploadDto = new UploadDto
                    {
                        FileName = SysFileService.HashFileName(),
                        FileNameType = 3,
                        FileDir = OptionsSetting.Upload.LocalSavePath,
                        ClassifyType = classifyType,
                        CategoryId = categoryId
                    };

                    SysFile sysfile = new();

                    switch (storeType)
                    {
                        case StoreType.LOCAL:
                            // SECURITY: Store uploads OUTSIDE wwwroot in secure-uploads directory (same as single upload)
                            string savePath = Path.Combine(WebHostEnvironment.ContentRootPath, "secure-uploads");
                            sysfile = await SysFileService.SaveFileToLocal(savePath, uploadDto, HttpContext.GetName(), file);
                            break;
                        case StoreType.ALIYUN:
                            int AlimaxContentLength = OptionsSetting.ALIYUN_OSS.MaxSize;
                            if (OptionsSetting.ALIYUN_OSS.REGIONID.IsEmpty())
                            {
                                uploadResults.Add(new()
                                {
                                    Success = false,
                                    FileName = file.FileName,
                                    Error = "Aliyun configuration missing"
                                });
                                continue;
                            }
                            if ((fileSize / 1024) > AlimaxContentLength)
                            {
                                uploadResults.Add(new
                                (){
                                    Success = false,
                                    FileName = file.FileName,
                                    Error = $"File too large, cannot exceed {AlimaxContentLength} MB"
                                });
                                continue;
                            }
                            sysfile = new(file.FileName, uploadDto.FileName, fileExt, fileSize + "kb", uploadDto.FileDir, HttpContext.GetName())
                            {
                                StoreType = (int)StoreType.ALIYUN,
                                FileType = file.ContentType,
                                ClassifyType = uploadDto.ClassifyType,
                                CategoryId = uploadDto.CategoryId,
                            };
                            sysfile = await SysFileService.SaveFileToAliyun(sysfile, uploadDto, file);
                            break;
                    }

                    uploadResults.Add(new
                    (){
                        Success = true,
                        Url = sysfile.AccessUrl,
                        FileName = sysfile.FileName,
                        FileId = sysfile.Id.ToString(),
                        OriginalName = file.FileName
                    });
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Failed to upload file: {file.FileName}");
                    uploadResults.Add(new()
                    {
                        Success = false,
                        FileName = file.FileName,
                        Error = "Upload failed"
                    });
                }
            }


            // ???????????URL???
            foreach (var item in uploadResults)
            {
                if (item.Success == true) // ??????????:((dynamic)item).success == true
                {
                    if (!string.IsNullOrEmpty(AccessUrl))
                    {
                        AccessUrl += ",";
                    }
                    AccessUrl += item.Url; // ????????:((dynamic)item).url
                }
            }




            return SUCCESS(new
            {
                url = AccessUrl,
                fileName = AccessUrl,
                fileId = AccessUrl
            });

            return SUCCESS(new
            {
                files = uploadResults,
                successCount = uploadResults.Count(r => ((dynamic)r).success == true),
                failCount = uploadResults.Count(r => ((dynamic)r).success == false)
            });
        }


         #endregion

        /// <summary>
        /// Download file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        [HttpGet]
        [ActionPermissionFilter(Permission = "common")]
        [Log(Title = "Download file", IsSaveResponseData = false)]
        public IActionResult DownloadFile(string? path, long fileId = 0)
        {
            var tempPath = path;
            if (fileId > 0)
            {
                var fileInfo = SysFileService.GetById(fileId);
                if (fileInfo != null)
                {
                    tempPath = fileInfo.FileUrl;
                }
            }
            string fullPath = tempPath;
            //if (tempPath.StartsWith("/"))
            //{
            //    fullPath = Path.Combine(WebHostEnvironment.WebRootPath, tempPath.ReplaceFirst("/", ""));
            //}
            string fileName = Path.GetFileName(fullPath);
            return DownFile(fullPath, fileName);
        }

        /// <summary>
        /// Initialize seed data
        /// </summary>
        /// <param name="clean">Whether to clear data</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [ActionPermissionFilter(Permission = "common")]
        [Log(BusinessType = BusinessType.INSERT, Title = "Initialize data")]
        public IActionResult InitSeedData(bool clean = false)
        {
            if (!WebHostEnvironment.IsDevelopment())
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "Failed to import data, please initialize in development mode");
            }
            var path = Path.Combine(WebHostEnvironment.WebRootPath, "data.xlsx");
            var WebRootPath = Path.Combine(WebHostEnvironment.ContentRootPath );
            SeedDataService seedDataService = new();
            var result = seedDataService.InitSeedData(path, WebRootPath, clean);
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var item in result)
            {
                Console.WriteLine(item);
            }
            Console.ForegroundColor = ConsoleColor.White;
            return SUCCESS(new
            {
                result
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [ActionPermissionFilter(Permission = "common")]
        [Log(BusinessType = BusinessType.INSERT, Title = "Initialize data")]
        public IActionResult UpdateSeedData()
        {
            if (!WebHostEnvironment.IsDevelopment())
            {
                return ToResponse(ResultCode.CUSTOM_ERROR, "Failed to import data, please initialize in development mode");
            }
            var path = Path.Combine(WebHostEnvironment.WebRootPath, "data.xlsx");
            SeedDataService seedDataService = new();

            var sysNotice = MiniExcel.Query<SysNotice>(path, sheetName: "notice").ToList();
            var result = seedDataService.InitNoticeData(sysNotice);

            var sysMenu = MiniExcel.Query<SysMenu>(path, sheetName: "menu").Where(f => f.MenuId >= 1104).ToList();
            var result5 = seedDataService.InitMenuData(sysMenu);

            return SUCCESS(new
            {
                result,
                result5
            });
        }
    }

    public class UploadResult
    {
        /// <summary>
        /// ??????
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// ????URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// ??????
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// ??ID(?????)
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// ?????
        /// </summary>
        public string OriginalName { get; set; }

        /// <summary>
        /// ????(?????)
        /// </summary>
        public string Error { get; set; }
    }
}

