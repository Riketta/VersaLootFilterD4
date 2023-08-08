using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using SharpDX;
using Windows.Services.Maps;

namespace VersaScreenCapture
{
    public class TemplateMatching
    {
        static double Threshold = 0.60; // TODO: fix footer mat: remove sell/buy/equip text
        static Mat Template = null;
        static Mat TemplateTooltipHeader = new Mat(@"D:\Trashland\D4 OpenCV Samples\tooltip_header.png", ImreadModes.Color);
        static Mat TemplateTooltipFooter = new Mat(@"D:\Trashland\D4 OpenCV Samples\tooltip_footer.png", ImreadModes.Color);

        // for OCR
        static readonly int TooltipBorderOffsetTop = 20;
        static readonly int TooltipBorderOffsetBottom = 45;
        static readonly int TooltipBorderOffsetLeft = 10;
        static readonly int TooltipBorderOffsetRight = 5;
        static readonly int ItemIconWidth = 110;
        static readonly int ItemIconHeight = 130;

        static readonly int HorizontalScreenOffset = 850;
        static readonly Rect RightScreenHalf = new Rect(HorizontalScreenOffset, 0, 1920 - HorizontalScreenOffset, 1080);

        public static (Point?, double) MatchFrame(Mat frame)
        {
            var result = new Mat(frame.Rows - Template.Rows + 1, frame.Cols - Template.Cols + 1, MatType.CV_32FC1);
            Cv2.MatchTemplate(frame, Template, result, TemplateMatchModes.CCoeffNormed);

            Cv2.MinMaxLoc(result, out double minValue, out double maxValue, out Point minLocation, out Point maxLocation);

            if (maxValue >= Threshold)
                return (maxLocation, maxValue);

            return (null, 0);
        }

        public static List<System.Drawing.Bitmap> MatchTestSamplesD4()
        {
            string[] filesItems = Directory.GetFiles(@"D:\Trashland\D4 OpenCV Samples\Items");

            List<Mat> matsItems = new List<Mat>();

            var mode = ImreadModes.Color;

            filesItems.ToList().ForEach(file => matsItems.Add(new Mat(file, mode)));

            var tooltipHeader = new Mat(@"D:\Trashland\D4 OpenCV Samples\tooltip_header.png", mode);
            var tooltipFooter = new Mat(@"D:\Trashland\D4 OpenCV Samples\tooltip_footer.png", mode);

            Console.WriteLine("=== Item Tooltips ===");
            var tooltips = new List<System.Drawing.Bitmap>();
            foreach (var item in matsItems)
            {
                (_, Mat tooltip) = FindDiabloItemTooltip(item, tooltipHeader, tooltipFooter);
                tooltips.Add(tooltip.ToBitmap());
            }

            return tooltips;
        }

        static (double, Mat) FindDiabloItemTooltip(Mat frame, Mat header, Mat footer)
        {
            //Cv2.ImShow("Frame", frame);
            //Cv2.WaitKey();

            double minMaxValue = 1;
            Mat[] tooltipComponents = new Mat[] { header, footer };
            Rect[] tooltipComponentRegions = new Rect[tooltipComponents.Length];
            for (int i = 0; i < tooltipComponents.Length; i++)
            {
                Mat component = tooltipComponents[i];

                Mat matchComponentResult = new Mat(component.Rows, component.Cols, MatType.CV_32FC1);
                //var matchComponentResult = new Mat(frame.Rows - component.Rows + 1, frame.Cols - component.Cols + 1, MatType.CV_32FC1);
                Cv2.MatchTemplate(frame, component, matchComponentResult, TemplateMatchModes.CCoeffNormed);
                //Cv2.ImShow("Heat Map", matchComponentResult);
                //Cv2.WaitKey();

                double maxValue;
                Point maxLocation;
                Cv2.MinMaxLoc(matchComponentResult, out _, out maxValue, out _, out maxLocation);

                //Console.WriteLine(maxValue); // DEBUG
                if (maxValue < Threshold)
                    return (maxValue, null);

                if (maxValue < minMaxValue)
                    minMaxValue = maxValue;

                Rect rectangle = new Rect(new Point(maxLocation.X, maxLocation.Y), new Size(component.Width, component.Height));
                //Cv2.Rectangle(frame, rectangle, Scalar.LimeGreen, 3); // DEBUG
                tooltipComponentRegions[i] = rectangle;
            }
            //Console.WriteLine(minMaxValue); // DEBUG

            //Cv2.ImShow("Frame", frame);
            //Cv2.WaitKey();
            //Cv2.DestroyAllWindows();

            Point tooltipPosition = new Point(tooltipComponentRegions[0].X + TooltipBorderOffsetLeft, tooltipComponentRegions[0].Y + TooltipBorderOffsetTop);
            Size tooltipSize = new Size(tooltipComponentRegions[1].Right - tooltipPosition.X - TooltipBorderOffsetRight,
                                        tooltipComponentRegions[1].Bottom - tooltipPosition.Y - TooltipBorderOffsetBottom);
            Rect tooltipRegion = new Rect(tooltipPosition, tooltipSize);
            Cv2.Rectangle(frame, tooltipRegion, Scalar.Black, 20); // for OCR

            Point itemIconPosition = new Point(tooltipRegion.Right - ItemIconWidth, tooltipRegion.Top);
            Size itemIconSize = new Size(ItemIconWidth, ItemIconHeight);
            Rect itemIconRegion = new Rect(itemIconPosition, itemIconSize);
            Cv2.Rectangle(frame, itemIconRegion, Scalar.Black, -1); // for OCR

            Mat tooltip = null;
            if (minMaxValue >= Threshold)
            {
                tooltip = new Mat(frame, tooltipRegion);

                //Cv2.ImShow("Frame", tooltip);
                //Cv2.WaitKey();
                //Cv2.DestroyAllWindows();
            }

            return (minMaxValue, tooltip);
        }

