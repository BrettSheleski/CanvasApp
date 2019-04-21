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
using System.Drawing;

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
        private async void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;

            canvas.Clear();

            Complex p1 = new Complex(-1.5, 1);
            Complex p2 = new Complex(0.5, -1);

            await DrawMandelbrotAsync(canvas, info, p1, p2);

        }

        Task DrawMandelbrotAsync(SKCanvas canvas, SKImageInfo info, Complex p1, Complex p2)
        {
            float minR = (float)Math.Min(p1.Real, p2.Real),
                maxR = (float)Math.Max(p1.Real, p2.Real),
                minI = (float)Math.Min(p1.Imaginary, p2.Imaginary),
                maxI = (float)Math.Max(p1.Imaginary, p2.Imaginary);

            float offsetX, offsetY, scaleX, scaleY;

            offsetX = info.Width / 2f;
            offsetY = info.Height / 2f;

            scaleX = info.Width / (maxR - minR);
            scaleY = info.Height / (maxI - minI);

            PointF center = new PointF((minR + maxR) / 2f, (minI + maxI) / 2f);

            float scale = Math.Min(scaleX, scaleY);

            canvas.Translate(offsetX, offsetY);
            canvas.Scale(scale);
            canvas.Translate(-center.X, -center.Y);

            SKMatrix iMatrix;
            SKPoint p;
            if (canvas.TotalMatrix.TryInvert(out iMatrix))
            {
                p = iMatrix.MapPoint(0, 0);

                minR = p.X;
                minI = p.Y;

                p = iMatrix.MapPoint(info.Width, info.Height);

                maxR = p.X;
                maxI = p.Y;
            }

            const int MAX_ITERATION_COUNT = 10;

            SKColor[] colors = new SKColor[MAX_ITERATION_COUNT];

            Random rand = new Random(DateTime.Now.Millisecond);

            for (int i = 0; i < MAX_ITERATION_COUNT; ++i)
            {
                colors[i] = SKColor.FromHsl((float)rand.NextDouble() * 360, 255, 255);
            }

            long rank;

            float step = 1 / scale;
            for (float r = minR; r < maxR; r += step)
            {
                for (float i = minI; i < maxI; i += step)
                {
                    p = new SKPoint(r, i);

                    rank = GetRank(p.X, p.Y, MAX_ITERATION_COUNT);

                    if (rank == MAX_ITERATION_COUNT)
                    {
                        canvas.DrawPoint(p, Xamarin.Forms.Color.Black.ToSKColor());
                    }
                    else
                    {
                        canvas.DrawPoint(p, colors[rank]);
                    }
                }
            }


            return Task.CompletedTask;

            if (canvas.TotalMatrix.TryInvert(out iMatrix))
            {
                for (float x = 0; x < info.Width; ++x)
                {
                    for (float y = 0; y < info.Height; ++y)
                    {
                        p = iMatrix.MapPoint(x, y);

                        rank = GetRank(p.X, p.Y, MAX_ITERATION_COUNT);

                        if (rank == MAX_ITERATION_COUNT)
                        {
                            canvas.DrawPoint(p, Xamarin.Forms.Color.Black.ToSKColor());
                        }
                        else
                        {
                            canvas.DrawPoint(p, colors[rank]);
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        long GetRank(double real, double imaginary, long maxN)
        {
            double r = 0, i = 0, xnew, ynew;
            int n;

            for (n = 0; n < maxN; ++n)
            {
                xnew = r * r - i * i + real;
                ynew = 2 * r * i + imaginary;
                if (xnew * xnew + ynew * ynew > 4)
                    return (n);
                r = xnew;
                i = ynew;
            }

            return maxN;
        }

    }
}
