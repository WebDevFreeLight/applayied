using System;
using System.Collections.Generic;
using ZR.Model.Dto;

namespace ZR.Model.System.Dto
{
    /// <summary>
    /// File manager query DTO
    /// </summary>
    public class FileManagerQueryDto
    {
        /// <summary>
        /// Path to browse (relative to wwwroot or base directory)
        /// </summary>
        public string Path { get; set; } = "";

        /// <summary>
        /// Search keyword for files/directories
        /// </summary>
        public string SearchKeyword { get; set; }

        /// <summary>
        /// Filter by file type (e.g., "image", "video", "document")
        /// </summary>
        public string FileTypeFilter { get; set; }
    }

    /// <summary>
    /// Directory creation DTO
    /// </summary>
    public class CreateDirectoryDto
    {
        /// <summary>
        /// Parent directory path
        /// </summary>
        public string ParentPath { get; set; } = "";

        /// <summary>
        /// New directory name
        /// </summary>
        public string DirectoryName { get; set; }
    }

    /// <summary>
    /// File/Directory rename DTO
    /// </summary>
    public class RenameDto
    {
        /// <summary>
        /// Current path (file or directory)
        /// </summary>
        public string CurrentPath { get; set; }

        /// <summary>
        /// New name (not full path, just the name)
        /// </summary>
        public string NewName { get; set; }

        /// <summary>
        /// Is this a directory?
        /// </summary>
        public bool IsDirectory { get; set; }
    }

    /// <summary>
    /// File/Directory move DTO
    /// </summary>
    public class MoveDto
    {
        /// <summary>
        /// Source path
        /// </summary>
        public string SourcePath { get; set; }

        /// <summary>
        /// Destination directory path
        /// </summary>
        public string DestinationPath { get; set; }

        /// <summary>
        /// Is this a directory?
        /// </summary>
        public bool IsDirectory { get; set; }
    }

    /// <summary>
    /// File/Directory delete DTO
    /// </summary>
    public class DeleteDto
    {
        /// <summary>
        /// Paths to delete
        /// </summary>
        public List<string> Paths { get; set; }

        /// <summary>
        /// Are these directories?
        /// </summary>
        public bool IsDirectory { get; set; }
    }

    /// <summary>
    /// File information response DTO
    /// </summary>
    public class FileInfoDto
    {
        /// <summary>
        /// File or directory name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Full path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Relative path from base directory
        /// </summary>
        public string RelativePath { get; set; }

        /// <summary>
        /// File extension (empty for directories)
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Formatted file size (e.g., "1.5 MB")
        /// </summary>
        public string FormattedSize { get; set; }

        /// <summary>
        /// Is this a directory?
        /// </summary>
        public bool IsDirectory { get; set; }

        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Last modified date
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// MIME type (for files)
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Is this an image file?
        /// </summary>
        public bool IsImage { get; set; }

        /// <summary>
        /// URL to access the file
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Thumbnail URL (for images)
        /// </summary>
        public string ThumbnailUrl { get; set; }
    }

    /// <summary>
    /// Directory contents response DTO
    /// </summary>
    public class DirectoryContentsDto
    {
        /// <summary>
        /// Current directory path
        /// </summary>
        public string CurrentPath { get; set; }

        /// <summary>
        /// Parent directory path (null if at root)
        /// </summary>
        public string ParentPath { get; set; }

        /// <summary>
        /// List of directories
        /// </summary>
        public List<FileInfoDto> Directories { get; set; } = new List<FileInfoDto>();

        /// <summary>
        /// List of files
        /// </summary>
        public List<FileInfoDto> Files { get; set; } = new List<FileInfoDto>();

        /// <summary>
        /// Total number of items
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Breadcrumb path items
        /// </summary>
        public List<BreadcrumbItem> Breadcrumbs { get; set; } = new List<BreadcrumbItem>();
    }

