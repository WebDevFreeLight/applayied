
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using ZXing;
using ZXing.Common;
using ZXing.QrCode.Internal;
using ZXing.Rendering;

namespace Infrastructure.Extensions.Qrcode
{
    public static class QrcodeHelper
    {
        /// <summary>
        /// ?? ?????? ??? ????????? ???????
        /// </summary>
        /// <param name = "msg"> ????????? ???????? ?? ??? QR </param>
        /// <returns> </RETRESS>



        public static Bitmap Create(string msg)
        {
            var writer = new MultiFormatWriter();
            var hint = new Dictionary<EncodeHintType, object>();
            //???? ??? ????????? ??????? ??? ????? UTF 8



            hint.Add(EncodeHintType.CHARACTER_SET, "utf-8");
            //???? ????? ????? ????? ? ?????



            hint.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H);
            var bm = writer.encode(msg, BarcodeFormat.QR_CODE, 200, 200, hint);
            var barcodeFormat = BarcodeFormat.QR_CODE;
            int width = 300, height = 100;

            // Create Barcodewriter with Pixeldata


            var barcodeWriter = new BarcodeWriter<PixelData>
            {
                Format = barcodeFormat,  // Set the barcode format (for example, code 128)


                Options = new ZXing.Common.EncodingOptions
                {
                    Width = width,    // Set the barcode width


                    Height = height,  // Set the barcode height


                    Margin = 10       // Set the margin


                },
                Renderer = new ZXing.Rendering.PixelDataRenderer()  // Set the crossbar for Pixeldata


            };

            // Parking (pixels) is born


            var barcodePixelData = barcodeWriter.Write(msg);




            // Convert pixeldata into a bitnery (using System.Rawing)


            var barcodeBitmap = new Bitmap(barcodePixelData.Width, barcodePixelData.Height);

            // Convert each pixel in Pixeldata into pixels


            for (int y = 0; y < barcodePixelData.Height; y++)
            {
                for (int x = 0; x < barcodePixelData.Width; x++)
                {
                    // Calculate the index for the pixel in the bench


                    int pixelIndex = (y * barcodePixelData.Width + x) * 4; // Each pixel has 4 byte (BGRA)



                    // ??????? ??? BGRA


                    byte blue = barcodePixelData.Pixels[pixelIndex];
                    byte green = barcodePixelData.Pixels[pixelIndex + 1];
                    byte red = barcodePixelData.Pixels[pixelIndex + 2];
                    byte alpha = barcodePixelData.Pixels[pixelIndex + 3];

                    // Convert BGRA to Argb (because Bitmap expects Argb format)


                    var color = Color.FromArgb(alpha, red, green, blue);

                    // Set the pixels on a bit


                    barcodeBitmap.SetPixel(x, y, color);
                }
            }





            return barcodeBitmap;


            // var barcodewriter = new barcodewriter () ?



            // Return Barcodewriter.write (bm);


        }









        /// <summary>
        /// ???? ????? ??? ????????? ??????? ?? ??????
        /// </summary>
        /// <param name = "filename"> ???? ??? ?????? </param>
        /// <retents> ????? ??? QR ? ???? ????? ?????? ??????? ??????? </returns>


        // public static string Read(string filename)
        // {
        //     if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
        //         return string.Empty;

        //     try
        //     {
        //         // Create a quick response code reader with the Z Xing Library



        //         var barcodeReader = new BarcodeReader<Bitmap>(bitmap => new BitmapLuminanceSource(bitmap));

        //         var reader = new BarcodeReader<Bitmap>
        //         {
        //             Options = new DecodingOptions
        //             {
        //                 CharacterSet = "UTF-8",
        //                 TryHarder = true // Empowering a more stringent deciphering mode


        //             }
        //         };

        //         // Use a use to ensure the BitMAP resources are used properly


        //         using (var bitmap = new Bitmap(filename))
        //         {
        //             var result = reader.Decode(bitmap);
        //             return result?.Text ?? string.Empty;
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         // Expectations are advised to register on actual projects
        //         // Here simply return an empty chain to preserve the original behavior


        //         return string.Empty;
        //     }
        // }












        /// <summary>
        /// ??? ????????? ???????/??? ?????? ????????
        /// </summary>
 
     //       There are more than 200 characters with translation, please reduce the translation content.
            public static Bitmap DrawLogo(Bitmap code, BitMatrix matrix, Bitmap logo )
            {
                if (code == null)
                {
                    throw new ArgumentNullException(nameof(code), "??Logo?,???????????!");
                }

                 

                if (matrix == null)
                {
                    throw new ArgumentNullException(nameof(matrix), "??Logo?,?????????????!");
                }

                if (logo == null)
                {
                    return code;
                }

                int[] matrixRectangle = matrix.getEnclosingRectangle();

               
 

                Bitmap result = new Bitmap(code.Width, code.Height
                // , PixelFormat.Format32bppArgb
                );
                using (Graphics graphics = Graphics.FromImage(result))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.DrawImage(code, 0, 0, code.Width, code.Height); 
                }

