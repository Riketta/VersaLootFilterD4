using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace VersaScreenCapture
{
    public class TemplateMatching
    {
        public static double ScreenshotCaptureSpeedTest(IntPtr handle)
        {
            // NVIDIA GeForce RTX 3060 Ti:
            // GDI - ~40 FPS
            // Win10 ScreenCapture - 144+ FPS

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