    /// <summary>
    /// Breadcrumb navigation item
    /// </summary>
    public class BreadcrumbItem
    {
        /// <summary>
        /// Display name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Path to this location
        /// </summary>
        public string Path { get; set; }
    }

    /// <summary>
    /// Upload multiple files DTO
    /// </summary>
    public class MultiUploadDto : UploadDto
    {
        /// <summary>
        /// Allow overwrite existing files
        /// </summary>
        public bool AllowOverwrite { get; set; } = false;
    }

    /// <summary>
    /// Disk space information DTO
    /// </summary>
    public class DiskSpaceDto
    {
        /// <summary>
        /// Total space in bytes
        /// </summary>
        public long TotalSpace { get; set; }

        /// <summary>
        /// Used space in bytes
        /// </summary>
        public long UsedSpace { get; set; }

        /// <summary>
        /// Free space in bytes
        /// </summary>
        public long FreeSpace { get; set; }

        /// <summary>
        /// Formatted total space
        /// </summary>
        public string FormattedTotal { get; set; }

        /// <summary>
        /// Formatted used space
        /// </summary>
        public string FormattedUsed { get; set; }

        /// <summary>
        /// Formatted free space
        /// </summary>
        public string FormattedFree { get; set; }

        /// <summary>
        /// Usage percentage
        /// </summary>
        public double UsagePercentage { get; set; }
    }

    /// <summary>
    /// Compress (zip) files/directories DTO
    /// </summary>
    public class CompressDto
    {
        /// <summary>
        /// Paths to compress
        /// </summary>
        public List<string> Paths { get; set; }

        /// <summary>
        /// Output zip file name (without extension)
        /// </summary>
        public string ZipFileName { get; set; }

        /// <summary>
        /// Directory where zip file will be created
        /// </summary>
        public string TargetDirectory { get; set; }
    }

    /// <summary>
    /// Extract (unzip/unrar) archive DTO
    /// </summary>
    public class ExtractDto
    {
        /// <summary>
        /// Path to archive file (zip, rar, 7z, etc.)
        /// </summary>
        public string ArchivePath { get; set; }

        /// <summary>
        /// Target directory for extraction (optional, defaults to archive location)
        /// </summary>
        public string TargetDirectory { get; set; }

        /// <summary>
        /// Overwrite existing files
        /// </summary>
        public bool Overwrite { get; set; } = false;

        /// <summary>
        /// Delete archive after extraction
        /// </summary>
        public bool DeleteAfterExtract { get; set; } = false;
    }

    /// <summary>
    /// Upload with auto-extract DTO
    /// </summary>
    public class UploadWithExtractDto : MultiUploadDto
    {
        /// <summary>
        /// Auto-extract compressed files after upload
        /// </summary>
        public bool AutoExtract { get; set; } = false;

        /// <summary>
        /// Delete archive after extraction
        /// </summary>
        public bool DeleteAfterExtract { get; set; } = false;
    }

    /// <summary>
    /// Get sizes for multiple items DTO
    /// </summary>
    public class GetSizesDto
    {
        /// <summary>
        /// Paths to calculate sizes for
        /// </summary>
        public List<string> Paths { get; set; }
    }

    /// <summary>
    /// Item size information
    /// </summary>
    public class ItemSizeDto
    {
        /// <summary>
        /// Item path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Size in bytes
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Formatted size
        /// </summary>
        public string FormattedSize { get; set; }
    }

    /// <summary>
    /// Multiple items sizes response DTO
    /// </summary>
    public class ItemsSizesDto
    {
        /// <summary>
        /// Total size of all items in bytes
        /// </summary>
        public long TotalSize { get; set; }

        /// <summary>
        /// Formatted total size
        /// </summary>
        public string FormattedSize { get; set; }

        /// <summary>
        /// Individual item sizes
        /// </summary>
        public List<ItemSizeDto> Items { get; set; } = new List<ItemSizeDto>();
    }
}

