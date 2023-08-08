using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VersaScreenCapture;

namespace VersaLootFilterD4
{
    internal class Program
    {
        static readonly string Title = string.Format("Versa Diablo IV Loot Filter ver. {0}", Assembly.GetEntryAssembly().GetName().Version.ToString());

        static WinAPI.VirtualKeys DefaultKey = WinAPI.VirtualKeys.Numpad0;
        static WinAPI.VirtualKeys QuickKey = WinAPI.VirtualKeys.Numpad1;

        static bool Debug()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            CaptureHandler.IsAvailable();

            var tooltips = TemplateMatching.MatchTestSamplesD4();
            foreach (var tooltip in tooltips)
                TooltipParser.ProcessItemTooltipImage(tooltip, true);

            Console.ReadLine();
            return true;
        }

        // TODO:
        // stop on 3 fails (empty bag slots)
        // prescan empty bag slots with OpenCV
        // manual cancel with button

        static void Main(string[] args)
        {
            //if (Debug()) return;

            Console.Title = Title;
            Console.WriteLine("### {0} ###", Title);

            Console.WriteLine("Parsing key...");
            WinAPI.VirtualKeys key = DefaultKey;

            Console.WriteLine("Getting process");
            Process[] processes = Process.GetProcesses();
            Process process = processes.First(p => p.ProcessName == "Diablo IV" && p.MainWindowTitle == "Diablo IV");
            if (process == null)
            {
                Console.WriteLine("No process found!");
                return;
            }
            process.EnableRaisingEvents = true;
            process.Exited += ProcessExitedCallback;

            Console.WriteLine("Getting handle");
            Actions.SetWindowHandle(process.MainWindowHandle);
            GameManager.MainWindowHandle = process.MainWindowHandle;
            Console.WriteLine("Process handle: {0}", GameManager.MainWindowHandle);

            Console.WriteLine("Main loop...");
            while (true)
            {
                if (!WinManager.IsWindowInFocus(GameManager.MainWindowHandle))
                    continue;

                if (WinManager.IsKeyPressed(DefaultKey))
                    Start();
                else if (WinManager.IsKeyPressed(QuickKey))
                    Start(true);
            }
        }

        static void Start(bool quick = false)
        {
            //Console.WriteLine("ScreenCapture analyze started");

            //CaptureHandler.StartPrimaryMonitorCapture(); // TODO: fix to window capture
            CaptureHandler.StartWindowCapture(GameManager.MainWindowHandle);
            TemplateMatching.DiabloTooltipDetectionLoop(quick ? null : Actions.IterateOverBag(), TooltipParser.ProcessItemTooltipImage);
            CaptureHandler.Stop();

            //Console.WriteLine("ScreenCapture analyze done");
        }

        private static void ProcessExitedCallback(object sender, EventArgs e)
        {
            Console.WriteLine("Target process exited");
            Environment.Exit(0);
        }
    }
}
