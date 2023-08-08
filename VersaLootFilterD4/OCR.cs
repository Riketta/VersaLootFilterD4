using IronOcr;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Resources.ResXFileRef;

namespace VersaLootFilterD4
{
    internal class OCR // Optical Character Recognition
    {
        private static IronTesseract IronTesseract = new IronTesseract();
        public static char[] TrimStartChars = new char[] { '\r', '\n', ' ', '|', '©', '®', '°', '*', '¢', '&', '_', ']', '-' };

        static OCR()
        {
            IronTesseract.Language = OcrLanguage.EnglishBest;

            IronTesseract.Configuration.BlackListCharacters = "|©®°*¢&_";
            IronTesseract.Configuration.ReadBarCodes = false;
            IronTesseract.Configuration.RenderSearchablePdfsAndHocr = false;
            IronTesseract.Configuration.PageSegmentationMode = TesseractPageSegmentationMode.SparseText; // SparseTextOsd, SparseText

            IronTesseract.Configuration.TesseractVariables["user_defined_dpi"] = "70";
            IronTesseract.Configuration.TesseractVariables["debug_file"] = "NUL";
        }

        public static List<string> Parse(byte[] image)
        {
            List<string> result = new List<string>();

            using (var input = new OcrInput())
            {
                input.TargetDPI = 0;
                input.AddImage(image);
                input.Contrast(2.5f);
                input.Invert();
                // Strategy
                // ColorSpace
                // InputImageType
                // ColorDepth

                //input.SaveAsImages(); // TODO: DEBUG

                var ocrResult = IronTesseract.Read(input);
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
