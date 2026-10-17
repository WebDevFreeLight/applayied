using System.Collections.Generic;
using System.IO;
using ZXing;
using QRCoder;
using System;
using ZXing.QrCode;
using Infrastructure.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;
using ZXing.Rendering;
using SkiaSharp;
using Infrastructure;
namespace ZR.Infrastructure.Extensions.Qrcode
{
    public static class BarcodeAndQRCodeHelper
    {








        /// <summary>
        /// توليد رمز QR وحفظه كصورة PNG.
        /// </summary>
        /// <param name="text">النص المراد تحويله إلى رمز QR.</param>
        /// <param name="outputPath">مسار حفظ الصورة.</param>
        public static void GenerateQRCode(this string text, string outputPath = "")
        {

            if (string.IsNullOrEmpty(outputPath))
            {
                outputPath = @$"wwwroot\QRCode\{text}Local.Png".GetParentPath(true);
            }



            // Create QRCodeGenerator instance to generate the QR code data
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

                // Use PngByteQRCode to generate a byte array for a PNG image
                using (var qrCode = new PngByteQRCode(qrCodeData))
                {
                    // Generate the QR code graphic as an image
                    var qrImage = qrCode.GetGraphic(20, drawQuietZones: true);

                    // Save the generated image to the output path
                    SaveImageToFile(qrImage, outputPath);

                }
            }
        }








        /// <summary>
        /// توليد رمز QR مع تخصيص ألوان وحجم (cross-platform using SkiaSharp).
        /// </summary>
        /// <param name="text">النص المراد تحويله إلى رمز QR.</param>
        /// <param name="pixelSize">حجم البكسل لكل وحدة.</param>
        /// <param name="foregroundColor">لون المقدمة.</param>
        /// <param name="backgroundColor">لون الخلفية.</param>
        /// <returns>مصفوفة بايت تحتوي على صورة رمز QR.</returns>
        public static string GenerateCustomBase64(this
            string text,
            int pixelSize = 20,
            string foregroundColor = "#000000",
            string backgroundColor = "#FFFFFF")
        {
            // Parse colors using cross-platform method
            var fgColor = ParseHtmlColor(foregroundColor);
            var bgColor = ParseHtmlColor(backgroundColor);

            // Create a QRCodeWriter instance
            var qrCodeWriter = new QRCodeWriter();

            // Calculate dimensions based on pixel size
            int width = 300;
            int height = 300;

            // Generate the QR code matrix (2D array of black/white pixels)
            var qrCodeMatrix = qrCodeWriter.encode(text, BarcodeFormat.QR_CODE, width, height);

            // Convert the matrix to SKBitmap with custom colors (cross-platform)
            using var barcodeBitmap = new SKBitmap(qrCodeMatrix.Width, qrCodeMatrix.Height);
            for (int i = 0; i < qrCodeMatrix.Width; i++)
            {
                for (int j = 0; j < qrCodeMatrix.Height; j++)
                {
                    barcodeBitmap.SetPixel(i, j, qrCodeMatrix[i, j] ? fgColor : bgColor);
                }
            }

            // Convert the SKBitmap to a byte array
            using var image = SKImage.FromBitmap(barcodeBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            byte[] byteImage = data.ToArray();

            // Convert the byte array to a Base64 string
            return Convert.ToBase64String(byteImage);
        }

        /// <summary>
        /// Helper method to parse HTML color strings (e.g., #FFFFFF) into SKColor
        /// </summary>
        private static SKColor ParseHtmlColor(string htmlColor)
        {
            if (string.IsNullOrEmpty(htmlColor))
                return SKColors.Black;

            // Remove # if present
            htmlColor = htmlColor.TrimStart('#');

            // Parse hex color
            if (htmlColor.Length == 6) // RGB format
            {
                byte r = Convert.ToByte(htmlColor.Substring(0, 2), 16);
                byte g = Convert.ToByte(htmlColor.Substring(2, 2), 16);
                byte b = Convert.ToByte(htmlColor.Substring(4, 2), 16);
                return new SKColor(r, g, b);
            }
            else if (htmlColor.Length == 8) // ARGB format
            {
                byte a = Convert.ToByte(htmlColor.Substring(0, 2), 16);
                byte r = Convert.ToByte(htmlColor.Substring(2, 2), 16);
                byte g = Convert.ToByte(htmlColor.Substring(4, 2), 16);
                byte b = Convert.ToByte(htmlColor.Substring(6, 2), 16);
                return new SKColor(r, g, b, a);
            }

            return SKColors.Black; // Default to black if parsing fails
        }











        /// <summary>
        /// توليد رمز QR مع تخصيص ألوان وحجم.
        /// </summary>
        /// <param name="text">النص المراد تحويله إلى رمز QR.</param>
        /// <param name="pixelSize">حجم البكسل لكل وحدة.</param>
        /// <param name="foregroundColor">لون المقدمة.</param>
        /// <param name="backgroundColor">لون الخلفية.</param>
        /// <returns>مصفوفة بايت تحتوي على صورة رمز QR.</returns>
        public static string GenerateBase64(this
            string text,
            int pixelSize = 20,
            string foregroundColor = "#000000",
            string backgroundColor = "#FFFFFF")
        {


            return Base64QRCodeHelper.GetQRCode(text, pixelSize, foregroundColor, backgroundColor, QRCodeGenerator.ECCLevel.Q, true, imgType: Base64QRCode.ImageType.Png);

        }
        /// <summary>
        /// توليد رمز شريطي (Barcode) باستخدام ZXing.
        /// </summary>
        /// <param name="text">النص المراد تحويله إلى رمز شريطي.</param>
        /// <param name="width">عرض الصورة.</param>
        /// <param name="height">ارتفاع الصورة.</param>
        /// <returns>مصفوفة بايت تحتوي على صورة الرمز الشريطي.</returns>
        public static byte[] GenerateBarcode(this string text, int width = 300, int height = 100)
        {
            var barcodeWriter = new ZXing.BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = width,
                    Height = height,
                    PureBarcode = true
                }
            };

