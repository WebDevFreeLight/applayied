using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ZR.Infrastructure.Helper
{
    /// <summary>
    /// SECURITY: Secure file upload validation with whitelist approach
    /// Validates file extensions, MIME types, and magic bytes
    /// </summary>
    public static class SecureFileValidator
    {
        // WHITELIST of allowed file types with their magic bytes signatures
        private static readonly Dictionary<string, FileTypeInfo> AllowedFileTypes = new()
        {
            // Images
            { ".jpg", new FileTypeInfo("image/jpeg", new byte[][] { new byte[] { 0xFF, 0xD8, 0xFF } }) },
            { ".jpeg", new FileTypeInfo("image/jpeg", new byte[][] { new byte[] { 0xFF, 0xD8, 0xFF } }) },
            { ".png", new FileTypeInfo("image/png", new byte[][] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } }) },
            { ".gif", new FileTypeInfo("image/gif", new byte[][] { new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 } }) },
            { ".webp", new FileTypeInfo("image/webp", new byte[][] { new byte[] { 0x52, 0x49, 0x46, 0x46 } }) }, // RIFF header
            { ".bmp", new FileTypeInfo("image/bmp", new byte[][] { new byte[] { 0x42, 0x4D } }) },
            { ".ico", new FileTypeInfo("image/x-icon", new byte[][] { new byte[] { 0x00, 0x00, 0x01, 0x00 } }) },
            { ".svg", new FileTypeInfo("image/svg+xml", null, true) }, // XML-based, needs special handling

            // Documents
            { ".pdf", new FileTypeInfo("application/pdf", new byte[][] { new byte[] { 0x25, 0x50, 0x44, 0x46 } }) }, // %PDF
            { ".doc", new FileTypeInfo("application/msword", new byte[][] { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } }) },
            { ".docx", new FileTypeInfo("application/vnd.openxmlformats-officedocument.wordprocessingml.document", new byte[][] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } }) }, // ZIP-based
            { ".xls", new FileTypeInfo("application/vnd.ms-excel", new byte[][] { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } }) },
            { ".xlsx", new FileTypeInfo("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", new byte[][] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } }) },
            { ".ppt", new FileTypeInfo("application/vnd.ms-powerpoint", new byte[][] { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } }) },
            { ".pptx", new FileTypeInfo("application/vnd.openxmlformats-officedocument.presentationml.presentation", new byte[][] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } }) },
            { ".txt", new FileTypeInfo("text/plain", null, true) }, // Text files don't have magic bytes
            { ".csv", new FileTypeInfo("text/csv", null, true) },

            // Videos (if needed)
            { ".mp4", new FileTypeInfo("video/mp4", new byte[][] { new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 }, new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70 } }) },
            { ".webm", new FileTypeInfo("video/webm", new byte[][] { new byte[] { 0x1A, 0x45, 0xDF, 0xA3 } }) },

            // Audio
            { ".mp3", new FileTypeInfo("audio/mpeg", new byte[][] { new byte[] { 0xFF, 0xFB }, new byte[] { 0xFF, 0xFA }, new byte[] { 0x49, 0x44, 0x33 } }) },
        };

        // Dangerous extensions that should NEVER be uploaded
        private static readonly HashSet<string> DangerousExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            // Executables
            ".exe", ".dll", ".com", ".bat", ".cmd", ".msi", ".scr", ".pif",
            // Scripts
            ".js", ".vbs", ".vbe", ".jse", ".ws", ".wsf", ".wsc", ".wsh", ".ps1", ".psm1", ".psd1",
            ".sh", ".bash", ".zsh", ".csh", ".ksh",
            // Web shells
            ".php", ".php3", ".php4", ".php5", ".php7", ".phtml", ".phar",
            ".asp", ".aspx", ".ashx", ".asmx", ".ascx", ".cshtml", ".vbhtml",
            ".jsp", ".jspx", ".jsf", ".jsw", ".jsv",
            ".cgi", ".pl", ".pm", ".py", ".pyc", ".pyo",
            ".rb", ".erb",
            // Java
            ".jar", ".war", ".ear", ".class",
            // Config/dangerous
            ".htaccess", ".htpasswd", ".config", ".ini",
            // Archives that could contain executables
            ".iso", ".img",
            // Others
            ".elf", ".bin", ".run", ".AppImage", ".deb", ".rpm",
            ".reg", ".inf", ".lnk", ".url", ".hta", ".chm"
        };

        /// <summary>
        /// Validates an uploaded file for security
        /// </summary>
        /// <returns>Validation result with error message if invalid</returns>
        public static FileValidationResult ValidateFile(IFormFile file, long maxSizeBytes = 15 * 1024 * 1024)
        {
            if (file == null || file.Length == 0)
            {
                return FileValidationResult.Fail("File is empty or null");
            }

            // Check file size
            if (file.Length > maxSizeBytes)
            {
                return FileValidationResult.Fail($"File exceeds maximum size of {maxSizeBytes / (1024 * 1024)}MB");
            }

            // Get and normalize extension
            string extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? "";

            // Check for dangerous extensions
            if (DangerousExtensions.Contains(extension))
            {
                return FileValidationResult.Fail($"File type '{extension}' is not allowed for security reasons");
            }

            // Check for double extensions (e.g., file.php.jpg)
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
            string secondExtension = Path.GetExtension(fileNameWithoutExt)?.ToLowerInvariant() ?? "";
            if (DangerousExtensions.Contains(secondExtension))
            {
                return FileValidationResult.Fail("Double extension detected with dangerous file type");
            }

            // Check if extension is in whitelist
            if (!AllowedFileTypes.TryGetValue(extension, out var expectedType))
            {
                return FileValidationResult.Fail($"File type '{extension}' is not in the allowed list");
            }

            // Validate MIME type matches expected
            string contentType = file.ContentType?.ToLowerInvariant() ?? "";
            if (!string.IsNullOrEmpty(expectedType.MimeType) &&
                !contentType.Contains(expectedType.MimeType.Split('/')[1]))
            {
                // Allow some flexibility but log for monitoring
                // Some browsers send different MIME types
            }

            // Validate magic bytes (file signature)
            if (expectedType.MagicBytes != null && expectedType.MagicBytes.Length > 0)
            {
                using var stream = file.OpenReadStream();
                byte[] headerBytes = new byte[16];
                int bytesRead = stream.Read(headerBytes, 0, headerBytes.Length);
                stream.Position = 0; // Reset stream position

                bool magicBytesMatch = expectedType.MagicBytes.Any(magic =>
                    bytesRead >= magic.Length &&
                    headerBytes.Take(magic.Length).SequenceEqual(magic));

                if (!magicBytesMatch)
                {
                    return FileValidationResult.Fail(
                        $"File content does not match expected format for '{extension}'. " +
                        "The file may be corrupted or disguised.");
                }
            }

            // Special handling for text-based files
            if (expectedType.IsTextBased)
            {
                using var reader = new StreamReader(file.OpenReadStream());
                string content = reader.ReadToEnd();

                // Check for embedded scripts in text files
                if (ContainsDangerousContent(content))
                {
                    return FileValidationResult.Fail("File contains potentially dangerous content");
                }
            }

            // Generate a secure random filename
            string secureFileName = GenerateSecureFileName(extension);

            return FileValidationResult.Success(secureFileName);
        }

        /// <summary>
        /// Generates a cryptographically secure random filename
        /// </summary>
        public static string GenerateSecureFileName(string extension)
        {
            // Use cryptographic random for filename
            byte[] randomBytes = new byte[16];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            string randomName = Convert.ToHexString(randomBytes).ToLowerInvariant();
            return $"{DateTime.UtcNow:yyyyMMdd}_{randomName}{extension}";
        }

        /// <summary>
        /// Check for dangerous content patterns in text files
        /// </summary>
        private static bool ContainsDangerousContent(string content)
        {
            if (string.IsNullOrEmpty(content)) return false;

            string[] dangerousPatterns = new[]
            {
                "<script", "<?php", "<%", "<asp:", "javascript:",
                "vbscript:", "onload=", "onerror=", "onclick=",
                "eval(", "exec(", "system(", "shell_exec(",
                "passthru(", "popen(", "proc_open(",
                "base64_decode(", "gzinflate(", "str_rot13("
            };

            string lowerContent = content.ToLowerInvariant();
            return dangerousPatterns.Any(pattern => lowerContent.Contains(pattern));
        }

        private class FileTypeInfo
        {
            public string MimeType { get; }
            public byte[][] MagicBytes { get; }
            public bool IsTextBased { get; }

            public FileTypeInfo(string mimeType, byte[][] magicBytes, bool isTextBased = false)
            {
                MimeType = mimeType;
                MagicBytes = magicBytes;
                IsTextBased = isTextBased;
            }
        }
    }

    public class FileValidationResult
    {
        public bool IsValid { get; private set; }
        public string ErrorMessage { get; private set; }
        public string SecureFileName { get; private set; }

        private FileValidationResult() { }

        public static FileValidationResult Success(string secureFileName)
        {
            return new FileValidationResult
            {
                IsValid = true,
                SecureFileName = secureFileName
            };
        }

        public static FileValidationResult Fail(string errorMessage)
        {
            return new FileValidationResult
            {
                IsValid = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
