using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using ZR.Model.System;
using ZR.Model.System.Dto;

namespace ZR.Admin.WebApi.Controllers
{
    /// <summary>
    /// File Storage Controller
    /// </summary>
    [Route("tool/file")]
    [ApiExplorerSettings(GroupName = "sys")]
    public class SysFileController : BaseController
    {
        /// <summary>
        /// File storage interface
        /// </summary>
        private readonly ISysFileService _SysFileService;

        public SysFileController(ISysFileService SysFileService)
        {
            _SysFileService = SysFileService;
        }

        /// <summary>
        /// Query file storage list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("list")]
        [ActionPermissionFilter(Permission = "tool:file:list")]
        public IActionResult QuerySysFile([FromQuery] SysFileQueryDto parm)
        {
            var predicate = Expressionable.Create<SysFile>();

            predicate = predicate.AndIF(parm.BeginCreate_time != null, it => it.Create_time >= parm.BeginCreate_time);
            predicate = predicate.AndIF(parm.EndCreate_time != null, it => it.Create_time <= parm.EndCreate_time);
            predicate = predicate.AndIF(parm.StoreType != null, m => m.StoreType == parm.StoreType);
            predicate = predicate.AndIF(parm.FileId != null, m => m.Id == parm.FileId);
            predicate = predicate.AndIF(parm.ClassifyType != null, m => m.ClassifyType == parm.ClassifyType);

            var response = _SysFileService.GetPages(predicate.ToExpression(), parm, x => x.Id, OrderByType.Desc);
            return SUCCESS(response);
        }

        /// <summary>
        /// Query file storage list
        /// </summary>
        /// <param name="parm"></param>
        /// <returns></returns>
        [HttpGet("listByGroup")]
        public IActionResult QueryFile([FromQuery] SysFileQueryDto parm)
        {
            var predicate = Expressionable.Create<SysFile>();

            predicate = predicate.AndIF(parm.StoreType != null, m => m.StoreType == parm.StoreType);
            predicate = predicate.AndIF(parm.FileId != null, m => m.Id == parm.FileId);
            predicate = predicate.AndIF(parm.ClassifyType != null, m => m.ClassifyType == parm.ClassifyType);
            predicate = predicate.AndIF(parm.CategoryId > 0, m => m.CategoryId == parm.CategoryId);
            predicate = predicate.And(m => m.FileType.StartsWith("image/", StringComparison.OrdinalIgnoreCase));

            var response = _SysFileService.GetPages(predicate.ToExpression(), parm, x => x.Id, OrderByType.Desc);
            return SUCCESS(response);
        }

