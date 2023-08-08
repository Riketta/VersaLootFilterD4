using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using static VersaScreenCapture.Interop;
using Composition.WindowsRuntimeHelpers;
using System.Linq;
using Windows.Foundation.Metadata;

namespace VersaScreenCapture
{
    // https://github.com/TheBlackPlague/DynoSharp

    public static class CaptureHandler
    {
        private static Direct3D11CaptureFramePool CaptureFramePool = null;
        private static GraphicsCaptureItem CaptureItem = null;
        private static GraphicsCaptureSession CaptureSession = null;

        private static readonly Device CaptureDevice = new Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport);

        public static bool FrameCaptured { get; private set; }
        public static bool IsCapturing { get; private set; }

        public static Device GraphicCaptureDevice()
        {
            return CaptureDevice;
        }

        public static bool IsAvailable()
        {
            if (!ApiInformation.IsTypePresent("Windows.Graphics.Capture.GraphicsCaptureItem"))
            {
                Console.WriteLine("Using Windows.Graphics.Capture not possible in current system!");
                return false;
            }
            else if (!GraphicsCaptureSession.IsSupported())
            {
                Console.WriteLine("GraphicsCaptureSession not supported in current system!");
                return false;
            }

            return true;
        }

        public static void Stop()
        {
            CaptureSession.Dispose();
            CaptureFramePool.Dispose();
            CaptureSession = null;
            CaptureFramePool = null;
            CaptureItem = null;
            IsCapturing = false;
        }

        private static IDirect3DDevice CreateCaptureDevice()
        {
            uint direct3D11DevicePointer = NativeMethodHandler.CreateDirect3D11DeviceFromDXGIDevice(
                CaptureDevice.NativePointer,
                out IntPtr graphicDevice
            );

            if (direct3D11DevicePointer != 0)
            {
                throw new InvalidProgramException("Native pointer pointed to wrong device.");
            }

            IDirect3DDevice windowsRuntimeDevice = (IDirect3DDevice)Marshal.GetObjectForIUnknown(graphicDevice) ??
                                                   throw new InvalidCastException();
            Marshal.Release(graphicDevice);

            return windowsRuntimeDevice;
        }

        private static void StartCapture(GraphicsCaptureItem capture)
        {
            CaptureItem = capture;
            CaptureItem.Closed += CaptureItemOnClosed;

            IDirect3DDevice windowsRuntimeDevice = CreateCaptureDevice();


            CaptureFramePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
                windowsRuntimeDevice,
                DirectXPixelFormat.B8G8R8A8UIntNormalized,
                2, // total frames in frame pool
                CaptureItem.Size // size of each frame
                );

            CaptureSession = CaptureFramePool.CreateCaptureSession(CaptureItem);
            //CaptureSession.IsBorderRequired = true;
            CaptureSession.IsCursorCaptureEnabled = false;

            CaptureFramePool.FrameArrived += (sender, arguments) =>
            {
                AddFrame();
            };

            CaptureSession.StartCapture();
            IsCapturing = true;
        }


        public static void StartWindowCapture(IntPtr windowHandle)
        {
            StartCapture(CaptureHelper.CreateItemForWindow(windowHandle));
        }

        public static void StartMonitorCapture(IntPtr hmon)
        {
            StartCapture(CaptureHelper.CreateItemForMonitor(hmon));
        }

        public static void StartPrimaryMonitorCapture()
        {
            MonitorInfo monitor = (from m in MonitorEnumerationHelper.GetMonitors()
                                   where m.IsPrimary
                                   select m).First();
            StartMonitorCapture(monitor.Hmon);
        }

        private static void AddFrame()
        {
            /*
            if (Stopwatch.IsRunning && FrameCaptured) {
                Stopwatch.Stop();

                CaptureFps = (uint)(1000 / Stopwatch.ElapsedMilliseconds);
                // Console.WriteLine("FPS: " + CaptureFps);
            }
            */

            LatestFrame.FreeRuntimeResources();
            // LatestFrame?.Dispose();
            // LatestFrame = GetNextFrame();
            LatestFrame.AddFrame(GetNextFrame());
            FrameCaptured = true;
            // Stopwatch.Reset();
            // Stopwatch.Start();
        }

        private static Direct3D11CaptureFrame GetNextFrame()
        {
            if (!IsCapturing)
            {
                Console.WriteLine("[EXCEPTION OCCURED]: Can't get frame without capture process.");
                //throw new InvalidOperationException("Can't get frame without capture process.");
                return null;
            }

            return CaptureFramePool.TryGetNextFrame();
        }

        //private static GraphicsCaptureItem CreateItemForWindow(IntPtr highlightedWindow)
        //{
        //    IActivationFactory factory = WindowsRuntimeMarshal.GetActivationFactory(typeof(GraphicsCaptureItem));
        //    IGraphicsCaptureItemInterop interop = (IGraphicsCaptureItemInterop)factory;
        //    Type graphicsCaptureItemInterface = typeof(GraphicsCaptureItem).GetInterface("IGraphicsCaptureItem") ??
        //                                        throw new InvalidCastException();
        //    IntPtr pointer = interop.CreateForWindow(highlightedWindow, graphicsCaptureItemInterface.GUID);
        //    GraphicsCaptureItem capture = Marshal.GetObjectForIUnknown(pointer) as GraphicsCaptureItem ??
        //                                  throw new InvalidCastException();
        //    Marshal.Release(pointer);
        //    return capture;
        //}

        private static void CaptureItemOnClosed(GraphicsCaptureItem sender, object eventArgs)
        {
            Stop();
        }
    }
}
