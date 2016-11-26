using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;

using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Device = SharpDX.Direct3D11.Device;
using Factory = SharpDX.DXGI.Factory;

class SharpDxHelper
{
    public SwapChain swapChain;
    private Device device;
    private Texture2D backBuffer;
    private Factory factory;
    private RenderTargetView renderView;

    public RenderTarget CreateRenderTarget(RenderForm form)
    {
        var desc = new SwapChainDescription()
        {
            BufferCount = 1,
            ModeDescription =
                       new ModeDescription(form.ClientSize.Width, form.ClientSize.Height,
                                           new Rational(60, 1), Format.R8G8B8A8_UNorm),
            IsWindowed = true,
            OutputHandle = form.Handle,
            SampleDescription = new SampleDescription(2, 0),
            SwapEffect = SwapEffect.Discard,
            Usage = Usage.RenderTargetOutput
        };


        Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, new SharpDX.Direct3D.FeatureLevel[] { SharpDX.Direct3D.FeatureLevel.Level_10_0 }, desc, out device, out swapChain);

        var d2dFactory = new SharpDX.Direct2D1.Factory();

        int width = form.ClientSize.Width;
        int height = form.ClientSize.Height;

        var rectangleGeometry = new RoundedRectangleGeometry(d2dFactory, new RoundedRectangle() { RadiusX = 32, RadiusY = 32, Rect = new RectangleF(128, 128, width - 128 * 2, height - 128 * 2) });
        factory = swapChain.GetParent<Factory>();
        factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);

        backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
        renderView = new RenderTargetView(device, backBuffer);

        Surface surface = backBuffer.QueryInterface<Surface>();

        RenderTarget d2dRenderTarget = new RenderTarget(d2dFactory, surface, new RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(Format.Unknown, AlphaMode.Premultiplied)));
        return d2dRenderTarget;
    }

    public void Destroy()
    {
        swapChain.Dispose();
        backBuffer.Dispose();
        device.ImmediateContext.ClearState();
        device.ImmediateContext.Flush();
        device.Dispose();
        renderView.Dispose();
        factory.Dispose();
    }
}