        /// <summary>
        /// Query file storage details
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{Id}")]
        [ActionPermissionFilter(Permission = "tool:file:query")]
        public IActionResult GetSysFile(long Id)
        {
            var response = _SysFileService.GetFirst(x => x.Id == Id);

            return SUCCESS(response);
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [ActionPermissionFilter(Permission = "tool:file:edit")]
        [Log(Title = "File Storage", BusinessType = BusinessType.UPDATE)]
        public IActionResult UpdateFile([FromBody] SysFileDto parm)
        {
            var modal = parm.Adapt<SysFile>().ToUpdate(HttpContext);
            modal.ClassifyType ??= "";
            var response = _SysFileService.UpdateFile(modal);

            return ToResponse(response);
        }

        /// <summary>
        /// Delete file storage
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{ids}")]
        [ActionPermissionFilter(Permission = "tool:file:delete")]
        [Log(Title = "File Storage", BusinessType = BusinessType.DELETE)]
        public IActionResult DeleteSysFile(string ids)
        {
            long[] idsArr = Tools.SpitLongArrary(ids);
            if (idsArr.Length <= 0) { return ToResponse(ApiResult.Error($"Delete failed, ID cannot be empty")); }

            var response = _SysFileService.Delete(idsArr);
            //TODO Delete local resources

            return ToResponse(response);
        }

        /// <summary>
        /// File storage export
        /// </summary>
        /// <returns></returns>
        [Log(BusinessType = BusinessType.EXPORT, IsSaveResponseData = false, Title = "File Storage")]
        [HttpGet("export")]
        [ActionPermissionFilter(Permission = "tool:file:export")]
        public IActionResult Export()
        {
            var list = _SysFileService.GetAll();

            string sFileName = ExportExcel(list, "SysFile", "File Storage");
            return SUCCESS(new { path = "/export/" + sFileName, fileName = sFileName });
        }

        #region File Manager Endpoints

        /// <summary>
        /// Get directory contents (files and folders)
        /// </summary>
        /// <param name="query">Query parameters for browsing</param>
        /// <returns>Directory contents with files and folders</returns>
        [HttpGet("manager/browse")]
        [ActionPermissionFilter(Permission = "tool:file:list")]
        public async Task<IActionResult> BrowseDirectory([FromQuery] ZR.Model.System.Dto.FileManagerQueryDto query)
        {
            try
            {
                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var result = await _SysFileService.GetDirectoryContents(rootPath, query);
                return SUCCESS(result);
            }
            catch (DirectoryNotFoundException ex)
            {
                return ToResponse(ApiResult.Error(ex.Message));
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error browsing directory: {ex.Message}"));
            }
        }

        /// <summary>
        /// Create a new directory
        /// </summary>
        /// <param name="dto">Directory creation parameters</param>
        /// <returns>Created directory info</returns>
        [HttpPost("manager/directory")]
        [ActionPermissionFilter(Permission = "tool:file:add")]
        [Log(Title = "File Manager", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CreateDirectory([FromBody] ZR.Model.System.Dto.CreateDirectoryDto dto)
        {
            try
            {
                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var (success, message, directoryInfo) = await _SysFileService.CreateDirectory(rootPath, dto);

                if (success)
                {
                    return SUCCESS(new { message, data = directoryInfo });
                }

                return ToResponse(ApiResult.Error(message));
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error creating directory: {ex.Message}"));
            }
        }

        /// <summary>
        /// Rename a file or directory
        /// </summary>
        /// <param name="dto">Rename parameters</param>
        /// <returns>Success status</returns>
        [HttpPut("manager/rename")]
        [ActionPermissionFilter(Permission = "tool:file:edit")]
        [Log(Title = "File Manager", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> Rename([FromBody] ZR.Model.System.Dto.RenameDto dto)
        {
            try
            {
                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var (success, message) = await _SysFileService.Rename(rootPath, dto);

                if (success)
                {
                    return SUCCESS(message);
                }

                return ToResponse(ApiResult.Error(message));
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error renaming: {ex.Message}"));
            }
        }

        /// <summary>
        /// Move a file or directory
        /// </summary>
        /// <param name="dto">Move parameters</param>
        /// <returns>Success status</returns>
        [HttpPut("manager/move")]
        [ActionPermissionFilter(Permission = "tool:file:edit")]
        [Log(Title = "File Manager", BusinessType = BusinessType.UPDATE)]
        public async Task<IActionResult> Move([FromBody] ZR.Model.System.Dto.MoveDto dto)
        {
            try
            {
                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var (success, message) = await _SysFileService.Move(rootPath, dto);

                if (success)
                {
                    return SUCCESS(message);
                }

                return ToResponse(ApiResult.Error(message));
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error moving: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete files or directories
        /// </summary>
        /// <param name="dto">Delete parameters</param>
        /// <returns>Success status</returns>
        [HttpDelete("manager/delete")]
        [ActionPermissionFilter(Permission = "tool:file:delete")]
        [Log(Title = "File Manager", BusinessType = BusinessType.DELETE)]
        public async Task<IActionResult> DeleteFileOrDirectory([FromBody] ZR.Model.System.Dto.DeleteDto dto)
        {
            try
            {
                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var (success, message) = await _SysFileService.DeleteFileOrDirectory(rootPath, dto);

                if (success)
                {
                    return SUCCESS(message);
                }

                return ToResponse(ApiResult.Error(message));
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error deleting: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get file or directory information
        /// </summary>
        /// <param name="path">Relative path to the file or directory</param>
        /// <returns>File/directory information</returns>
        [HttpGet("manager/info")]
        [ActionPermissionFilter(Permission = "tool:file:query")]
        public async Task<IActionResult> GetFileInfo([FromQuery] string path)
        {
            try
            {
                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var result = await _SysFileService.GetFileInfo(rootPath, path ?? "");

                if (result != null)
                {
                    return SUCCESS(result);
                }

                return ToResponse(ApiResult.Error("File or directory not found"));
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error getting file info: {ex.Message}"));
            }
        }

        /// <summary>
        /// Download a file
        /// </summary>
        /// <param name="path">Relative path to the file</param>
        /// <returns>File download stream</returns>
        [HttpGet("manager/download")]
        [ActionPermissionFilter(Permission = "tool:file:query")]
        public async Task<IActionResult> DownloadFile([FromQuery] string path)
        {
            try
            {
                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var (stream, fileName, contentType) = await _SysFileService.DownloadFile(rootPath, path);

                return File(stream, contentType, fileName);
            }
            catch (FileNotFoundException ex)
            {
                return ToResponse(ApiResult.Error($"File not found: {ex.Message}"));
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error downloading file: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get disk space information
        /// </summary>
        /// <returns>Disk space details</returns>
        [HttpGet("manager/diskspace")]
        [ActionPermissionFilter(Permission = "tool:file:query")]
        public async Task<IActionResult> GetDiskSpace()
        {
            try
            {
                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var result = await _SysFileService.GetDiskSpace(rootPath);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error getting disk space: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get sizes for multiple files/directories
        /// </summary>
        /// <param name="dto">Paths to calculate sizes for</param>
        /// <returns>Total size and individual sizes</returns>
        [HttpPost("manager/sizes")]
        [ActionPermissionFilter(Permission = "tool:file:query")]
        public async Task<IActionResult> GetItemsSizes([FromBody] ZR.Model.System.Dto.GetSizesDto dto)
        {
            try
            {
                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var result = await _SysFileService.GetItemsSizes(rootPath, dto);
                return SUCCESS(result);
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error getting sizes: {ex.Message}"));
            }
        }

        /// <summary>
        /// Upload multiple files
        /// </summary>
        /// <param name="dto">Upload parameters with multiple files</param>
        /// <returns>List of uploaded files</returns>
        [HttpPost("manager/upload")]
        [ActionPermissionFilter(Permission = "tool:file:add")]
        [Log(Title = "File Manager", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> UploadMultipleFiles([FromForm]  MultiUploadDto dto)
        {
            try
            {
                if (dto.files == null || dto.files.Count == 0)
                {
                    return ToResponse(ApiResult.Error("No files provided"));
                }

                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var userName = HttpContext.GetName();

                var uploadedFiles = await _SysFileService.UploadMultipleFiles(rootPath, dto, userName);

                return SUCCESS(new
                {
                    count = uploadedFiles.Count,
                    files = uploadedFiles
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error uploading files: {ex.Message}"));
            }
        }

        /// <summary>
        /// Compress files/directories to ZIP
        /// </summary>
        /// <param name="dto">Compression parameters</param>
        /// <returns>Created ZIP file info</returns>
        [HttpPost("manager/compress")]
        [ActionPermissionFilter(Permission = "tool:file:add")]
        [Log(Title = "File Manager - Compress", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> CompressToZip([FromBody] ZR.Model.System.Dto.CompressDto dto)
        {
            try
            {
                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var (success, message, zipFile) = await _SysFileService.CompressToZip(rootPath, dto);

                if (success)
                {
                    return SUCCESS(new { message, zipFile });
                }

                return ToResponse(ApiResult.Error(message));
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error compressing files: {ex.Message}"));
            }
        }

        /// <summary>
        /// Extract archive file (ZIP, RAR, 7z, etc.)
        /// </summary>
        /// <param name="dto">Extraction parameters</param>
        /// <returns>Extraction result</returns>
        [HttpPost("manager/extract")]
        [ActionPermissionFilter(Permission = "tool:file:add")]
        [Log(Title = "File Manager - Extract", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> ExtractArchive([FromBody] ZR.Model.System.Dto.ExtractDto dto)
        {
            try
            {
                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var (success, message, extractedPath) = await _SysFileService.ExtractArchive(rootPath, dto);

                if (success)
                {
                    return SUCCESS(new { message, extractedPath });
                }

                return ToResponse(ApiResult.Error(message));
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error extracting archive: {ex.Message}"));
            }
        }

        /// <summary>
        /// Upload and auto-extract archive
        /// </summary>
        /// <param name="dto">Upload with extract parameters</param>
        /// <returns>Upload and extraction results</returns>
        [HttpPost("manager/upload-extract")]
        [ActionPermissionFilter(Permission = "tool:file:add")]
        [Log(Title = "File Manager - Upload & Extract", BusinessType = BusinessType.INSERT)]
        public async Task<IActionResult> UploadAndExtract([FromForm] ZR.Model.System.Dto.UploadWithExtractDto dto)
        {
            try
            {
                if (dto.files == null || dto.files.Count == 0)
                {
                    return ToResponse(ApiResult.Error("No files provided"));
                }

                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var userName = HttpContext.GetName();

                var uploadedFiles = await _SysFileService.UploadMultipleFiles(rootPath, dto, userName);

                if (!dto.AutoExtract)
                {
                    return SUCCESS(new
                    {
                        count = uploadedFiles.Count,
                        files = uploadedFiles
                    });
                }

                // Auto-extract archive files
                var extractedResults = new List<object>();
                foreach (var file in uploadedFiles)
                {
                    if (_SysFileService.IsArchiveFile(file.FileExt))
                    {
                        var extractDto = new ZR.Model.System.Dto.ExtractDto
                        {
                            ArchivePath = file.StorePath + "/" + file.FileName,
                            Overwrite = dto.AllowOverwrite,
                            DeleteAfterExtract = dto.DeleteAfterExtract
                        };

                        var (success, message, extractedPath) = await _SysFileService.ExtractArchive(rootPath, extractDto);
                        extractedResults.Add(new
                        {
                            fileName = file.FileName,
                            success,
                            message,
                            extractedPath
                        });
                    }
                }

                return SUCCESS(new
                {
                    count = uploadedFiles.Count,
                    files = uploadedFiles,
                    extracted = extractedResults
                });
            }
            catch (Exception ex)
            {
                return ToResponse(ApiResult.Error($"Error uploading and extracting: {ex.Message}"));
            }
        }

        #endregion
    }
}