        public delegate void ProcessItemTooltipBitmap(System.Drawing.Bitmap template, bool manual = false);
        public static void DiabloTooltipDetectionLoop(IEnumerable<int> bagIter, ProcessItemTooltipBitmap postProcess)
        {
            if (bagIter == null)
                DiabloDetectTooltip(postProcess, true);
            else
            {
                foreach (int bagSlot in bagIter)
                    DiabloDetectTooltip(postProcess);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postProcess"></param>
        /// <param name="manual">if true - parse one tooltip without chain (like for quick check of vendor item)</param>
        public static void DiabloDetectTooltip(ProcessItemTooltipBitmap postProcess, bool manual = false)
        {
            //Console.WriteLine("IDLE"); // DEBUG
            if (!manual)
                Thread.Sleep(200); // let tooltip render

            Mat frame = null;
            for (int i = 0; i < 5 || frame == null; i++)
                (frame, _, _) = LatestFrame.GetLatestFrameAsMat();
            if (frame == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("= Null Frame =");
                Console.ResetColor();

                return;
            }

            Mat fixedFrame;
            if (manual)
                fixedFrame = frame;
            else
                fixedFrame = new Mat(frame, RightScreenHalf);
            fixedFrame = fixedFrame.CvtColor(ColorConversionCodes.BGRA2BGR); // BGRA2GRAY
            frame.Dispose();

            //Cv2.ImShow("Frame", fixedFrame);
            //Cv2.WaitKey();
            //Cv2.DestroyAllWindows();

            (double value, Mat tooltip) = FindDiabloItemTooltip(fixedFrame, TemplateTooltipHeader, TemplateTooltipFooter);
            fixedFrame.Dispose();

            if (value < Threshold)
                return; // TODO: return yield error value?

            //Console.WriteLine("Found item tooltip!"); // DEBUG
            postProcess(tooltip.ToBitmap(), manual);
        }


        public static double ScreenshotCaptureSpeedTest(IntPtr handle)
        {
            // GDI - ~40 FPS
            // ScreenCapture - 144+ FPS

            double duration = 60000;
            int calls = 0;
            bool testing = true;

            Task task = new Task(() =>
            {
                while (testing)
                {
                    //var screenshot = GetMatScreenshot(handle); // GDI
                    (var screenshot, _, _) = LatestFrame.GetLatestFrameAsMat(); // ScreenCapture
                    if (screenshot == null)
                        continue;

                    //OpenCvSharp.Cv2.ImShow("Latest Frame", screenshot);
                    //OpenCvSharp.Cv2.WaitKey();

                    _ = screenshot.DataStart;
                    screenshot.Dispose();
                    calls++;
                }
            });

            task.Start();
            task.Wait(Convert.ToInt32(duration));
            testing = false;

            return calls / duration * 1000;
        }
    }
}