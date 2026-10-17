using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SevenZipExtractor;

namespace Infrastructure
{
    public static class FileUtil
    {
        /// <summary>
        /// ?????????
        /// </summary>
        /// <param name="path"></param>
        /// <returns>eg: /{yourPath}/2020/11/3/</returns>
        public static string GetdirPath(string path = "")
        {
            DateTime date = DateTime.Now;
            string timeDir = date.ToString("yyyyMMdd");// date.ToString("yyyyMM/dd/HH/");

            if (!string.IsNullOrEmpty(path))
            {
                timeDir = Path.Combine(path, timeDir);
            }
            return timeDir;
        }

        /// <summary>
        /// ?????MD5?(16?)
        /// </summary>
        /// <param name="str">???,??????</param>
        /// <returns></returns>
        public static string HashFileName(string str = null)
        {
            if (string.IsNullOrEmpty(str))
            {
                str = Guid.NewGuid().ToString();
            }
            MD5 md5 = MD5.Create();
            return BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(str)), 4, 8).Replace("-", "");
        }

        /// <summary>
        /// ????????????????(????)
        /// </summary>
        /// <param name="file">????</param>
        public static void DeleteDirectory(string file)
        {
            try
            {
                //??????????
                if (Directory.Exists(file))
                {
                    DirectoryInfo fileInfo = new DirectoryInfo(file);
                    //??????????
                    fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;
                    foreach (string f in Directory.GetFileSystemEntries(file))
                    {
                        if (File.Exists(f))
                        {
                            //?????????
                            File.SetAttributes(file, FileAttributes.Normal);
                            //??????????
                            File.Delete(f);
                        }
                        else
                        {
                            //?????????? d
                            DeleteDirectory(f);
                        }
                    }
                    //??????
                    Directory.Delete(file);
                }

            }
            catch (Exception ex) // ????
            {
                Console.WriteLine("??????" + ex.Message);
            }
        }

        /// <summary>
        /// ????
        /// </summary>
        /// <param name="zipPath"></param>
        /// <param name="genCodePath"></param>
        /// <param name="zipFileName">???????</param>
        /// <returns></returns>
        public static bool ZipGenCode(string zipPath, string genCodePath, string zipFileName)
        {
            if (string.IsNullOrEmpty(zipPath)) return false;
            try
            {
                CreateDirectory(genCodePath);
                string zipFileFullName = Path.Combine(zipPath, zipFileName);
                if (File.Exists(zipFileFullName))
                {
                    File.Delete(zipFileFullName);
                }

                ZipFile.CreateFromDirectory(genCodePath, zipFileFullName);
                DeleteDirectory(genCodePath);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("???????" + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// ?????
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool CreateDirectory(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                path = path.Replace("\\", "/").Replace("//", "/");
            }
            try
            {
                if (!Directory.Exists(path))
                {
                    DirectoryInfo info = Directory.CreateDirectory(path);
                    Console.WriteLine("????????" + info);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"????????,{ex.Message}");
                return false;
            }
            return true;
        }

        /// <summary>
        /// ???
        /// </summary>
        /// <param name="path">?????????</param>
        /// <param name="content">??????</param>
        public static void WriteAndSave(string path, string content)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                path = path.Replace("\\", "/").Replace("//", "/");
            }
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            Console.WriteLine("??????,Path=" + path);
            try
            {
                File.WriteAllText(path, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("???????:" + ex.Message);
            }
        }












        /// <summary>
        /// Moves a file or directory from the source path to the destination path.
        /// </summary>
        /// <param name="sourcePath">The source file or directory path.</param>
        /// <param name="destinationPath">The destination path where the file or directory should be moved.</param>
        /// <returns>True if the move was successful, otherwise false.</returns>
        public static bool MoveFileOrDirectory(this string sourcePath, string destinationPath, bool makeDelete = false, string newCreator = "Abdulqader")
        {
            try
            {

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    sourcePath = sourcePath.GetLiunxPath();
                    destinationPath = destinationPath.GetLiunxPath();
                }

                if (File.Exists(sourcePath))
                {
                    if (makeDelete)
                    {
                        File.Delete(destinationPath);
                    }

                    // If the source is a file, move the file.
                    // string destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(sourcePath));
                    var path = Path.GetDirectoryName(destinationPath);

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(destinationPath);
                    }

                    File.Move(sourcePath, destinationPath);

                    // File.Delete(sourcePath);

                    if (makeDelete)
                    {
                        File.Delete(sourcePath);
                        Console.WriteLine($"File.Delete(sourcePath to {sourcePath}");
                    }

                    Console.WriteLine($"File moved to {destinationPath}");
                }
                else if (Directory.Exists(sourcePath))
                {
                    // If the source is a directory, move the directory.
                    string destinationDirPath = Path.Combine(destinationPath, Path.GetFileName(sourcePath));
                    if (Directory.Exists(destinationDirPath))
                    {
                        Console.WriteLine("Destination directory already exists. Deleting it before moving.");
                        Directory.Delete(destinationDirPath, true); // Delete the existing destination directory if it exists.
                    }
                    Directory.Move(sourcePath, destinationDirPath);
                    Console.WriteLine($"Directory moved to {destinationDirPath}");
                }
                else
                {
                    Console.WriteLine("Source path does not exist.");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error moving file or directory: " + ex.Message);
                return false;
            }
        }

        public static void DeleteFiles(string[] filePaths)
        {
            foreach (string filePath in filePaths)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        // Remove the read-only attribute and delete the file
                        File.SetAttributes(filePath, FileAttributes.Normal);
                        File.Delete(filePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting file '{filePath}': {ex.Message}");
                }
            }
        }




        public static void DeleteDirectory(this string directoryPath, string extSearch)
        {
            try
            {
                // Check if the directory exists
                if (Directory.Exists(directoryPath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

                    // Remove the read-only attribute of the directory
                    directoryInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;

                    // Get all files and subdirectories in the directory
                    var entries = Directory.GetFiles(directoryPath);
                    foreach (string entry in entries)
                    {
                        if (File.Exists(entry))
                        {
                            // Check if the file has the specified extension
                            if (Path.GetExtension(entry).Equals(extSearch, StringComparison.OrdinalIgnoreCase))
                            {
                                // Remove the read-only attribute of the file
                                File.SetAttributes(entry, FileAttributes.Normal);

                                // Delete the file with the specified extension
                                File.Delete(entry);
                                Console.WriteLine($"Deleted {extSearch} file: {entry}");
                            }
                        }
                        else
                        {
                            // Recursively delete subdirectories
                            DeleteDirectory(entry, extSearch);
                        }
                    }

                    // Delete the directory if it's empty
                    if (Directory.GetFileSystemEntries(directoryPath).Length == 0)
                    {
                        Directory.Delete(directoryPath);
                        Console.WriteLine($"Deleted empty directory: {directoryPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting directory '{directoryPath}': {ex.Message}");
            }
        }










        /// <summary>
        /// Uploads a file using IFormFile and saves it to the specified path.
        /// </summary>
        /// <param name="formFile">The uploaded file.</param>
        /// <param name="uploadPath">The directory where the file should be saved.</param>
        /// <returns>The relative path to the saved file.</returns>
        public static async Task<string> UploadFileAsync(IFormFile formFile, string uploadPath)
        {
            if (formFile == null || formFile.Length == 0)
                throw new ArgumentException("File is empty or null.", nameof(formFile));

            if (string.IsNullOrEmpty(uploadPath))
                throw new ArgumentException("Upload path is required.", nameof(uploadPath));


            uploadPath = uploadPath.GetGeneratedPath(iscreatefile: false);
            // Ensure the upload path exists
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            // Generate a sanitized file name
            var fileName = Path.GetFileName(formFile.FileName);



            var sanitizedFileName = SanitizeFileName(fileName);

            // Full file path
            var filePath = Path.Combine(uploadPath, sanitizedFileName);

            // Save the file
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await formFile.CopyToAsync(fileStream);
            }

















            // var fileName = "suupliersspos";

            // var dattime = DateTime.Now.ToString("MMddHHmmfffffff");


            // var resolvedPath = 0;
            // //  ?? Stopwatch.GetTimestamp();
            // var sFileName = $"{fileName}Expoert{dattime}{resolvedPath}.xlsx";
            // sFileName = sFileName.Trim();

            // string fullPathVV = Path.Combine(
            //    "", "wwwroot",
            //     "ExpoertTemplate", "Template",
            //     $"{sFileName}"
            // );





            // var reposesavepath = FileUtil.UploadFileAsync(formFile, fullPathVV).GetResultFromTask();



            // Return the relative path
            return filePath;
        }





        /// <summary>
        /// Compressed code
        /// </summary>
        /// <param name="zipPath"></param>
        /// <param name="genCodePath"></param>
        /// <param name="zipFileName">Compressed file name</param>
        /// <returns></returns>

        public static async Task<string> UploadFile(byte[] bytes, string fileName)
        {
            //fileName = SanitizeFileName(fileName);

            Stream stream = new MemoryStream(bytes);
            IFormFile file = new FormFile(stream, 0, bytes.Length, Path.GetFileNameWithoutExtension(fileName), Path.GetFileName(fileName));

            using (var ms = new FileStream(fileName, FileMode.Create))
            {
                //await file.CopyToAsync(ms);
                await stream.CopyToAsync(ms);

            }

            //await using var writeStream = File.Create(Path.Combine(uploadsFolder, ""));

            //await File.WriteAllBytesAsync(uploadsFolder, bytes);

            return "~/" + fileName.Replace("\\", "/");

        }

        /// <summary>
        /// Sanitizes a file name to remove invalid characters.
        /// </summary>
        /// <param name="fileName">The original file name.</param>
        /// <returns>A sanitized file name.</returns>
        private static string SanitizeFileName(this string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }














        public static bool UnzipGenCode(this string zipPath, string extractPath, string zipFileName)
        {
            if (string.IsNullOrEmpty(zipPath) || string.IsNullOrEmpty(extractPath) || string.IsNullOrEmpty(zipFileName)) return false;
            try
            {
                // Create the directory where the files will be extracted
                if (!Directory.Exists(extractPath))
                {
                    Directory.CreateDirectory(extractPath);
                }

                // Get the full path of the zip file
                string zipFileFullName = Path.Combine(zipPath, zipFileName);
                if (!File.Exists(zipFileFullName))
                {
                    Console.WriteLine("Zip file does not exist.");
                    return false;
                }

                // Extract the zip file to the specified directory
                ZipFile.ExtractToDirectory(zipFileFullName, extractPath);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in extracting file: " + ex.Message);
                return false;
            }
        }

        public static bool UnzipGenCodev2(this string zipPath, string extractPath, string zipFileName)
        {
            if (string.IsNullOrEmpty(zipPath) || string.IsNullOrEmpty(extractPath) || string.IsNullOrEmpty(zipFileName)) return false;
            try
            {
                // Create the directory where the files will be extracted
                if (!Directory.Exists(extractPath))
                {
                    Directory.CreateDirectory(extractPath);
                }

                // Get the full path of the zip file
                string zipFileFullName = Path.Combine(zipPath, zipFileName);
                if (!File.Exists(zipFileFullName))
                {
                    Console.WriteLine("Zip file does not exist.");
                    return false;
                }

                // Check file extension and extract accordingly
                string fileExtension = Path.GetExtension(zipFileFullName).ToLower();
                if (fileExtension == ".7z")
                {
                    // Extract 7z file
                    using (var extractor = new ArchiveFile(zipFileFullName))
                    {
                        extractor.Extract(extractPath, true);
                    }
                }
                else
                {
                    // Extract other zip file formats using ZipFile
                    ZipFile.ExtractToDirectory(zipFileFullName, extractPath);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in extracting file: " + ex.Message);
                return false;
            }
        }






        public static string ZipGenCodeFilesize(this string zipPath, string genCodePath, string zipFileName)
        {
            if (string.IsNullOrEmpty(zipPath)) return "Invalid path"; // Return an error message for invalid path

            try
            {
                // Ensure the target directory exists
                CreateDirectory(genCodePath);

                // Combine the zip path and file name to get the full path
                string zipFileFullName = Path.Combine(zipPath, zipFileName);

                // Delete the zip file if it already exists
                if (File.Exists(zipFileFullName))
                {
                    File.Delete(zipFileFullName);
                }

                // Create the zip file from the specified directory
                ZipFile.CreateFromDirectory(genCodePath, zipFileFullName);

                // Get the size of the created zip file in bytes
                long fileSizeBytes = new FileInfo(zipFileFullName).Length;

                // Convert the file size to the desired unit
                string fileSizeFormatted = FormatFileSize(fileSizeBytes);

                // Clean up the generated code directory
                DeleteDirectory(genCodePath);
                CreateDirectory(genCodePath);

                // Return the formatted file size
                return fileSizeFormatted;
            }
            catch (Exception ex)
            {
                // Log the error and return an error message
                Console.WriteLine("Error in compressing file: " + ex.Message);
                return "Error: " + ex.Message;
            }
        }



        private static string FormatFileSize(long fileSizeBytes)
        {
            string[] units = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            double size = fileSizeBytes;
            int unitIndex = 0;

            // Dynamically determine the appropriate unit
            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            // Format the size with the appropriate unit
            return $"{size:F2} {units[unitIndex]}";
        }



        private static string FormatFileSizeOld(long fileSizeBytes)
        {
            string[] units = { "bytes", "KB", "MB", "GB", "TB" };
            double size = fileSizeBytes;
            int unitIndex = 0;

            // Dynamically determine the appropriate unit
            while (size >= 1024 && unitIndex < units.Length - 1)
            {
                size /= 1024;
                unitIndex++;
            }

            // Format the size with the appropriate unit
            return $"{size:F2} {units[unitIndex]}";
        }




        // Helper method to format file size
        private static string FormatFileSize(long fileSizeBytes, string unit)
        {
            double size = fileSizeBytes;

            switch (unit.ToUpper())
            {
                case "KB":
                    size /= 1024; // Convert to KB
                    return $"{size:F2} KB";
                case "MB":
                    size /= (1024 * 1024); // Convert to MB
                    return $"{size:F2} MB";
                case "GB":
                    size /= (1024 * 1024 * 1024); // Convert to GB
                    return $"{size:F2} GB";
                case "TB":
                    size /= (1024L * 1024 * 1024 * 1024); // Convert to TB
                    return $"{size:F2} TB";
                default:
                    return $"{fileSizeBytes} bytes"; // Default to bytes
            }
        }


        /// <summary>
        /// Write file content as a byte array.
        /// </summary>
        /// <param name="path">Full path with extension</param>
        /// <param name="content">Byte array content to write to the file</param>
        public static void WriteAndSave(this string path, byte[] content)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                path = path.Replace("\\", "/").Replace("//", "/");
            }
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            Console.WriteLine("Start writing file, Path= " + path + " ");
            try
            {
                File.WriteAllBytes(path, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing file:" + ex.Message);
            }
        }


        public static async void WriteAndSaveAsync(this string path, byte[] content)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                path = path.Replace("\\", "/").Replace("//", "/");
            }
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            Console.WriteLine("Start writing file, Path= " + path + " ");
            try
            {
                await File.WriteAllBytesAsync(path, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing file:" + ex.Message);
            }
        }



        public static async Task WriteAndSaveAsync(this string path, string content)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                path = path.Replace("\\", "/").Replace("//", "/");
            }

            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            Console.WriteLine("Start writing file, Path=" + path);
            try
            {
                // Using asynchronous file writing
                await File.WriteAllTextAsync(path, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing file:" + ex.Message);
            }
        }




        public static byte[] ReadFileBytes(this string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                path = path.Replace("\\", "/").Replace("//", "/");
            }

            try
            {
                if (File.Exists(path))
                {
                    Console.WriteLine($"Start reading file as bytes, Path={path}");
                    return File.ReadAllBytes(path);
                }

                Console.WriteLine($"File not found: {path}");
                return Array.Empty<byte>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                return Array.Empty<byte>();
            }
        }

        public static async Task<byte[]> ReadFileBytesAsync(this string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                path = path.Replace("\\", "/").Replace("//", "/");
            }

            try
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine($"File not found: {path}");
                    return Array.Empty<byte>();
                }

                Console.WriteLine($"Start async reading file as bytes, Path={path}");

                using (var fileStream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 4096,
                    useAsync: true))
                {
                    var buffer = new byte[fileStream.Length];
                    var bytesRead = 0;
                    var chunkSize = 4096;

                    while (bytesRead < buffer.Length)
                    {
                        var remaining = buffer.Length - bytesRead;
                        var count = remaining < chunkSize ? remaining : chunkSize;

                        var currentRead = await fileStream.ReadAsync(
                            buffer,
                            bytesRead,
                            count);

                        if (currentRead == 0) break;

                        bytesRead += currentRead;
                    }

                    return buffer;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                return Array.Empty<byte>();
            }
        }


        public static string ReadFile(this string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                path = path.Replace("\\", "/").Replace("//", "/");
            }
            try
            {
                if (File.Exists(path))
                {
                    Console.WriteLine("Start reading file, Path=" + path);
                    return File.ReadAllText(path);
                }
                else
                {
                    Console.WriteLine("File not found: " + path);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading file:" + ex.Message);
                return string.Empty;
            }
        }


        public static async Task<string> ReadFileAsync(this string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                path = path.Replace("\\", "/").Replace("//", "/");
            }

            try
            {
                if (File.Exists(path))
                {
                    Console.WriteLine("Start reading file, Path=" + path);

                    // Using asynchronous file reading with Dispose pattern
                    using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
                else
                {
                    Console.WriteLine("File not found: " + path);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading file:" + ex.Message);
                return string.Empty;
            }
        }

    }

    public static class PathHelpers
    {
        /// <summary>
        /// Get the parent path based on the operating system (Unix or Windows).
        /// </summary>
        public static string GetParentPath(this string pathwhere, bool isbase = false)
        {

            const string marker = "ZR.Admin.WebApi";

            int markerIndex = pathwhere.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (markerIndex == -1)
            {
                // throw new InvalidOperationException($"The path does not contain the marker \"{marker}\".");
            }
            else
            {
                if (!pathwhere.ToLower().Contains("vue-water-frontend"))
                {
                    pathwhere = pathwhere.Substring(markerIndex);

                }

            }


            string tempPath = "";

            if (pathwhere.ToLower().Contains("vue-water-frontend"))
            {
                tempPath = pathwhere;
            }
            else
            {
                tempPath =

                          // Path.Combine(
                          App.WebHostEnvironment.ContentRootPath
                              // , 
                              +
                              pathwhere
                                // )
                                ;

            }






            if (isbase)
            {
                tempPath = App.WebHostEnvironment.ContentRootPath + "\\" + pathwhere;

                if (ComputerHelper.IsUnix())
                {
                    return pathwhere.GetLiunxPath();
                }
                return tempPath;
            }

            if (ComputerHelper.IsUnix())
            {
                // Unix-based system path
                var v = Path.Combine(tempPath);

                v = v.Replace("\\", "/");

                Console.WriteLine(v, "Unixbased");

                return v;
            }
            else
            {
                // Windows-based system path

                // string parentPath = Path.GetDirectoryName(tempPath) ?? tempPath;  // Get parent directory

                return tempPath.Replace("/", "\\");  // Ensure backslashes on Windows

                return tempPath.Substring(0, tempPath.LastIndexOf(@"\"));
            }
        }

        public static string GetLiunxPath(this string pathWhere)
        {
            // Combine the given path with the WebRootPath
            pathWhere = Path.Combine(App.WebHostEnvironment.ContentRootPath, pathWhere);
            Console.WriteLine(App.WebHostEnvironment.WebRootPath + " WebRootPath");
            Console.WriteLine(pathWhere + " tempPath");

            // Replace backslashes with forward slashes for Unix compatibility
            foreach (char c in pathWhere)
            {
                pathWhere = pathWhere.Replace("\\", "/");
            }

            // Find the index of "wwwroot" in the path
            var wwwrootIndex = pathWhere.IndexOf("//", StringComparison.OrdinalIgnoreCase);
            if (wwwrootIndex == -1)
            {
                Console.WriteLine(pathWhere + " Unix-based");
                return pathWhere;
                throw new ArgumentException("The provided path does not contain '//'. " + pathWhere, nameof(pathWhere));
            }
            else
            {

                var relativePath = pathWhere.Substring(wwwrootIndex + "//".Length);
                Console.WriteLine(relativePath + " Unix-based");
            }

            // Get the relative path after "wwwroot" and ensure it uses Unix-style slashes

            return "";
        }

        public static string GetLiunxPathV1(this string pathwhere)
        {
            pathwhere = Path.Combine(App.WebHostEnvironment.WebRootPath, pathwhere);


            return pathwhere;
            Console.WriteLine(App.WebHostEnvironment.WebRootPath, "WebRootPath");
            Console.WriteLine(pathwhere, "tempPath");

            pathwhere = pathwhere.Replace("\\", "/");

            // Unixbased- system path
            var v = Path.Combine(pathwhere);
            v = v.Replace("\\", "/");

            var wwwrootIndex = v.IndexOf("//", StringComparison.OrdinalIgnoreCase);
            if (wwwrootIndex == -1)
            {
                throw new ArgumentException("The provided path does not contain '//'.", nameof(v));
            }

            // Append the part of pathWhere that follows "wwwroot" and replace backslashes with forward slashes
            var relativePath = v.Substring(wwwrootIndex + "//".Length)
                                        .Replace("//", "/").Replace("///", "/").Replace("\\", "/");

            Console.WriteLine(relativePath + " Unixbased");
            return relativePath;
        }

        public static string GetBaseUrl()
        {
            var httpContextAccessor = App.GetService<IHttpContextAccessor>();
            if (httpContextAccessor?.HttpContext == null)
            {
                throw new InvalidOperationException("HTTP Context is not available.");
            }


            var request = httpContextAccessor.HttpContext.Request;
            var path = $@"{request.Scheme}://{request.Host}{request.PathBase}";
            var url = $"{path}";

            return url;
        }

        public static string GetFileNameFromUrl(this string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            try
            {
                // Create a Uri object
                var uri = new Uri(url);

                // Use Uri's LocalPath to get the path and Path.GetFileName to extract the file name
                var v = ("\\wwwroot\\" + uri.LocalPath).GetParentPath(true);
                //var v2 = ("\\wwwroot\\"+ uri.AbsolutePath)  .GetParentPath(true);
                return v;
            }
            catch (UriFormatException)
            {
                // Handle cases where the input is not a valid URL
                return string.Empty;
            }
        }


        public static string GetFileNameFromUrlForCV(this string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            try
            {
                // Create a Uri object
                var uri = new Uri(url);

                // Use Uri's LocalPath to get the path and Path.GetFileName to extract the file name
                var v = ("wwwroot" + uri.LocalPath).GetParentPath(true).Replace("\\", "/"); ; ;

                return v;
            }
            catch (UriFormatException)
            {
                return url.GetFileNameFromUrl();
                // Handle cases where the input is not a valid URL
                return string.Empty;
            }
        }

        public static (string pathWhere, bool issucce) GetHttpUrl(this string pathWhere)
        {
            var baseUrl = "";
            pathWhere = "/wwwroot" + pathWhere;
            var Fullpathd = pathWhere.GetGeneratedPath(null, false);

            var v = Fullpathd.DirectoryExists();
            baseUrl = GetBaseUrl();

            pathWhere = pathWhere.Replace("/wwwroot", "");
            pathWhere = pathWhere.Replace("\\wwwroot", "");

            var v1 = baseUrl + pathWhere;

            // var whatrespone = v1.GetFileNameFromUrl();
            // var whatrespon2e = whatrespone;

            return (v1, v);

        }
        public static string GetUrl(this string pathWhere)
        {
            var baseUrl = "";
            GetBaseUrl();

            // Check if "wwwroot" is in the path and get the substring following it
            var wwwrootIndex = pathWhere.IndexOf("wwwroot", StringComparison.OrdinalIgnoreCase);
            if (wwwrootIndex == -1)
            {
                // throw new ArgumentException("The provided path does not contain 'wwwroot'.", nameof(pathWhere));
            }

            // Append the part of pathWhere that follows "wwwroot" and replace backslashes with forward slashes
            var relativePath = pathWhere.Substring(wwwrootIndex + "wwwroot".Length)
                                        .Replace("\\", "/");

            // Ensure there is no double slash in the final URL
            var completeUrl = $"{baseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";

            return completeUrl;
        }

        /// <summary>
        /// Check if a directory exists.
        /// </summary>
        public static bool DirectoryExists(this string path)
        {
            return (Directory.Exists(path) || File.Exists(path));
        }

        /// <summary>
        /// Create a directory if it doesn't exist.
        /// </summary>
        public static void EnsureDirectoryExists(this string path)
        {
            if (!DirectoryExists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public static string ConvertImageToBase64(this string imagePath)
        {
            byte[] imageBytes = File.ReadAllBytes(imagePath);
            return Convert.ToBase64String(imageBytes);
        }



        ///// <summary>
        ///// Checks if the path is fully qualified (absolute path)
        ///// </summary>
        //public static bool IsFullyQualifiedPath(this string path)
        //{
        //    if (string.IsNullOrWhiteSpace(path))
        //        return false;

        //    // For .NET Core 3.0+, .NET 5+ (recommended)
        //    return Path.IsPathFullyQualified(path);

        //}


        /// <summary>
        /// Checks if the path is fully qualified (absolute path)
        /// Works across all .NET versions
        /// </summary>
        public static bool IsFullyQualifiedPath(this string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                // Modern .NET (recommended)
                if (typeof(Path).GetMethod("IsPathFullyQualified") != null)
                {
                    return Path.IsPathFullyQualified(path);
                }

                // Fallback for older .NET versions
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Windows paths
                    var isAbsolutePath = path.Contains(@"\\") ||                     // UNC path
                          (path.Length >= 3 &&                           // Drive letter path
                           char.IsLetter(path[0]) &&
                           path[1] == ':' &&
                           path[2] == '\\');
                    return isAbsolutePath;
                }
                else
                {
                    // Linux/Mac paths
                    return path.StartsWith("/");
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur
                // For example, log the error or rethrow it
                Console.WriteLine("Error checking path: " + ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Gets the fully qualified path if it's not already
        /// </summary>
        public static string ToFullyQualifiedPath(this string path, string basePath = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            if (path.IsFullyQualifiedPath())
                return path;

            basePath ??= Directory.GetCurrentDirectory();
            return Path.GetFullPath(path, basePath);
        }

        /// <summary>
        /// Get the full path combining the parent path and the generation path.
        /// </summary>
        public static string GetGeneratedPath(this string genPath, string customPath = null, bool iscreatefile = true)
        {
            // If custom path is provided, use it; otherwise, use the default genPath.


            const string marker = @"ZR.Admin.WebApi";





            var finalGenPath = !string.IsNullOrEmpty(customPath) ? customPath : genPath;



            if (!finalGenPath.ToLower().Contains("vue-water-frontend"))
            {


                int markerIndex = finalGenPath.LastIndexOf(marker, StringComparison.OrdinalIgnoreCase);
                if (markerIndex == -1)
                {
                    // throw new InvalidOperationException($"The path does not contain the marker \"{marker}\".");
                }
                else
                {


                    finalGenPath = finalGenPath.Substring(markerIndex + marker.Length);

                    // Optionally, clean up leading slashes or backslashes
                    // finalGenPath = finalGenPath.TrimStart('\\', '/');



                    // finalGenPath = finalGenPath.Substring(markerIndex);
                }
            }

            // Get the parent path based on the OS.
            string parentPath = finalGenPath.GetParentPath();


            // Handle parent directory references (..)
            //finalGenPath = ResolveParentDirectoryReferences(finalGenPath);

            // Get the base directory path
            //string basePath = Environment.CurrentDirectory;

            //// Combine paths and normalize
            //string fullPath = Path.GetFullPath(Path.Combine(basePath, finalGenPath));

            // Combine the parent path with genPath.
            var fullPath = parentPath;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {

                fullPath = fullPath.GetLiunxPath();
            }
            // Ensure the directory exists.

            if (iscreatefile)
            {
                EnsureDirectoryExists(fullPath);

            }

            return fullPath;
        }





        public static string ResolveParentDirectoryReferences(this string path)
        {
            var segments = new List<string>();
            foreach (var segment in path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (segment == "..")
                {
                    if (segments.Count > 0)
                    {
                        segments.RemoveAt(segments.Count - 1);
                    }
                }
                else if (segment != ".")
                {
                    segments.Add(segment);
                }
            }

            var resolvedPath = string.Join(Path.DirectorySeparatorChar.ToString(), segments);

            //resolvedPath = resolvedPath.ResolveParentDirectoryReferences();

            const string marker = @"ZR.Admin.WebApi";

            //// Combine with current directory and get full path
            //var basePath = Environment.CurrentDirectory.Replace(marker, "");
            //resolvedPath = Path.GetFullPath(Path.Combine(basePath, resolvedPath));

            return resolvedPath;
        }

    












}
}

