using IronOcr;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace VersaLootFilterD4
{
    internal class OCR // Optical Character Recognition
    {
        private static readonly IronTesseract IronTesseract = new IronTesseract();
        public static char[] TrimStartChars = new char[] { '\r', '\n', ' ', '|', '©', '®', '°', '*', '¢', '&', '_', ']', '-' };

        static OCR()
        {
            IronTesseract.Language = OcrLanguage.EnglishBest;

            IronTesseract.Configuration.BlackListCharacters = "|©®°*¢&_";
            IronTesseract.Configuration.ReadDataTables = false;
            IronTesseract.Configuration.ReadBarCodes = false;
            IronTesseract.Configuration.RenderSearchablePdfsAndHocr = false;
            IronTesseract.Configuration.PageSegmentationMode = TesseractPageSegmentationMode.SparseText; // SparseText, SingleColumn, SingleBlock

            IronTesseract.Configuration.TesseractVariables["user_defined_dpi"] = "70";
            IronTesseract.Configuration.TesseractVariables["debug_file"] = "NUL";
        }

        public static string BruteFilters(Bitmap image)
        {
            string filepath = "debug_image.png";
            image.Save(filepath, ImageFormat.Png);
            string result = OcrInputFilterWizard.Run(filepath, out double confidence, IronTesseract);

            File.WriteAllText(filepath + ".txt", result);

            return result;
        }

        public static List<string> Parse(Bitmap image, bool saveImageToFile, bool debug = false)
        {
            //ImageConverter converter = new ImageConverter();
            //image = (byte[])converter.ConvertTo(image, typeof(byte[]));

            List<string> result;

            long startTime = Stopwatch.GetTimestamp();
            using (var input = new OcrInput(image))
            {
                input.TargetDPI = 0;
                input.Contrast(2.5f);
                input.Invert();
                // Strategy
                // ColorSpace
                // InputImageType
                // ColorDepth
                
                long elapsedTime = Stopwatch.GetTimestamp() - startTime;
                if (debug)
                    Logger.WriteLineInColor(ConsoleColor.Cyan, $"### OcrInput: {elapsedTime / 10_000} ms");

                if (saveImageToFile)
                    input.SaveAsImages(DateTime.Now.Ticks.ToString());

                startTime = Stopwatch.GetTimestamp();
                var ocrResult = IronTesseract.Read(input);
                elapsedTime = Stopwatch.GetTimestamp() - startTime;
                if (debug)
                    Logger.WriteLineInColor(ConsoleColor.Cyan, $"### IronTesseract.Read: {elapsedTime / 10_000} ms");

                result = new List<string>(ocrResult.Lines.Length);
                foreach (var ocrLine in ocrResult.Lines)
                {
                    string line = ocrLine.Text.Trim().Replace('@', 'O');
                    if (line.Length < 3) continue;

                    result.Add(line.TrimStart(TrimStartChars));
                }
            }

            return result;
        }
    }
}