            var pixelData = barcodeWriter.Write(text);

            using var bitmap = new SKBitmap(pixelData.Width, pixelData.Height);
            // bitmap.Pixels = pixelData.Pixels;
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }







        /// <summary>
        /// توليد رمز شريطي (Barcode) مع تخصيص الحجم والنوع.
        /// </summary>
        /// <param name="text">النص المراد تحويله إلى رمز شريطي.</param>
        /// <param name="width">عرض رمز الشريط.</param>
        /// <param name="height">ارتفاع رمز الشريط.</param>
        /// <param name="barcodeFormat">نوع الرمز الشريطي (مثل: Code128، EAN-13).</param>
        /// <returns>سلسلة Base64 تحتوي على صورة الرمز الشريطي.</returns>
        // public static string GenerateBarcodeBase64(this string text, int width = 300, int height = 100, BarcodeFormat barcodeFormat = BarcodeFormat.QR_CODE)
        // {
        //     // Create a BarcodeWriter instance
        //     var barcodeWriter = new BarcodeWriter<PixelData>
        //     {
        //         Format = barcodeFormat,  // Set the barcode format (e.g., Code 128)
        //         Options = new ZXing.Common.EncodingOptions
        //         {
        //             Width = width,    // Set the width of the barcode
        //             Height = height,  // Set the height of the barcode
        //             Margin = 10       // Set the margin
        //         }
        //         ,
        //         Renderer = new ZXing.Rendering.PixelDataRenderer()
        //     };

        //     // Generate the barcode (bitmap image)
        //     var barcodeBitmap = barcodeWriter.Write(text);

        //     // Convert the Bitmap to a byte array
        //     using (var ms = new MemoryStream())
        //     {
        //         barcodeBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);


        //         byte[] byteImage = barcodeBitmap.Pixels?? ms.ToArray();

        //         // Convert the byte array to a Base64 string
        //         return Convert.ToBase64String(byteImage);
        //     }
        // }




        /// <summary>
        /// توليد رمز شريطي (Barcode) مع تخصيص الحجم والنوع.
        /// </summary>
        /// <param name="text">النص المراد تحويله إلى رمز شريطي.</param>
        /// <param name="width">عرض رمز الشريط.</param>
        /// <param name="height">ارتفاع رمز الشريط.</param>
        /// <param name="barcodeFormat">نوع الرمز الشريطي (مثل: Code128، EAN-13).</param>
        /// <returns>سلسلة Base64 تحتوي على صورة الرمز الشريطي.</returns>
        public static string GenerateBarcodeBase64(this string text, int width = 300, int height = 100, BarcodeFormat barcodeFormat = BarcodeFormat.QR_CODE)
        {
            // Create a BarcodeWriter instance with PixelData
            var barcodeWriter = new BarcodeWriter<PixelData>
            {
                Format = barcodeFormat,  // Set the barcode format (e.g., Code 128)
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = width,    // Set the width of the barcode
                    Height = height,  // Set the height of the barcode
                    Margin = 10       // Set the margin
                },
                Renderer = new ZXing.Rendering.PixelDataRenderer()  // Set the renderer for PixelData
            };

            // Generate the barcode (PixelData image)
            PixelData barcodePixelData = barcodeWriter.Write(text);

            // Convert PixelData to SkiaSharp SKBitmap (cross-platform)
            using var barcodeBitmap = new SKBitmap(barcodePixelData.Width, barcodePixelData.Height);

            // Convert each pixel in PixelData to a SKBitmap pixel
            for (int y = 0; y < barcodePixelData.Height; y++)
            {
                for (int x = 0; x < barcodePixelData.Width; x++)
                {
                    // Calculate the index for the pixel in the byte array
                    int pixelIndex = (y * barcodePixelData.Width + x) * 4; // Each pixel has 4 bytes (BGRA)

                    // Extract the BGRA values
                    byte blue = barcodePixelData.Pixels[pixelIndex];
                    byte green = barcodePixelData.Pixels[pixelIndex + 1];
                    byte red = barcodePixelData.Pixels[pixelIndex + 2];
                    byte alpha = barcodePixelData.Pixels[pixelIndex + 3];

                    // Create SKColor (already in correct format)
                    var color = new SKColor(red, green, blue, alpha);

                    // Set the pixel on the SKBitmap
                    barcodeBitmap.SetPixel(x, y, color);
                }
            }

            // Convert the SKBitmap to a byte array (PNG format)
            using var image = SKImage.FromBitmap(barcodeBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            byte[] byteImage = data.ToArray();

            // Convert the byte array to a Base64 string
            return Convert.ToBase64String(byteImage);
        }

















        /// <summary>
        /// توليد رمز شريطي (Barcode) مع تخصيص الحجم والنوع.
        /// </summary>
        /// <param name="text">النص المراد تحويله إلى رمز شريطي.</param>
        /// <param name="width">عرض رمز الشريط.</param>
        /// <param name="height">ارتفاع رمز الشريط.</param>
        /// <param name="barcodeFormat">نوع الرمز الشريطي (مثل: Code128، EAN-13).</param>
        /// <returns>سلسلة Base64 تحتوي على صورة الرمز الشريطي.</returns>
        public static string GenerateBarcodeJpegBase64(this string text, int width = 300, int height = 100, BarcodeFormat barcodeFormat = BarcodeFormat.QR_CODE)
        {
            // Create a BarcodeWriter instance with PixelData
            var barcodeWriter = new BarcodeWriter<PixelData>
            {
                Format = barcodeFormat,  // Set the barcode format (e.g., Code 128)
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = width,    // Set the width of the barcode
                    Height = height,  // Set the height of the barcode
                    Margin = 10       // Set the margin
                },
                Renderer = new ZXing.Rendering.PixelDataRenderer()  // Set the renderer for PixelData
            };

            // Generate the barcode (PixelData image)
            PixelData barcodePixelData = barcodeWriter.Write(text);

            // Convert PixelData to SkiaSharp SKBitmap (cross-platform)
            using var barcodeBitmap = new SKBitmap(barcodePixelData.Width, barcodePixelData.Height);

            // Convert each pixel in PixelData to a SKBitmap pixel
            for (int y = 0; y < barcodePixelData.Height; y++)
            {
                for (int x = 0; x < barcodePixelData.Width; x++)
                {
                    // Calculate the index for the pixel in the byte array
                    int pixelIndex = (y * barcodePixelData.Width + x) * 4; // Each pixel has 4 bytes (BGRA)

                    // Extract the BGRA values
                    byte blue = barcodePixelData.Pixels[pixelIndex];
                    byte green = barcodePixelData.Pixels[pixelIndex + 1];
                    byte red = barcodePixelData.Pixels[pixelIndex + 2];
                    byte alpha = barcodePixelData.Pixels[pixelIndex + 3];

                    // Create SKColor
                    var color = new SKColor(red, green, blue, alpha);

                    // Set the pixel on the SKBitmap
                    barcodeBitmap.SetPixel(x, y, color);
                }
            }

            // Convert the SKBitmap to a byte array (JPEG format)
            using var image = SKImage.FromBitmap(barcodeBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
            byte[] byteImage = data.ToArray();

            // Convert the byte array to a Base64 string
            return Convert.ToBase64String(byteImage);
        }


        // public static string ReadQrCode(byte[] qrCode)
        // {
        //     var coreCompatReader = new BarcodeReader();

        //     using (Stream stream = new MemoryStream(qrCode))
        //     {
        //         using (var coreCompatImage = (Bitmap)Image.FromStream(stream))
        //         {
        //             return coreCompatReader.Decode(coreCompatImage).Text;
        //         }
        //     }
        // }











        /// <summary>
        /// توليد رمز شريطي (Barcode) مع تخصيص الحجم والنوع.
        /// </summary>
        /// <param name="text">النص المراد تحويله إلى رمز شريطي.</param>
        /// <param name="width">عرض رمز الشريط.</param>
        /// <param name="height">ارتفاع رمز الشريط.</param>
        /// <param name="barcodeFormat">نوع الرمز الشريطي (مثل: Code128، EAN-13).</param>
        /// <returns>سلسلة Base64 تحتوي على صورة الرمز الشريطي.</returns>
        public static string ImageSharpBarcodeBase64(this string text, int width = 300, int height = 100, BarcodeFormat barcodeFormat = BarcodeFormat.QR_CODE)
        {
            // Create a BarcodeWriter instance
            var barcodeWriter = new BarcodeWriter<Image<Rgba32>>
            {
                Format = barcodeFormat,  // Set the barcode format (e.g., Code 128)
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = width,    // Set the width of the barcode
                    Height = height,  // Set the height of the barcode
                    Margin = 10       // Set the margin
                },
                // Renderer = new BarcodeWriter<Image<Rgba32>>(),  // Set the renderer for the barcode
            };
            // barcodeWriter.Renderer = new();
            // Generate the barcode (ImageSharp image)
            var barcodeImage = barcodeWriter.Write(text);

            // Convert the ImageSharp Image to a byte array (PNG format)
            using (var ms = new MemoryStream())
            {
                barcodeImage.Save(ms, new PngEncoder()); // Save as PNG format
                byte[] byteImage = ms.ToArray();

                // Convert the byte array to a Base64 string
                return Convert.ToBase64String(byteImage);
            }
        }











        /// <summary>
        /// توليد رمز شريطي (Barcode) مع تخصيص الحجم والنوع.
        /// </summary>
        /// <param name="text">النص المراد تحويله إلى رمز شريطي.</param>
        /// <param name="width">عرض رمز الشريط.</param>
        /// <param name="height">ارتفاع رمز الشريط.</param>
        /// <param name="barcodeFormat">نوع الرمز الشريطي (مثل: Code128، EAN-13).</param>
        /// <returns>سلسلة Base64 تحتوي على صورة الرمز الشريطي.</returns>
        public static string ImageSharpBarcodeJpegBase64(this string text, int width = 300, int height = 100, BarcodeFormat barcodeFormat = BarcodeFormat.QR_CODE)
        {
            // Create a BarcodeWriter instance
            var barcodeWriter = new BarcodeWriter<Image<Rgba32>>
            {
                Format = barcodeFormat,  // Set the barcode format (e.g., Code 128)
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = width,    // Set the width of the barcode
                    Height = height,  // Set the height of the barcode
                    Margin = 10       // Set the margin
                }
            };

            // Generate the barcode (ImageSharp image)
            var barcodeImage = barcodeWriter.Write(text);

            // Convert the ImageSharp Image to a byte array (PNG format)
            using (var ms = new MemoryStream())
            {
                barcodeImage.Save(ms, new JpegEncoder()); // Save as PNG format
                byte[] byteImage = ms.ToArray();

                // Convert the byte array to a Base64 string
                return Convert.ToBase64String(byteImage);
            }
        }

        /// <summary>
        /// توليد رمز QR مع تخصيص الحجم.
        /// </summary>
        /// <param name="text">النص المراد تحويله إلى رمز QR.</param>
        /// <param name="width">عرض رمز QR.</param>
        /// <param name="height">ارتفاع رمز QR.</param>
        /// <returns>سلسلة Base64 تحتوي على صورة رمز QR.</returns>
        public static string GenerateQRCodeBase64(this string text, int width = 300, int height = 300)
        {
            // Create a QRCodeWriter instance
            var qrCodeWriter = new QRCodeWriter();

            // Set the encoding options
            var encodingOptions = new ZXing.Common.EncodingOptions
            {
                Width = width,    // Set the width of the QR code
                Height = height,  // Set the height of the QR code
                Margin = 1        // Set the margin
            };

            // Generate the QR code matrix (2D array of black/white pixels)
            var qrCodeMatrix = qrCodeWriter.encode(text, BarcodeFormat.QR_CODE, width, height
            // , encodingOptions

            );

            // Convert the matrix to SKBitmap (cross-platform)
            using var barcodeBitmap = new SKBitmap(qrCodeMatrix.Width, qrCodeMatrix.Height);
            for (int i = 0; i < qrCodeMatrix.Width; i++)
            {
                for (int j = 0; j < qrCodeMatrix.Height; j++)
                {
                    barcodeBitmap.SetPixel(i, j, qrCodeMatrix[i, j] ? SKColors.Black : SKColors.White);
                }
            }

            // Convert the SKBitmap to a byte array
            using var image = SKImage.FromBitmap(barcodeBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
            byte[] byteImage = data.ToArray();

            // Convert the byte array to a Base64 string
            return Convert.ToBase64String(byteImage);
        }


        /// <summary>
        /// حفظ الصورة إلى ملف.
        /// </summary>
        /// <param name="imageBytes">الصورة كمصفوفة بايت.</param>
        /// <param name="filePath">المسار المطلوب لحفظ الصورة.</param>
        public static void SaveImageToFile(byte[] imageBytes, string filePath)
        {
            File.WriteAllBytes(filePath, imageBytes);
        }



        /// <summary>
        /// توليد رمز شريطي (Barcode) وإرجاع مسار الملف.
        /// </summary>
        /// <param name="text">النص المراد تحويله.</param>
        /// <param name="outputPath">مسار الحفظ.</param>
        public static void GenerateBarcodeToFile(this string text, string outputPath)
        {
            var image = text.GenerateBarcode();
            SaveImageToFile(image, outputPath);
        }

        /// <summary>
        /// تحويل النصوص إلى QR ورموز شريطية بكميات كبيرة.
        /// </summary>
        /// <param name="texts">قائمة النصوص.</param>
        /// <param name="outputDirectory">مسار المجلد للحفظ.</param>
        /// <param name="type">النوع (QR أو Barcode).</param>
        public static void BatchGenerate(this List<string> texts, string outputDirectory, string type = "QRCode")
        {
            Directory.CreateDirectory(outputDirectory);
            foreach (var text in texts)
            {
                var fileName = Path.Combine(outputDirectory, $"{text}.{type}.png");
                if (type == "QRCode")
                {
                    text.GenerateQRCode(fileName);
                }
                else if (type == "Barcode")
                {
                    text.GenerateBarcodeToFile(fileName);
                }
            }
        }
    }
}

