using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VersaLootFilterD4
{
    internal class Logger
    {
        public static void WriteLineInColor(ConsoleColor foregroundColor, object value)
        {
            WriteLineInColor(foregroundColor, value.ToString());
        }

        public static void WriteLineInColor(ConsoleColor foregroundColor, string format, params object[] arg)
        {
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(format, arg);
            Console.ResetColor();
        }
    }
}
