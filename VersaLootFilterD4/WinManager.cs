using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VersaLootFilterD4
{
    class WinManager
    {
        static Dictionary<WinAPI.VirtualKeys, bool> KeyboardState = new Dictionary<WinAPI.VirtualKeys, bool>();

        public static Process GetProcess(string process)
        {
            Process p = null;

            Process[] processes = Process.GetProcessesByName(process);
            if (processes.Length > 0)
                p = processes[0];

            return p;
        }

        public static void PressKey(IntPtr handle, WinAPI.VirtualKeys key)
        {
            WinAPI.SendMessage(handle, WinAPI.WM_KEYDOWN, (UInt32)key, IntPtr.Zero);
            WinAPI.SendMessage(handle, WinAPI.WM_KEYUP, (UInt32)key, IntPtr.Zero);
            //WinAPI.PostMessage(handle, WinAPI.WM_KEYDOWN, (UInt32)key, IntPtr.Zero);
            //WinAPI.PostMessage(handle, WinAPI.WM_KEYUP, (UInt32)key, IntPtr.Zero);
        }

        public static void MouseClick(IntPtr handle, bool RMB = false)
        {
            MouseClick(handle, 0, RMB);
        }

        public static void MouseClick(IntPtr handle, int holdTime, bool RMB = false)
        {
            WinAPI.mouse_event(RMB ? WinAPI.MOUSEEVENTF_RIGHTDOWN : WinAPI.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(holdTime);
            WinAPI.mouse_event(RMB ? WinAPI.MOUSEEVENTF_RIGHTUP : WinAPI.MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
        }

        public static bool IsKeyDown(WinAPI.VirtualKeys key)
        {
            return (1 & (WinAPI.GetKeyState(key) >> 15)) == 1;
        }

        public static bool IsKeyPressed(WinAPI.VirtualKeys key)
        {
            bool currentState = IsKeyDown(key);
            if (!KeyboardState.ContainsKey(key))
            {
                KeyboardState[key] = currentState;
                return currentState;
            }

            bool previousState = KeyboardState[key];
            KeyboardState[key] = currentState;

            return currentState && !previousState;
        }

        public static bool IsKeyReleased(WinAPI.VirtualKeys key)
        {
            bool currentState = IsKeyDown(key);
            if (!KeyboardState.ContainsKey(key))
            {
                KeyboardState[key] = currentState;
                return !currentState;
            }

            bool previousState = KeyboardState[key];
            KeyboardState[key] = currentState;

            return !currentState && previousState;
        }


        public static Point GetMousePosition()
        {
            return Cursor.Position;
        }

        public static void MoveMouse(Point position)
        {
            Cursor.Position = position;
        }

        public static void MoveMouseAndCrawlAround(Point position, int offset, int times = 6)
        {
            for (int i = 0; i < times; i++)
            {
                position.Y += offset;
                MoveMouse(position);
                Thread.Sleep(15);
            }
        }

        public static void SetWindowInFocus(IntPtr handle)
        {
            bool SyncShow = WinAPI.SetForegroundWindow(handle);
            bool ASyncShow = WinAPI.ShowWindowAsync(handle, 9); // SW_RESTORE = 9
        }

        public static bool IsWindowInFocus(IntPtr handle)
        {
            return handle == WinAPI.GetForegroundWindow();
        }

        public static Color GetPixelColor(IntPtr handle, int x, int y)
        {
            IntPtr hdc = WinAPI.GetWindowDC(handle);
            uint pixel = WinAPI.GetPixel(hdc, x, y);
            WinAPI.ReleaseDC(handle, hdc);
            Color color = Color.FromArgb((int)(pixel & 0x000000FF),
                            (int)(pixel & 0x0000FF00) >> 8,
                            (int)(pixel & 0x00FF0000) >> 16);
            return color;
        }

        public static Bitmap GetScreenshotGDI(IntPtr handle)
        {
            WinAPI.RECT rect;
            if (!WinAPI.GetWindowRect(handle, out rect))
            {
            }

            var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            var result = new Bitmap(bounds.Width, bounds.Height);

            using (var graphics = Graphics.FromImage(result))
                graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);

            return result;
        }

        public static Point FindWindowCenter(IntPtr handle)
        {
            WinAPI.RECT rct;

            if (!WinAPI.GetWindowRect(handle, out rct))
                return Point.Empty;

            int titleHeight = WinAPI.GetSystemMetrics(WinAPI.SM_CYCAPTION);

            int width = rct.Right - rct.Left + 1;
            int height = rct.Bottom - rct.Top + 1;
            //int x = rct.Left + width / 2;
            //int y = rct.Top + height / 2;
            int x = width / 2;
            int y = titleHeight + (height - titleHeight) / 2;
            return new Point(x, y);
        }

        public static Bitmap GetCurrentIcon()
        {
            Bitmap cursorIcon = null;

            WinAPI.CURSORINFO pci;
            pci.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(WinAPI.CURSORINFO));

            if (WinAPI.GetCursorInfo(out pci))
            {
                using (var icon = new Bitmap(32, 32))
                {
                    using (Graphics g = Graphics.FromImage(icon))
                        WinAPI.DrawIcon(g.GetHdc(), 0, 0, pci.hCursor);
                    cursorIcon = new Bitmap(icon).Clone() as Bitmap; // We have to "clone" icon because handle won't be released if we continue use it
                }
            }

            return cursorIcon;
        }

        public static void SetWindowFullscreen(IntPtr handle)
        {
            var rect = Screen.FromHandle(handle).Bounds;
            WinAPI.SetWindowPos(handle, handle, rect.X, rect.Y, rect.Width, rect.Height, WinAPI.SWP_SHOWWINDOW | WinAPI.SWP_FRAMECHANGED);
        }

        public static void MinimizeWindow(IntPtr handle)
        {
            WinAPI.ShowWindow(handle, WinAPI.SW_MINIMIZE);
        }

        public static void MaximizeWindow(IntPtr handle)
        {
            WinAPI.ShowWindow(handle, WinAPI.SW_MAXIMIZE);
        }

        public static void SetWindowStyleOff(IntPtr handle, int nIndex, long dwStylesToOff)
        {
            IntPtr windowStyles = GetWindowStyles(handle, nIndex);
            WinAPI.SetWindowLongPtr(handle, nIndex, ((windowStyles.ToInt64() | dwStylesToOff) ^ dwStylesToOff));
        }

        public static IntPtr GetWindowStyles(IntPtr handle, int nIndex)
        {
            return WinAPI.GetWindowLongPtr(handle, nIndex);
        }
    }
}
