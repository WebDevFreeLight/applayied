using Microsoft.AspNetCore.Http;
using ZR.Model.Dto;
using ZR.Model.System;
using ZR.Model.System.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZR.ServiceCore.Services
{
    public interface ISysFileService : IBaseService<SysFile>
    {
        Task<long> InsertFile(SysFile file);

        /// <summary>
        /// Upload file to local storage
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="rootPath"></param>
        /// <param name="userName"></param>
        /// <param name="dto"></param>
        /// <returns>File object</returns>
        Task<SysFile> SaveFileToLocal(string rootPath, UploadDto dto, string userName, IFormFile formFile);

        /// <summary>
        /// Upload file to Aliyun
        /// </summary>
        /// <param name="file"></param>
        /// <param name="dto"></param>
        /// <param name="formFile"></param>
        /// <returns></returns>
        Task<SysFile> SaveFileToAliyun(SysFile file, UploadDto dto, IFormFile formFile);

        /// <summary>
        /// Create folder by time
        /// </summary>
        /// <param name="path"></param>
        /// <param name="byTimeStore"></param>
        /// <returns>eg: 2020/11/3</returns>
        string GetdirPath(string path = "", bool byTimeStore = true);

        /// <summary>
        /// Get MD5 value of filename (16 bits)
        /// </summary>
        /// <param name="str">Filename, excluding extension</param>
        /// <returns></returns>
        string HashFileName(string str = null);

        int UpdateFile(SysFile parm);

        #region File Manager Operations

        /// <summary>
        /// Get directory contents (files and subdirectories)
        /// </summary>
        /// <param name="rootPath">Root directory path</param>
        /// <param name="query">Query parameters</param>
        /// <returns>Directory contents with files and folders</returns>
        Task<DirectoryContentsDto> GetDirectoryContents(string rootPath, FileManagerQueryDto query);

        /// <summary>
        /// Create a new directory
        /// </summary>
        /// <param name="rootPath">Root directory path</param>
        /// <param name="dto">Directory creation parameters</param>
        /// <returns>Success status and new directory info</returns>
        Task<(bool Success, string Message, FileInfoDto DirectoryInfo)> CreateDirectory(string rootPath, CreateDirectoryDto dto);

        /// <summary>
        /// Rename a file or directory
        /// </summary>
        /// <param name="rootPath">Root directory path</param>
        /// <param name="dto">Rename parameters</param>
        /// <returns>Success status and message</returns>
        Task<(bool Success, string Message)> Rename(string rootPath, RenameDto dto);

        /// <summary>
        /// Move a file or directory
        /// </summary>
        /// <param name="rootPath">Root directory path</param>
        /// <param name="dto">Move parameters</param>
        /// <returns>Success status and message</returns>
        Task<(bool Success, string Message)> Move(string rootPath, MoveDto dto);

        /// <summary>
        /// Delete files or directories
        /// </summary>
        /// <param name="rootPath">Root directory path</param>
        /// <param name="dto">Delete parameters</param>
        /// <returns>Success status and message</returns>
        Task<(bool Success, string Message)> DeleteFileOrDirectory(string rootPath, DeleteDto dto);

        /// <summary>
        /// Get file information
        /// </summary>
        /// <param name="rootPath">Root directory path</param>
        /// <param name="relativePath">Relative path to file</param>
        /// <returns>File information</returns>
        Task<FileInfoDto> GetFileInfo(string rootPath, string relativePath);

        /// <summary>
        /// Download a file
        /// </summary>
        /// <param name="rootPath">Root directory path</param>
        /// <param name="relativePath">Relative path to file</param>
        /// <returns>File stream and metadata</returns>
        Task<(System.IO.Stream Stream, string FileName, string ContentType)> DownloadFile(string rootPath, string relativePath);

        /// <summary>
        /// Format file size to human-readable format
        /// </summary>
        /// <param name="bytes">Size in bytes</param>
        /// <returns>Formatted size string (e.g., "1.5 MB")</returns>
        string FormatFileSize(long bytes);

        /// <summary>
        /// Get disk space information
        /// </summary>
        /// <param name="rootPath">Root directory path</param>
        /// <returns>Disk space information</returns>
        Task<DiskSpaceDto> GetDiskSpace(string rootPath);

        /// <summary>
        /// Upload multiple files
        /// </summary>
        /// <param name="rootPath">Root directory path</param>
        /// <param name="dto">Upload parameters</param>
        /// <param name="userName">User name</param>
        /// <returns>List of uploaded files</returns>
        Task<List<SysFile>> UploadMultipleFiles(string rootPath, MultiUploadDto dto, string userName);

        /// <summary>
        /// Get MIME type from file extension
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <returns>MIME type string</returns>
        string GetMimeType(string extension);

        /// <summary>
        /// Compress files/directories into a ZIP archive
        /// </summary>
        /// <param name="rootPath">Root directory path</param>
        /// <param name="dto">Compression parameters</param>
        /// <returns>Success status, message, and created zip file info</returns>
        Task<(bool Success, string Message, FileInfoDto ZipFile)> CompressToZip(string rootPath, CompressDto dto);

        /// <summary>
        /// Extract archive file (ZIP, RAR, 7z, etc.)
        /// </summary>
        /// <param name="rootPath">Root directory path</param>
        /// <param name="dto">Extraction parameters</param>
        /// <returns>Success status, message, and extraction directory info</returns>
        Task<(bool Success, string Message, string ExtractedPath)> ExtractArchive(string rootPath, ExtractDto dto);

        /// <summary>
        /// Check if file is a compressed archive
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <returns>True if file is an archive</returns>
        bool IsArchiveFile(string extension);

        /// <summary>
        /// Get sizes for multiple files/directories
        /// </summary>
        /// <param name="rootPath">Root directory path</param>
        /// <param name="dto">Paths to calculate sizes for</param>
        /// <returns>Total size and individual item sizes</returns>
        Task<ItemsSizesDto> GetItemsSizes(string rootPath, GetSizesDto dto);

        #endregion
    }
}

