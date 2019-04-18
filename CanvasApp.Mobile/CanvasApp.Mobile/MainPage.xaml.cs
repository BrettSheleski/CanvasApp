using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using SkiaSharp.Views.Forms;
using System.Numerics;
using SkiaSharp;

namespace CanvasApp.Mobile
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(true)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }
        private void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;

            canvas.Clear();

            foo(canvas, info);


        }

        void foo(SKCanvas canvas, SKImageInfo info)
        {
            double r = -1.5;
            double i = 0.5;



            float minR = -2, maxR = 2, minI = -2, maxI = -2;

            Complex z = new Complex(r, i);


            float offsetX, offsetY;

            offsetX = info.Width / 2;
            offsetY = info.Height / 2;

            using (SKPaint text = new SKPaint { Color = Color.Black.ToSKColor(), Style = SKPaintStyle.Stroke, StrokeWidth = 1, TextSize = 30 })
            using (SKPaint red = new SKPaint { Color = Color.Red.ToSKColor(), Style = SKPaintStyle.StrokeAndFill, StrokeWidth = 10, StrokeCap = SKStrokeCap.Round })
            using (SKPaint blue = new SKPaint())
            {
                blue.Style = SKPaintStyle.StrokeAndFill;
                blue.Color = Color.Blue.ToSKColor();
                blue.StrokeWidth = 10;

                var numbers = GetIteration(z, 1).TakeWhile((n, ix) => ix < 10 && z.Magnitude <= 2);

                SKPoint? lastPoint = null;
                SKPoint center;
                SKPoint textOffset = new SKPoint(15f, 15f);

                foreach (var number in numbers)
                {
                    center = new SKPoint(offsetX + (float)number.Real, offsetY + (float)number.Imaginary);

                    if (lastPoint.HasValue)
                    {
                        canvas.DrawLine(lastPoint.Value, center, red);
                        canvas.DrawText($"({number.Real},{number.Imaginary})", center + textOffset, text);
                    }

                    canvas.DrawCircle(center, 5, blue);

                    lastPoint = center;
                }
            }

        }

        IEnumerable<Complex> GetIteration(Complex z, Complex constant)
        {
            yield return z;

            while (true)
            {
                z = GetNext(z, constant);

                yield return z;
            }
        }

        Complex GetNext(Complex z, Complex constant)
        {
            return (z * z) + constant;
        }
    }
}
