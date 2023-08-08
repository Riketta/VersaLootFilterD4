//using Composition.WindowsRuntimeHelpers;
//using Microsoft.Graphics.Canvas;
//using Microsoft.Graphics.Canvas.UI.Composition;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Drawing.Imaging;
//using System.IO;
//using System.Linq;
//using System.Numerics;
//using System.Runtime.InteropServices;
//using System.Runtime.InteropServices.WindowsRuntime;
//using System.Text;
//using System.Threading.Tasks;
//using VersaScreenCapture;
//using Windows.Foundation.Metadata;
//using Windows.Graphics;
//using Windows.Graphics.Capture;
//using Windows.Graphics.DirectX;
//using Windows.Graphics.DirectX.Direct3D11;
//using Windows.Graphics.Imaging;
//using Windows.System;
//using Windows.UI;
//using Windows.UI.Composition;
//using Windows.UI.Xaml;
//using Windows.UI.Xaml.Hosting;

//namespace VersaScreenCapture
//{
//    // https://github.com/microsoft/Windows.UI.Composition-Win32-Samples/tree/master/dotnet/WPF/ScreenCapture
//    // https://github.com/TheBlackPlague/DynoSharp
//    // https://github.com/robmikh/Win32CaptureSample
//    // https://github.com/robmikh/SimpleRecorder
//    // https://github.com/robmikh/WPFCaptureSample
//    // https://github.com/robmikh/UWPCaptureSample

//    public static class ScreenCapture
//    {
//        public static Capture CurrentCapture = null;

//        public class Capture : IDisposable
//        {

//            private GraphicsCaptureItem item;
//            private Direct3D11CaptureFramePool framePool;
//            private GraphicsCaptureSession session;
//            private SizeInt32 lastSize;

//            private IDirect3DDevice device;
//            private SharpDX.Direct3D11.Device d3dDevice;
//            private SharpDX.DXGI.SwapChain1 swapChain;

//            public Capture(IDirect3DDevice d, GraphicsCaptureItem i)
//            {
//                item = i;
//                device = d;
//                d3dDevice = Direct3D11Helper.CreateSharpDXDevice(device);

//                var dxgiFactory = new SharpDX.DXGI.Factory2();
//                var description = new SharpDX.DXGI.SwapChainDescription1()
//                {
//                    Width = item.Size.Width,
//                    Height = item.Size.Height,
//                    Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
//                    Stereo = false,
//                    SampleDescription = new SharpDX.DXGI.SampleDescription()
//                    {
//                        Count = 1,
//                        Quality = 0
//                    },
//                    Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
//                    BufferCount = 2,
//                    Scaling = SharpDX.DXGI.Scaling.Stretch,
//                    SwapEffect = SharpDX.DXGI.SwapEffect.FlipSequential,
//                    AlphaMode = SharpDX.DXGI.AlphaMode.Premultiplied,
//                    Flags = SharpDX.DXGI.SwapChainFlags.None
//                };
//                swapChain = new SharpDX.DXGI.SwapChain1(dxgiFactory, d3dDevice, ref description);

//                framePool = Direct3D11CaptureFramePool.Create(
//                    device,
//                    DirectXPixelFormat.B8G8R8A8UIntNormalized,
//                    2,
//                    i.Size);
//                session = framePool.CreateCaptureSession(i);
//                session.IsBorderRequired = true;
//                lastSize = i.Size;

//                framePool.FrameArrived += OnFrameArrived;
//            }

//            public void Dispose()
//            {
//                session?.Dispose();
//                framePool?.Dispose();
//                swapChain?.Dispose();
//                d3dDevice?.Dispose();
//            }

//            public void StartCapture()
//            {
//                session.StartCapture();
//            }

//            public ICompositionSurface CreateSurface(Compositor compositor)
//            {
//                return compositor.CreateCompositionSurfaceForSwapChain(swapChain);
//            }

//            private void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
//            {
//                var newSize = false;

//                using (var frame = sender.TryGetNextFrame())
//                {
//                    if (frame.ContentSize.Width != lastSize.Width ||
//                        frame.ContentSize.Height != lastSize.Height)
//                    {
//                        // The thing we have been capturing has changed size.
//                        // We need to resize the swap chain first, then blit the pixels.
//                        // After we do that, retire the frame and then recreate the frame pool.
//                        newSize = true;
//                        lastSize = frame.ContentSize;
//                        swapChain.ResizeBuffers(
//                            2,
//                            lastSize.Width,
//                            lastSize.Height,
//                            SharpDX.DXGI.Format.B8G8R8A8_UNorm,
//                            SharpDX.DXGI.SwapChainFlags.None);
//                    }

//                    using (var backBuffer = swapChain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0))
//                    using (var bitmap = Direct3D11Helper.CreateSharpDXTexture2D(frame.Surface))
//                    {
//                        d3dDevice.ImmediateContext.CopyResource(bitmap, backBuffer);

//                        // ProcessFrame(frame);
//                    }

//                } // Retire the frame.

//                swapChain.Present(0, SharpDX.DXGI.PresentFlags.None);

//                if (newSize)
//                {
//                    framePool.Recreate(
//                        device,
//                        DirectXPixelFormat.B8G8R8A8UIntNormalized,
//                        2,
//                        lastSize);
//                }
//            }
//        }

//        public static void StartHwndCapture(IntPtr hwnd)
//        {
//            GraphicsCaptureItem item = CaptureHelper.CreateItemForWindow(hwnd);
//            if (item != null)
//                StartCaptureFromItem(item);
//        }

//        public static void StartHmonCapture(IntPtr hmon)
//        {
//            GraphicsCaptureItem item = CaptureHelper.CreateItemForMonitor(hmon);
//            if (item != null)
//                StartCaptureFromItem(item);
//        }

//        public static void StartPrimaryMonitorCapture()
//        {
//            MonitorInfo monitor = (from m in MonitorEnumerationHelper.GetMonitors()
//                                   where m.IsPrimary
//                                   select m).First();
//            StartHmonCapture(monitor.Hmon);
//        }

//        public static Capture StartCaptureFromItem(GraphicsCaptureItem item)
//        {
//            IDirect3DDevice device = Direct3D11Helper.CreateDevice();
//            Capture capture = new Capture(device, item);

//            CurrentCapture = capture;
//            capture.StartCapture();
//            return capture; // thread may be blocked
//        }

//        public static void StopCapture(Capture capture)
//        {
//            capture?.Dispose();
//        }

//        public static bool IsAvailable()
//        {
//            if (!ApiInformation.IsTypePresent("Windows.Graphics.Capture.GraphicsCaptureItem"))
//            {
//                Console.WriteLine("Using Windows.Graphics.Capture not possible in current system!");
//                return false;
//            }
//            else if (!GraphicsCaptureSession.IsSupported())
//            {
//                Console.WriteLine("GraphicsCaptureSession not supported in current system!");
//                return false;
//            }

//            return true;
//        }
//    }
//}
