using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VersaLootFilterD4
{
    internal class Actions
    {
        public static readonly int BagWidth = 11;
        public static readonly int BagRows = 3;
        /// <summary>
        /// Item icon width + spacing between items in pixels
        /// </summary>
        public static readonly int ItemIconWidth = 55;
        /// <summary>
        /// Item icon height + spacing between items in pixels
        /// </summary>
        public static readonly int ItemIconHeight = 82;
        /// <summary>
        /// Offset to position of first item relative to bottom right corner with FullHD resolution
        /// </summary>
        public static readonly Point FirstItemOffsetFromBottomRight = new Point(-601 - (ItemIconWidth / 2), -279 - (ItemIconHeight / 2));

        private static IntPtr Handle = IntPtr.Zero;
        private static readonly string ErrorMessageNoHandle = "No target window handle specified!";

        public static int CursorOffset
        {
            get
            {
                cursorOffset *= -1;
                return cursorOffset;
            }
        }
        static int cursorOffset = -1;

        public static IEnumerable<int> IterateOverBag()
        {
            JumpToFirstItem();

            for (int row = 0; row < BagRows; row++)
            {
                for (int column = 0; column < BagWidth; column++)
                {
                    yield return (row * BagWidth) + column + 1;
                    JumpToNextItem();
                }

                JumpToFirstItem();
                JumpToNextRow(row + 1);
            }

            JumpToFirstItem();
        }

        public static void JumpToFirstItem()
        {
            Point position = new Point(1920 + FirstItemOffsetFromBottomRight.X, 1080 + FirstItemOffsetFromBottomRight.Y);
            WinManager.MoveMouseAndCrawlAround(position, CursorOffset);
        }

        public static void JumpToNextItem()
        {
            Point position = WinManager.GetMousePosition();
            position.X += ItemIconWidth;
            WinManager.MoveMouseAndCrawlAround(position, CursorOffset);
        }

        public static void JumpToNextRow(int times)
        {
            Point position = WinManager.GetMousePosition();
            position.Y += times * ItemIconHeight;
            WinManager.MoveMouseAndCrawlAround(position, CursorOffset);
        }

        public static void JumpToNextRow()
        {
            JumpToNextRow(0);
        }

        public static void SellItem(IntPtr handle)
        {
            WinManager.MouseClick(handle, true);
            Thread.Sleep(15);
        }

        public static void SellItem()
        {
            if (Handle != IntPtr.Zero)
                SellItem(Handle);
            else
                Console.WriteLine(ErrorMessageNoHandle);
        }

        public static void MarkAsJunk(IntPtr handle)
        {
            WinManager.PressKey(handle, WinAPI.VirtualKeys.Space);
            Thread.Sleep(15);
        }

        public static void MarkAsJunk()
        {
            if (Handle != IntPtr.Zero)
                MarkAsJunk(Handle);
            else
                Console.WriteLine(ErrorMessageNoHandle);
        }

        public static void SetWindowHandle(IntPtr handle)
        {
            Handle = handle;
        }
    }
}
