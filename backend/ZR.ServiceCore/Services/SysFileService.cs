using Infrastructure;
using Infrastructure.Attribute;
using Infrastructure.Enums;
using Infrastructure.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.IO.Compression;
using SevenZipExtractor;
using ZR.Common;
using ZR.Model.Dto;
using ZR.Model.System;
using ZR.Model.System.Dto;

namespace ZR.ServiceCore.Services
{
    /// <summary>
    /// ???? 4
    /// </summary>
    [AppService(ServiceType = typeof(ISysFileService), ServiceLifetime = LifeTime.Transient)]
    public class SysFileService : BaseService<SysFile>, ISysFileService
    {
        private string domainUrl = AppSettings.GetConfig("ALIYUN_OSS:domainUrl");
        private readonly ISysConfigService SysConfigService;
        private OptionsSetting OptionsSetting;
        public SysFileService(ISysConfigService sysConfigService, IOptions<OptionsSetting> options)
        {
            SysConfigService = sysConfigService;
            OptionsSetting = options.Value;
        }

        /// <summary>
        /// ????
        /// </summary>
        /// <param name="rootPath">?????</param>
        /// <param name="formFile">??????</param>
        /// <param name="clasifyType">????</param>
        /// <param name="userName"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<SysFile> SaveFileToLocal(string rootPath, UploadDto dto, string userName, string clasifyType, IFormFile formFile)
        {
            var fileName = dto.FileName;
            var fileDir = dto.FileDir;
            string fileExt = Path.GetExtension(formFile.FileName);
            fileName = (fileName.IsEmpty() ? HashFileName() : fileName) + fileExt;

            string filePath = GetdirPath(fileDir);
            string finalFilePath = Path.Combine(rootPath, filePath, fileName);
            double fileSize = Math.Round(formFile.Length / 1024.0, 2);

            if (!Directory.Exists(Path.GetDirectoryName(finalFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(finalFilePath));
            }
            // ????????
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

            // ????????????????
            bool isImageByExtension = imageExtensions.Contains(fileExt);
            if (dto.Quality > 0 && isImageByExtension)
            {
                await SaveCompressedImageAsync(formFile, finalFilePath);
            }
            else
            {
                using (var stream = new FileStream(finalFilePath, FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                }
            }
            string accessPath = string.Concat(filePath.Replace("\\", "/"), "/", fileName);
            // Store relative path instead of absolute URL so it works across environments
            string relativeUrl = "/" + accessPath.TrimStart('/');
            SysFile file = new(formFile.FileName, fileName, fileExt, fileSize + "kb", filePath, userName)
            {
                StoreType = (int)StoreType.LOCAL,
                FileType = formFile.ContentType,
                FileUrl = finalFilePath.Replace("\\", "/"),
                AccessUrl = relativeUrl,
                ClassifyType = clasifyType,
                CategoryId = dto.CategoryId,
            };
            file.Id = await InsertFile(file);
            return file;
        }
        //public async Task<SysFile> SaveFileToLocal(string rootPath, string fileName, string fileDir, string userName, IFormFile formFile)
        //{
        //    return await SaveFileToLocal(rootPath, fileName, fileDir, userName, string.Empty, formFile);
        //}
        public async Task<SysFile> SaveFileToLocal(string rootPath, UploadDto dto, string userName, IFormFile formFile)
        {
            return await SaveFileToLocal(rootPath, dto, userName, dto.ClassifyType, formFile);
        }
        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="file"></param>
        /// <param name="dto"></param>
        /// <param name="formFile"></param>
        /// <returns></returns>
        public async Task<SysFile> SaveFileToAliyun(SysFile file, UploadDto dto, IFormFile formFile)
        {
            file.FileName = (file.FileName.IsEmpty() ? HashFileName() : file.FileName) + file.FileExt;
            file.StorePath = GetdirPath(file.StorePath);
            string finalPath = Path.Combine(file.StorePath, file.FileName);
            HttpStatusCode statusCode;
            if (dto.Quality > 0)
            {
                // ????
                using var stream = new MemoryStream();
                await CompressImageAsync(formFile, stream, dto.Quality);
                stream.Position = 0;
                statusCode = AliyunOssHelper.PutObjectFromFile(stream, finalPath, "");
            }
            else
            {
                statusCode = AliyunOssHelper.PutObjectFromFile(formFile.OpenReadStream(), finalPath, "");
            }
            if (statusCode != HttpStatusCode.OK) return file;

            file.StorePath = file.StorePath;
            file.FileUrl = finalPath;
            file.AccessUrl = string.Concat(domainUrl, "/", file.StorePath.Replace("\\", "/"), "/", file.FileName);
            file.Id = await InsertFile(file);

            return file;
        }

        /// <summary>
        /// ????????
        /// </summary>
        /// <param name="storePath"></param>
        /// <param name="byTimeStore">????????</param>
        /// <returns></returns>
        public string GetdirPath(string storePath = "", bool byTimeStore = true)
        {
            DateTime date = DateTime.Now;
            string timeDir = date.ToString("yyyy/MMdd");

            if (!string.IsNullOrEmpty(storePath))
            {
                timeDir = Path.Combine(storePath, timeDir);//5
            }
            Console.WriteLine("File storage directory: " + timeDir);
            return timeDir.Replace("\\", "/");
        }

        public string HashFileName(string str = null)
        {
            if (string.IsNullOrEmpty(str))
            {
                str = Guid.NewGuid().ToString().ToLower();
            }
            return BitConverter.ToString(MD5.HashData(Encoding.Default.GetBytes(str)), 4, 8).Replace("-", "").ToLower();
        }

        public Task<long> InsertFile(SysFile file)
        {
            return Insertable(file).ExecuteReturnSnowflakeIdAsync();//????????ID;
        }

        /// <summary>
        /// ???????
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int UpdateFile(SysFile model)
        {
            return Update(model, t => new { t.ClassifyType, }, true);
        }

        /// <summary>
        /// ????
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="finalFilePath"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        public static async Task SaveCompressedImageAsync(IFormFile formFile, string finalFilePath, int quality = 75)
        {
            using (var image = await Image.LoadAsync(formFile.OpenReadStream()))
            {
                // ?????????(??)
                //image.Mutate(x => x.Resize(new ResizeOptions
                //{
                //    Mode = ResizeMode.Max,
                //    Size = new Size(1920, 1080) // ??????,??????
                //}));

                // ?????? JPEG
                var encoder = new JpegEncoder { Quality = quality }; // ??????????

                await using (var stream = new FileStream(finalFilePath, FileMode.Create))
                {
                    await image.SaveAsync(stream, encoder);
                }
            }
        }
        private async Task CompressImageAsync(IFormFile file, Stream outputStream, int quality)
        {
            // ????
            using var image = await Image.LoadAsync(file.OpenReadStream());

            // ?? JPEG ????????
            var encoder = new JpegEncoder { Quality = quality };

            // ????????????
            await image.SaveAsync(outputStream, encoder);
        }

        #region File Manager Operations

        /// <summary>
        /// Get directory contents
        /// </summary>
        public async Task<DirectoryContentsDto> GetDirectoryContents(string rootPath, FileManagerQueryDto query)
        {
            await Task.CompletedTask; // For async consistency

            var relativePath = query.Path?.Trim() ?? "";
            var fullPath = Path.Combine(rootPath, relativePath);

            if (!Directory.Exists(fullPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {relativePath}");
            }

            var result = new DirectoryContentsDto
            {
                CurrentPath = relativePath,
                ParentPath = GetParentPath(relativePath)
            };

            // Build breadcrumbs
            result.Breadcrumbs = BuildBreadcrumbs(relativePath);

            // Get directories
            var directories = Directory.GetDirectories(fullPath);
            foreach (var dir in directories)
            {
                var dirInfo = new DirectoryInfo(dir);
                var fileInfoDto = MapDirectoryToFileInfo(dirInfo, rootPath);

                // Apply search filter
                if (!string.IsNullOrEmpty(query.SearchKeyword) &&
                    !fileInfoDto.Name.Contains(query.SearchKeyword, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                result.Directories.Add(fileInfoDto);
            }

            // Get files
            var files = Directory.GetFiles(fullPath);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var fileInfoDto = MapFileToFileInfo(fileInfo, rootPath);

                // Apply search filter
                if (!string.IsNullOrEmpty(query.SearchKeyword) &&
                    !fileInfoDto.Name.Contains(query.SearchKeyword, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Apply file type filter
                if (!string.IsNullOrEmpty(query.FileTypeFilter))
                {
                    if (query.FileTypeFilter.Equals("image", StringComparison.OrdinalIgnoreCase) && !fileInfoDto.IsImage)
                    {
                        continue;
                    }
                    // Add more filter types as needed
                }

                result.Files.Add(fileInfoDto);
            }

            result.TotalCount = result.Directories.Count + result.Files.Count;

            return result;
        }

        /// <summary>
        /// Create a new directory
        /// </summary>
        public async Task<(bool Success, string Message, FileInfoDto DirectoryInfo)> CreateDirectory(string rootPath, CreateDirectoryDto dto)
        {
            await Task.CompletedTask; // For async consistency

            try
            {
                var parentPath = Path.Combine(rootPath, dto.ParentPath?.Trim() ?? "");
                var newDirPath = Path.Combine(parentPath, dto.DirectoryName);

                if (Directory.Exists(newDirPath))
                {
                    return (false, "Directory already exists", null);
                }

                if (string.IsNullOrWhiteSpace(dto.DirectoryName) ||
                    dto.DirectoryName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    return (false, "Invalid directory name", null);
                }

                Directory.CreateDirectory(newDirPath);
                var dirInfo = new DirectoryInfo(newDirPath);
                var fileInfoDto = MapDirectoryToFileInfo(dirInfo, rootPath);

                return (true, "Directory created successfully", fileInfoDto);
            }
            catch (Exception ex)
            {
                return (false, $"Error creating directory: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Rename a file or directory
        /// </summary>
        public async Task<(bool Success, string Message)> Rename(string rootPath, RenameDto dto)
        {
            await Task.CompletedTask; // For async consistency

            try
            {
                var sourcePath = Path.Combine(rootPath, dto.CurrentPath);
                var sourceDir = Path.GetDirectoryName(sourcePath);
                var newPath = Path.Combine(sourceDir, dto.NewName);

                if (string.IsNullOrWhiteSpace(dto.NewName) ||
                    dto.NewName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    return (false, "Invalid name");
                }

                if (dto.IsDirectory)
                {
                    if (!Directory.Exists(sourcePath))
                    {
                        return (false, "Source directory not found");
                    }
                    if (Directory.Exists(newPath))
                    {
                        return (false, "A directory with the new name already exists");
                    }
                    Directory.Move(sourcePath, newPath);
                }
                else
                {
                    if (!File.Exists(sourcePath))
                    {
                        return (false, "Source file not found");
                    }
                    if (File.Exists(newPath))
                    {
                        return (false, "A file with the new name already exists");
                    }
                    File.Move(sourcePath, newPath);
                }

                return (true, "Renamed successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error renaming: {ex.Message}");
            }
        }

        /// <summary>
        /// Move a file or directory
        /// </summary>
        public async Task<(bool Success, string Message)> Move(string rootPath, MoveDto dto)
        {
            await Task.CompletedTask; // For async consistency

            try
            {
                var sourcePath = Path.Combine(rootPath, dto.SourcePath);
                var destDir = Path.Combine(rootPath, dto.DestinationPath);
                var fileName = Path.GetFileName(sourcePath);
                var destPath = Path.Combine(destDir, fileName);

                if (!Directory.Exists(destDir))
                {
                    return (false, "Destination directory not found");
                }

                if (dto.IsDirectory)
                {
                    if (!Directory.Exists(sourcePath))
                    {
                        return (false, "Source directory not found");
                    }
                    if (Directory.Exists(destPath))
                    {
                        return (false, "A directory with the same name already exists in destination");
                    }
                    Directory.Move(sourcePath, destPath);
                }
                else
                {
                    if (!File.Exists(sourcePath))
                    {
                        return (false, "Source file not found");
                    }
                    if (File.Exists(destPath))
                    {
                        return (false, "A file with the same name already exists in destination");
                    }
                    File.Move(sourcePath, destPath);
                }

                return (true, "Moved successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error moving: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete files or directories
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteFileOrDirectory(string rootPath, DeleteDto dto)
        {
            await Task.CompletedTask; // For async consistency

            try
            {
                int deletedCount = 0;
                foreach (var relativePath in dto.Paths)
                {
                    var fullPath = Path.Combine(rootPath, relativePath);

                    if (dto.IsDirectory)
                    {
                        if (Directory.Exists(fullPath))
                        {
                            Directory.Delete(fullPath, recursive: true);
                            deletedCount++;
                        }
                    }
                    else
                    {
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                            deletedCount++;
                        }
                    }
                }

                return (true, $"Deleted {deletedCount} item(s) successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting: {ex.Message}");
            }
        }

        /// <summary>
        /// Get file information
        /// </summary>
        public async Task<FileInfoDto> GetFileInfo(string rootPath, string relativePath)
        {
            await Task.CompletedTask; // For async consistency

            var fullPath = Path.Combine(rootPath, relativePath);

            if (Directory.Exists(fullPath))
            {
                var dirInfo = new DirectoryInfo(fullPath);
                return MapDirectoryToFileInfo(dirInfo, rootPath);
            }
            else if (File.Exists(fullPath))
            {
                var fileInfo = new FileInfo(fullPath);
                return MapFileToFileInfo(fileInfo, rootPath);
            }

            return null;
        }

        /// <summary>
        /// Download a file
        /// </summary>
        public async Task<(Stream Stream, string FileName, string ContentType)> DownloadFile(string rootPath, string relativePath)
        {
            var fullPath = Path.Combine(rootPath, relativePath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("File not found", relativePath);
            }

            var fileInfo = new FileInfo(fullPath);
            var memoryStream = new MemoryStream();

            using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                await fileStream.CopyToAsync(memoryStream);
            }

            memoryStream.Position = 0;
            var contentType = GetMimeType(fileInfo.Extension);

            return (memoryStream, fileInfo.Name, contentType);
        }

        /// <summary>
        /// Format file size to human-readable format
        /// </summary>
        public string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Get disk space information
        /// </summary>
        public async Task<DiskSpaceDto> GetDiskSpace(string rootPath)
        {
            await Task.CompletedTask; // For async consistency

            try
            {
                var driveInfo = new DriveInfo(Path.GetPathRoot(rootPath));

                var result = new DiskSpaceDto
                {
                    TotalSpace = driveInfo.TotalSize,
                    FreeSpace = driveInfo.AvailableFreeSpace,
                    UsedSpace = driveInfo.TotalSize - driveInfo.AvailableFreeSpace
                };

                result.FormattedTotal = FormatFileSize(result.TotalSpace);
                result.FormattedFree = FormatFileSize(result.FreeSpace);
                result.FormattedUsed = FormatFileSize(result.UsedSpace);
                result.UsagePercentage = Math.Round((double)result.UsedSpace / result.TotalSpace * 100, 2);

                return result;
            }
            catch (Exception)
            {
                return new DiskSpaceDto();
            }
        }

        /// <summary>
        /// Upload multiple files
        /// </summary>
        public async Task<List<SysFile>> UploadMultipleFiles(string rootPath, MultiUploadDto dto, string userName)
        {
            var uploadedFiles = new List<SysFile>();

            if (dto.files != null && dto.files.Count > 0)
            {
                foreach (var file in dto.files)
                {
                    var uploadDto = new UploadDto
                    {
                        FileName = dto.FileName,
                        FileDir = dto.FileDir,
                        FileNameType = dto.FileNameType,
                        ClassifyType = dto.ClassifyType,
                        CategoryId = dto.CategoryId,
                        Quality = dto.Quality,
                        UserName = userName,
                        file = file
                    };

                    var uploadedFile = await SaveFileToLocal(rootPath, uploadDto, userName, file);
                    uploadedFiles.Add(uploadedFile);
                }
            }

            return uploadedFiles;
        }

        /// <summary>
        /// Get MIME type from file extension
        /// </summary>
        public string GetMimeType(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return "application/octet-stream";

            extension = extension.ToLowerInvariant();

            var mimeTypes = new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"},
                {".zip", "application/zip"},
                {".rar", "application/x-rar-compressed"},
                {".7z", "application/x-7z-compressed"},
                {".tar", "application/x-tar"},
                {".gz", "application/gzip"},
                {".bz2", "application/x-bzip2"},
                {".xz", "application/x-xz"},
                {".cab", "application/vnd.ms-cab-compressed"},
                {".iso", "application/x-iso9660-image"},
                {".jar", "application/java-archive"},
                {".apk", "application/vnd.android.package-archive"},
                {".war", "application/java-archive"},
                {".ear", "application/java-archive"},
                {".mp3", "audio/mpeg"},
                {".mp4", "video/mp4"},
                {".avi", "video/x-msvideo"},
                {".json", "application/json"},
                {".xml", "application/xml"},
                {".svg", "image/svg+xml"},
                {".webp", "image/webp"}
            };

            return mimeTypes.ContainsKey(extension) ? mimeTypes[extension] : "application/octet-stream";
        }

        #endregion

        #region Private Helper Methods

        private string GetParentPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            var parentPath = Path.GetDirectoryName(path);
            return string.IsNullOrEmpty(parentPath) ? "" : parentPath.Replace("\\", "/");
        }

        private List<BreadcrumbItem> BuildBreadcrumbs(string currentPath)
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Name = "Root", Path = "" }
            };

            if (string.IsNullOrWhiteSpace(currentPath))
                return breadcrumbs;

            var parts = currentPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            var pathBuilder = "";

            foreach (var part in parts)
            {
                pathBuilder = string.IsNullOrEmpty(pathBuilder) ? part : $"{pathBuilder}/{part}";
                breadcrumbs.Add(new BreadcrumbItem
                {
                    Name = part,
                    Path = pathBuilder
                });
            }

            return breadcrumbs;
        }

        private FileInfoDto MapDirectoryToFileInfo(DirectoryInfo dirInfo, string rootPath)
        {
            var relativePath = GetRelativePath(rootPath, dirInfo.FullName);

            return new FileInfoDto
            {
                Name = dirInfo.Name,
                Path = dirInfo.FullName,
                RelativePath = relativePath,
                Extension = "",
                Size = 0,
                FormattedSize = "-",
                IsDirectory = true,
                CreatedDate = dirInfo.CreationTime,
                ModifiedDate = dirInfo.LastWriteTime,
                MimeType = "folder",
                IsImage = false,
                Url = "",
                ThumbnailUrl = ""
            };
        }

        private FileInfoDto MapFileToFileInfo(FileInfo fileInfo, string rootPath)
        {
            var relativePath = GetRelativePath(rootPath, fileInfo.FullName);
            var extension = fileInfo.Extension.ToLowerInvariant();
            var isImage = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" }.Contains(extension);

            // Store relative path instead of absolute URL so it works across environments
            string relativeUrl = "/" + relativePath.Replace("\\", "/").TrimStart('/');

            return new FileInfoDto
            {
                Name = fileInfo.Name,
                Path = fileInfo.FullName,
                RelativePath = relativePath,
                Extension = extension,
                Size = fileInfo.Length,
                FormattedSize = FormatFileSize(fileInfo.Length),
                IsDirectory = false,
                CreatedDate = fileInfo.CreationTime,
                ModifiedDate = fileInfo.LastWriteTime,
                MimeType = GetMimeType(extension),
                IsImage = isImage,
                Url = relativeUrl,
                ThumbnailUrl = isImage ? relativeUrl : ""
            };
        }

        private string GetRelativePath(string rootPath, string fullPath)
        {
            if (fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(rootPath.Length).TrimStart('\\', '/').Replace("\\", "/");
            }
            return fullPath;
        }

        #endregion

        #region Compression Operations

        /// <summary>
        /// Compress files/directories into ZIP archive
        /// </summary>
        public async Task<(bool Success, string Message, FileInfoDto ZipFile)> CompressToZip(string rootPath, CompressDto dto)
        {
            await Task.CompletedTask; // For async consistency

            try
            {
                if (dto.Paths == null || dto.Paths.Count == 0)
                {
                    return (false, "No files or directories specified for compression", null);
                }

                // Determine target directory
                var targetDir = string.IsNullOrEmpty(dto.TargetDirectory)
                    ? rootPath
                    : Path.Combine(rootPath, dto.TargetDirectory);

                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                // Generate zip file name
                var zipFileName = string.IsNullOrEmpty(dto.ZipFileName)
                    ? $"archive_{DateTime.Now:yyyyMMdd_HHmmss}.zip"
                    : dto.ZipFileName;

                if (!zipFileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    zipFileName += ".zip";
                }

                var zipFilePath = Path.Combine(targetDir, zipFileName);

                // Delete existing zip if it exists
                if (File.Exists(zipFilePath))
                {
                    File.Delete(zipFilePath);
                }

                // Create ZIP archive
                using (var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    foreach (var relativePath in dto.Paths)
                    {
                        var fullPath = Path.Combine(rootPath, relativePath);

                        if (Directory.Exists(fullPath))
                        {
                            // Add directory and all its contents
                            AddDirectoryToZip(archive, fullPath, Path.GetFileName(fullPath));
                        }
                        else if (File.Exists(fullPath))
                        {
                            // Add single file
                            var entryName = Path.GetFileName(fullPath);
                            archive.CreateEntryFromFile(fullPath, entryName, CompressionLevel.Optimal);
                        }
                    }
                }

                // Get file info for the created zip
                var zipFileInfo = new FileInfo(zipFilePath);
                var zipFileDto = MapFileToFileInfo(zipFileInfo, rootPath);

                return (true, $"Created archive: {zipFileName}", zipFileDto);
            }
            catch (Exception ex)
            {
                return (false, $"Error creating archive: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Extract archive file (ZIP, RAR, 7z, etc.)
        /// </summary>
        public async Task<(bool Success, string Message, string ExtractedPath)> ExtractArchive(string rootPath, ExtractDto dto)
        {
            await Task.CompletedTask; // For async consistency

            try
            {
                var archivePath = Path.Combine(rootPath, dto.ArchivePath);

                if (!File.Exists(archivePath))
                {
                    return (false, "Archive file not found", null);
                }

                var archiveExtension = Path.GetExtension(archivePath).ToLowerInvariant();

                // Determine extraction directory
                string extractPath;
                if (!string.IsNullOrEmpty(dto.TargetDirectory))
                {
                    extractPath = Path.Combine(rootPath, dto.TargetDirectory);
                }
                else
                {
                    // Extract to a folder with the same name as the archive (without extension)
                    var archiveName = Path.GetFileNameWithoutExtension(archivePath);
                    var archiveDir = Path.GetDirectoryName(archivePath);
                    extractPath = Path.Combine(archiveDir, archiveName);
                }

                // Create extraction directory if it doesn't exist
                if (!Directory.Exists(extractPath))
                {
                    Directory.CreateDirectory(extractPath);
                }

                // Extract based on file type
                if (archiveExtension == ".zip")
                {
                    // Use built-in ZIP extraction
                    ExtractZipArchive(archivePath, extractPath, dto.Overwrite);
                }
                else
                {
                    // Use SharpCompress for RAR, 7z, tar, etc.
                    ExtractGenericArchive(archivePath, extractPath, dto.Overwrite);
                }

                // Delete archive if requested
                if (dto.DeleteAfterExtract)
                {
                    File.Delete(archivePath);
                }

                var relativePath = GetRelativePath(rootPath, extractPath);
                return (true, $"Extracted to: {relativePath}", relativePath);
            }
            catch (Exception ex)
            {
                return (false, $"Error extracting archive: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Check if file is an archive (SevenZipExtractor supports these formats)
        /// </summary>
        public bool IsArchiveFile(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return false;

            var ext = extension.ToLowerInvariant();
            // SevenZipExtractor supports: 7z, XZ, BZIP2, GZIP, TAR, ZIP, WIM, AR, ARJ, CAB, CHM, CPIO, CramFS, DMG, EXT, FAT, GPT, HFS, IHEX, ISO, LZH, LZMA, MBR, MSI, NSIS, NTFS, QCOW2, RAR, RPM, SquashFS, UDF, UEFI, VDI, VHD, VMDK, WIM, XAR and Z
            var archiveExtensions = new[] {
                ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz",
                ".cab", ".iso", ".wim", ".lzh", ".lzma", ".arj",
                ".chm", ".dmg", ".msi", ".rpm", ".deb", ".jar",
                ".war", ".ear", ".apk"
            };

            return archiveExtensions.Contains(ext);
        }

        #endregion

        #region Private Compression Helpers

        /// <summary>
        /// Add directory and all its contents to ZIP archive recursively
        /// </summary>
        private void AddDirectoryToZip(ZipArchive archive, string directoryPath, string entryPrefix)
        {
            var files = Directory.GetFiles(directoryPath);
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var entryName = Path.Combine(entryPrefix, fileName).Replace("\\", "/");
                archive.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
            }

            var directories = Directory.GetDirectories(directoryPath);
            foreach (var directory in directories)
            {
                var dirName = Path.GetFileName(directory);
                var newPrefix = Path.Combine(entryPrefix, dirName);
                AddDirectoryToZip(archive, directory, newPrefix);
            }
        }

        /// <summary>
        /// Extract ZIP archive
        /// </summary>
        private void ExtractZipArchive(string archivePath, string extractPath, bool overwrite)
        {
            using (var archive = ZipFile.OpenRead(archivePath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        // Directory entry
                        continue;
                    }

                    var destinationPath = Path.Combine(extractPath, entry.FullName);
                    var destinationDir = Path.GetDirectoryName(destinationPath);

                    if (!Directory.Exists(destinationDir))
                    {
                        Directory.CreateDirectory(destinationDir);
                    }

                    if (overwrite || !File.Exists(destinationPath))
                    {
                        entry.ExtractToFile(destinationPath, overwrite);
                    }
                }
            }
        }

        /// <summary>
        /// Extract generic archive using SevenZipExtractor (RAR, 7z, tar, etc.)
        /// </summary>
        private void ExtractGenericArchive(string archivePath, string extractPath, bool overwrite)
        {
            try
            {
                using (ArchiveFile archiveFile = new ArchiveFile(archivePath))
                {
                    foreach (Entry entry in archiveFile.Entries)
                    {
                        // Skip directories
                        if (entry.IsFolder)
                        {
                            continue;
                        }

                        string destinationPath = Path.Combine(extractPath, entry.FileName);
                        string destinationDir = Path.GetDirectoryName(destinationPath);

                        // Create directory if it doesn't exist
                        if (!Directory.Exists(destinationDir))
                        {
                            Directory.CreateDirectory(destinationDir);
                        }

                        // Check if file exists and handle overwrite
                        if (File.Exists(destinationPath))
                        {
                            if (overwrite)
                            {
                                File.Delete(destinationPath);
                            }
                            else
                            {
                                continue; // Skip this file
                            }
                        }

                        // Extract the file
                        entry.Extract(destinationPath);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error extracting archive with SevenZipExtractor: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get sizes for multiple files/directories
        /// </summary>
        public async Task<ItemsSizesDto> GetItemsSizes(string rootPath, GetSizesDto dto)
        {
            await Task.CompletedTask;

            var result = new ItemsSizesDto
            {
                Items = new List<ItemSizeDto>()
            };

            long totalSize = 0;

            foreach (var relativePath in dto.Paths)
            {
                try
                {
                    var fullPath = Path.Combine(rootPath, relativePath);

                    long itemSize = 0;

                    if (Directory.Exists(fullPath))
                    {
                        // Calculate directory size recursively
                        itemSize = GetDirectorySize(fullPath);
                    }
                    else if (File.Exists(fullPath))
                    {
                        // Get file size
                        var fileInfo = new FileInfo(fullPath);
                        itemSize = fileInfo.Length;
                    }

                    totalSize += itemSize;

                    result.Items.Add(new ItemSizeDto
                    {
                        Path = relativePath,
                        Size = itemSize,
                        FormattedSize = FormatFileSize(itemSize)
                    });
                }
                catch (Exception)
                {
                    // If we can't get size, add 0
                    result.Items.Add(new ItemSizeDto
                    {
                        Path = relativePath,
                        Size = 0,
                        FormattedSize = "0 B"
                    });
                }
            }

            result.TotalSize = totalSize;
            result.FormattedSize = FormatFileSize(totalSize);

            return result;
        }

        /// <summary>
        /// Calculate directory size recursively
        /// </summary>
        private long GetDirectorySize(string directoryPath)
        {
            long size = 0;

            try
            {
                // Get all files in directory
                var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        size += fileInfo.Length;
                    }
                    catch
                    {
                        // Skip files we can't access
                        continue;
                    }
                }
            }
            catch
            {
                // If we can't access directory, return 0
                return 0;
            }

            return size;
        }

        #endregion
    }
}

