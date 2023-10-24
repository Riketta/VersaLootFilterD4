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
        private static readonly string Title = string.Format("Versa Diablo IV Loot Filter ver. {0}", Assembly.GetEntryAssembly().GetName().Version.ToString());

        private static readonly WinAPI.VirtualKeys DefaultFilterKey = WinAPI.VirtualKeys.Numpad0;
        private static readonly WinAPI.VirtualKeys SingleItemFilterKey = WinAPI.VirtualKeys.Numpad1;
        private static readonly WinAPI.VirtualKeys DebugSingleItemFilterKey = WinAPI.VirtualKeys.Numpad2;

        private static bool IsDebug = false;

        static bool Debug()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            CaptureHandler.IsAvailable();

            var tooltips = FrameProcessor.MatchTestSamplesD4();
            foreach (var tooltip in tooltips)
                OCR.Parse(tooltip, true);

            Console.ReadLine();
            return true;
        }

        // TODO:
        // count stats of 700+ item power items: should be exactly 4 stats, not including armor and weapon damage.
        // prescan empty bag slots with OpenCV.
        // manual cancel with button.

        static void Main(string[] args)
        {
            //if (Debug()) return;

            Console.Title = Title;
            Console.WriteLine("### {0} ###", Title);

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

                if (WinManager.IsKeyPressed(DefaultFilterKey))
                    StartFilter();
                else if (WinManager.IsKeyPressed(SingleItemFilterKey))
                    StartFilter(true);
                else if (WinManager.IsKeyPressed(DebugSingleItemFilterKey))
                {
                    bool oldDebug = IsDebug;
                    IsDebug = true;
                    StartFilter(true);
                    IsDebug = oldDebug;
                }
            }
        }

        static void StartFilter(bool parseSingleTooltip = false)
        {
            //Console.WriteLine("ScreenCapture analyze started");

            //CaptureHandler.StartPrimaryMonitorCapture(); // TODO: fix to window capture
            CaptureHandler.StartWindowCapture(GameManager.MainWindowHandle);
            int renderDelay = parseSingleTooltip ? 0 : 200;
            var itemIter = parseSingleTooltip ? Enumerable.Range(1, 1) : Actions.IterateOverBag();

            int fails = 0;
            int failsMax = 3;
            int itemsTextParsed = 0;
            long totalTextParsingTime = 0;
            foreach (int itemIndex in itemIter)
            {
                if (renderDelay > 0)
                    Thread.Sleep(renderDelay); // let tooltip render

                Bitmap tooltipImage = null;
                try
                {
                    tooltipImage = FrameProcessor.GetDiabloTooltip(IsDebug);
                }
                catch (Exception ex)
                {
                    Logger.WriteLineInColor(ConsoleColor.Red, $"Exception occurred: {ex}");
                }
                if (tooltipImage is null) // empty bag slot or error
                {
                    Console.WriteLine($"Failed to detect tooltip! Skipping.");
                    
                    // TODO: fix white and blue items not being parsed and counts as "empty slot" due to not passing threshold
                    fails++;
                    if (fails == failsMax)
                        break;

                    continue;
                }

                //OCR.BruteFilters(tooltipImage); // use to correct OCR parsing filters
                long startTime = Stopwatch.GetTimestamp();
                List<string> tooltipStringArr = OCR.Parse(tooltipImage, IsDebug);
                long elapsedTime = Stopwatch.GetTimestamp() - startTime;
                if (IsDebug)
                {
                    itemsTextParsed++;
                    totalTextParsingTime += elapsedTime;
                    Logger.WriteLineInColor(ConsoleColor.Cyan, $"### Tooltip text parsed in: {elapsedTime / 10_000} ms");
                }

                Item item = null;
                try
                {
                    item = TooltipParser.Parse(tooltipStringArr, IsDebug);
                }
                catch (Exception ex)
                {
                    Logger.WriteLineInColor(ConsoleColor.Red, $"Exception occurred: {ex}");
                }
                if (item is null)
                {
                    Console.WriteLine("Failed to parse item! Skipping.");
                    continue;
                }

                Logger.WriteLineInColor(ConsoleColor.DarkYellow, item);

                var result = LootFilter.FilterItem(item);
                switch (result)
                {
                    case LootFilter.Result.Junk:
                        Actions.MarkAsJunk();
                        //Actions.SellItem();
                        break;

                    case LootFilter.Result.Keep:
                        Actions.MarkAsFavourite();
                        break;
                }

                fails = 0; // TODO: reset only if not error?

                Console.WriteLine("=================================================");
            }
            CaptureHandler.Stop();

            if (IsDebug)
                Logger.WriteLineInColor(ConsoleColor.Cyan, $"### Average time text parsed in: {totalTextParsingTime / (itemsTextParsed > 0 ? itemsTextParsed : 1) / 10_000} ms");

            //Console.WriteLine("ScreenCapture analyze done");
        }

        private static void ProcessExitedCallback(object sender, EventArgs e)
        {
            Console.WriteLine("Target process exited");
            Environment.Exit(0);
        }
    }
}