                return result;
            }
            

            /// <summary>
            /// ????? ??? ????????? ???????
            /// </summary>
            /// <param name = "content"> ????? ??? QR </param>
            /// <param name = "logo"> QR Code Icon ?????? ?????? </param>
            /// <param name = "configuration"> ?????? ??????? </param>
            /// <returns> </RETRESS>

            public static Bitmap GenerateQrCode(string content, Bitmap logo )
            {
               
                MultiFormatWriter formatter = new MultiFormatWriter();
                Dictionary<EncodeHintType, object> configs = new Dictionary<EncodeHintType, object>()
                {
                    // { EncodeHintType.CHARACTER_SET, configuration.Charset },
                    { EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.H },
                    // { EncodeHintType.MARGIN, configuration.Margin },
                    { EncodeHintType.DISABLE_ECI, true },
                };
                BitMatrix matrix = formatter.encode(content, BarcodeFormat.QR_CODE, 50,60, configs);
                BarcodeWriter<Bitmap> writer = new BarcodeWriter<Bitmap>();
                // writer.Renderer = new BitmapRenderer();
                Bitmap result = writer.Write(matrix);
                if (logo != null)
                {
                    result = DrawLogo(result, matrix, logo);
                }
                return result;
            }

          //  There are more than 200 characters with translation, please reduce the translation content.
            public static Bitmap GenerateQrCode(string content, string logoFile )
            {
                Bitmap logo = null;
                if (!string.IsNullOrEmpty(logoFile) && File.Exists(logoFile))
                {
                    logo = new Bitmap(logoFile);
                }
                return GenerateQrCode(content, logo);
            }

            /// <summary>
            /// <erneritdoc cref = "generateqrcode (????? ? ???? ????? ? QBCODECONFIGURATION)"/>
            /// </summary>
            /// <param name = "content"> ????? ??? QR </param>
            /// <param name = "configuration"> ?????? ??????? </param>
            /// <returns> </RETRESS>

            public static Bitmap GenerateQrCode(string content )
            {
                return GenerateQrCode(content, logo: null);
            }

            /// <summary>
            /// ????? ?????? ????????
            /// </summary>
            /// <param name = "content"> ????? ???????? </param>
            /// <param name = "configuration"> ?????? ??????? </param>
            /// <returns> </RETRESS>

            public static Bitmap GenerateBarCode(string content)
            {

#if NETSTANDARD2_0
            BarcodeWriter writer = new BarcodeWriter();
#elif NETCOREAPP3_1
            ZXing.BarcodeWriter<Bitmap> writer = new ZXing.BarcodeWriter<Bitmap>();
            writer.Renderer = new BitmapRenderer();
#else

            ZXing.BarcodeWriter<Bitmap> writer = new ZXing.BarcodeWriter<Bitmap>();
 


#endif
            writer.Format = BarcodeFormat.CODE_128;
                writer.Options = new EncodingOptions()
                {
                    Height = 50,
                    Width = 50,
                    Margin = 15
                };
                // writer.Options.Hints[EncodeHintType.CHARACTER_SET] = configuration.Charset;
                Bitmap result = writer.Write(content);
                return result;
            }

            /// <summary>
            /// <erneritdoc cref = "readcode (bitmap ? string)"/>
            /// </summary>
            /// <param name = "codeFile"> barcode/QR Code Picture Path Path </param>
            /// <param name = "charset"> ????? ??????? </param>
            /// <returns> </RETRESS>

            public static string ReadCode(this string codeFile, string charset = "utf-8")
            {
                if (string.IsNullOrEmpty(codeFile) || !File.Exists(codeFile))
                {
                    return null;
                }
                Bitmap bmp = new Bitmap(codeFile);
                return ReadCode(bmp, charset);
            }

            /// <summary>
            /// ???? ??? ????????/QR
            /// </summary>
            /// <param name = "code"> barcode/QR Code Data Data </param>
            /// <param name = "charset"> ????? ??????? </param>
            /// <returns> </RETRESS>

            public static string ReadCode(Bitmap code, string charset = "utf-8")
            {
                if (code == null)
                {
                    throw new ArgumentNullException(nameof(code), "Image resources do not exist!");
                }
#if NETSTANDARD2_0
            BarcodeReader reader = new BarcodeReader();
#elif NETCOREAPP3_1
            ZXing.BarcodeReader<Bitmap> reader = new ZXing.BarcodeReader<Bitmap>(null,(bitmap)=>new ZXing.BitmapLuminanceSource(bitmap),null);
#else
            ZXing.BarcodeReader<Bitmap> reader = new ZXing.BarcodeReader<Bitmap>((bitmap) =>   null);

#endif





            reader.Options = new DecodingOptions()
                {
                    CharacterSet = charset,
                    PossibleFormats = new List<BarcodeFormat>()
                {
                    BarcodeFormat.QR_CODE,
                    BarcodeFormat.CODE_128,
                    BarcodeFormat.CODE_39
                },
                    TryHarder = true,
                    PureBarcode = true
                };
                reader.AutoRotate = true;
                reader.TryInverted = true;
                var result = reader.Decode(code);
                if (result == null)
                {
                    return null;
                }
                return result.Text;
            }



        }
}

