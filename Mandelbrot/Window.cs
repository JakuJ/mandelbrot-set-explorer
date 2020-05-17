using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Mandelbrot
{
    /// <summary>
    /// Main rendering window class
    /// </summary>
    public sealed class Window : GameWindow
    {
        private enum MouseWheelMode
        {
            Resolution,
            ZoomFactor,
            Iterations,
            EscapeRadius
        };

        /// <summary>
        /// A <see cref="MouseWheelMode"/> instance directing which rendering parameter will the mouse wheel change.
        /// </summary>
        private MouseWheelMode _mode;

        /// <summary>
        /// The <see cref="MandelbrotSet"/> instance used to render the set.
        /// </summary>
        private readonly MandelbrotSet _mandelbrot;

        /// <summary>
        /// Current image <see cref="TextureTarget.Texture2D"/> id.
        /// </summary>
        private int _texture;

        /// <summary>
        /// The click zoom factor.
        /// </summary>
        private double _zoomFactor;

        /// <summary>
        /// The ratio of the resolution of the generated image to the resolution of the actual window.
        /// </summary>
        private int _resolution;

        /// <summary>
        /// Gets the width of the generated image.
        /// </summary>
        /// <value>The width of the generated image.</value>
        private int ImageWidth => Width * _resolution / 100;

        /// <summary>
        /// Gets the height of the generated image.
        /// </summary>
        /// <value>The height of the generated image.</value>
        private int ImageHeight => Height * _resolution / 100;

        /// <summary>
        /// The base title for the window.
        /// </summary>
        private readonly string _baseTitle;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Mandelbrot.Window"/> class.
        /// </summary>
        /// <param name="width">Window width.</param>
        /// <param name="height">Window height.</param>
        /// <param name="mandelbrot"><see cref="T:Mandelbrot.MandelbrotSet"/> instance used by this <see cref="T:Mandelbrot.Window"/></param>
        public Window(int width, int height, MandelbrotSet mandelbrot) : base(width, height)
        {
            _mandelbrot = mandelbrot;
            _baseTitle = "Mandelbrot Set";
            _mode = MouseWheelMode.ZoomFactor;
            _zoomFactor = 2;
            _resolution = 100;

            UpdateTitle();
            GL.Enable(EnableCap.Texture2D);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
        }

        protected override void OnLoad(EventArgs e)
        {
            CursorVisible = true;
            GenerateTexture(ImageWidth, ImageHeight);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            double factor = (e.Button == MouseButton.Left) ? _zoomFactor : 1 / _zoomFactor;

            _mandelbrot.Zoom(e.X / (double) Width, e.Y / (double) Height, factor);
            RedrawTexture();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            switch (_mode)
            {
                case MouseWheelMode.Resolution:
                    _resolution = Math.Max(25, _resolution - (25 * e.Delta));
                    break;
                case MouseWheelMode.EscapeRadius:
                    try
                    {
                        int newRadius = (int) (_mandelbrot.R / Math.Pow(2, e.Delta));
                        _mandelbrot.R = Math.Min(32768, Math.Max(2, newRadius));
                    }
                    catch (ArithmeticException)
                    {
                    }

                    break;
                case MouseWheelMode.Iterations:
                    _mandelbrot.N = Math.Max(25, _mandelbrot.N - (25 * e.Delta));
                    break;
                case MouseWheelMode.ZoomFactor:
                    _zoomFactor = Math.Max(1, _zoomFactor - e.Delta);
                    UpdateTitle();
                    return;
            }

            RedrawTexture();
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Key.Escape:
                    Exit();
                    break;
                case Key.Space:
                    _mode = _mode.Next();
                    UpdateTitle();
                    break;
                case Key.S:
                    SaveImage();
                    break;
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0, 0);
            GL.Vertex2(-1, 1);

            GL.TexCoord2(1, 0);
            GL.Vertex2(1, 1);

            GL.TexCoord2(1, 1);
            GL.Vertex2(1, -1);

            GL.TexCoord2(0, 1);
            GL.Vertex2(-1, -1);

            GL.End();

            SwapBuffers();
        }

        /// <summary>
        /// Saves the current image to the Screenshots folder.
        /// </summary>
        private void SaveImage()
        {
            Directory.CreateDirectory("Captured");
            Bitmap bmp = _mandelbrot.Render(ImageWidth, ImageHeight);
            bmp.Save($"Captured/{DateTime.Now.ToShortDateString()}:{DateTime.Now.ToLongTimeString()}.bmp",
                ImageFormat.Bmp);
        }

        /// <summary>
        /// Generates the image (<see cref="TextureTarget.Texture2D"/>) of the Mandelbrot set bounded by the internal parameters, with given resolution.
        /// </summary>
        /// <returns>The texture id.</returns>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        private void GenerateTexture(int width, int height)
        {
            Bitmap bmp = _mandelbrot.Render(width, height);

            _texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _texture);

            BitmapData data = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                bmp.PixelFormat
            );

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba8,
                data.Width, data.Height,
                0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte,
                data.Scan0);

            bmp.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int) TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int) TextureMagFilter.Linear);
        }

        /// <summary>
        /// Generates new texture using the <see cref="_mandelbrot"/> <see cref="MandelbrotSet"/> instance.
        /// </summary>
        private void RedrawTexture()
        {
            GL.DeleteTexture(_texture);

            DateTime start = DateTime.UtcNow;
            GenerateTexture(ImageWidth, ImageHeight);
            DateTime end = DateTime.UtcNow;

            UpdateTitle((end - start).TotalSeconds);
        }

        /// <summary>
        /// Updates the window title.
        /// </summary>
        /// <param name="timeElapsed">Last rendering time.</param>
        private void UpdateTitle(double timeElapsed = 0)
        {
            Title = string.Format(
                "{0} – Res: {1}% - Zoom: {2}x, 10^{7:F1} - Speed: {3:F3}s - N: {4} - R: {5} - Mode: {6}",
                _baseTitle,
                _resolution,
                _zoomFactor,
                timeElapsed,
                _mandelbrot.N,
                _mandelbrot.R,
                Enum.GetName(typeof(MouseWheelMode), _mode),
                Math.Log10(_mandelbrot.XMax - _mandelbrot.XMin));
        }
    }
}