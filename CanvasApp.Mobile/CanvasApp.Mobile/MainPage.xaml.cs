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
            SetColors();
            InitializeComponent();
        }
        private void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;

            canvas.Clear();

            Complex p1 = new Complex(-1.5, 1);
            Complex p2 = new Complex(0.5, -1);

            var tasks = DrawMandelbrotAsync(canvas, info);

            Task.WaitAll(tasks.ToArray());
        }

        void SetColors()
        {

            SKColor[] colors = new SKColor[_maxIterations];

            _colors.CopyTo(colors, 0);

            Random rand = new Random(DateTime.Now.Millisecond);

            for (long i = _colors.LongLength; i < _maxIterations; ++i)
            {
                colors[i] = SKColor.FromHsl((float)rand.NextDouble() * 360, 255, 255);
            }

            _colors = colors;
        }

        public RectangleF CanvasBounds
        {
            get { return (RectangleF)GetValue(CanvasBoundsProperty); }
            set { SetValue(CanvasBoundsProperty, value); }
        }

        public static readonly BindableProperty CanvasBoundsProperty = BindableProperty.Create(nameof(CanvasBounds), typeof(RectangleF), typeof(MainPage), new RectangleF(new PointF(-1.5f, 1), new SizeF(2, -2)), BindingMode.TwoWay, propertyChanged: OnCanvasBoundsPropertyChanged);
        private SKMatrix _canvasInvertMatrix;
        private long _maxIterations = 100;

        SKColor[] _colors = new SKColor[] { };

        private static void OnCanvasBoundsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((MainPage)bindable).OnCanvasBoundsChanged((RectangleF)oldValue, (RectangleF)newValue);
        }

        protected virtual void OnCanvasBoundsChanged(RectangleF oldValue, RectangleF newValue)
        {
            Canvas.InvalidateSurface();
        }

        IEnumerable<Task> DrawMandelbrotAsync(SKCanvas canvas, SKImageInfo info)
        {
            Complex p1 = new Complex(CanvasBounds.Left, CanvasBounds.Top),
                p2 = new Complex(CanvasBounds.Right, CanvasBounds.Bottom);

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
            canvas.Scale(scale, -scale);
            canvas.Translate(-center.X, -center.Y);

            if (canvas.TotalMatrix.TryInvert(out _canvasInvertMatrix))
            {
                var pTemp = _canvasInvertMatrix.MapPoint(minR, maxI);

                minR = pTemp.X;
                maxI = pTemp.Y;


                pTemp = _canvasInvertMatrix.MapPoint(maxR, minI);

                maxR = pTemp.X;
                maxI = pTemp.Y;


            }


            float step = 1 / scale;

            int divisionsH = 4, divisionsW = 4;

            float chunkW = Math.Abs(CanvasBounds.Width / divisionsW);
            float chunkH = Math.Abs(CanvasBounds.Height / divisionsH);

            for (int h = 0; h < divisionsH; ++h)
            {
                for (int w = 0; w < divisionsW; ++w)
                {
                    // avoid closure issues.
                    int ww = w;
                    int hh = h;

                    
                    yield return Task.Run(() =>
                    {
                        float startR = minR + (ww * chunkW),
                            endR = startR + chunkW,
                            startI = minI + (hh * chunkH),
                            endI = startI + chunkH;


                        long rank;

                        for (float r = startR; r < endR; r += step)
                        {
                            for (float i = startI; i < endI; i += step)
                            {
                                rank = GetRank(r, i, _maxIterations);

                                if (rank == _maxIterations)
                                {
                                    canvas.DrawPoint(r, i, Xamarin.Forms.Color.Black.ToSKColor());
                                }
                                else
                                {
                                    canvas.DrawPoint(r, i, _colors[rank]);
                                }
                            }
                        }
                    });
                }
            }

            //for (float r = minR; r < maxR; r += step)
            //{
            //    for (float i = minI; i < maxI; i += step)
            //    {
            //        p = new SKPoint(r, i);

            //        rank = GetRank(p.X, p.Y, _maxIterations);

            //        if (rank == _maxIterations)
            //        {
            //            canvas.DrawPoint(p, Xamarin.Forms.Color.Black.ToSKColor());
            //        }
            //        else
            //        {
            //            canvas.DrawPoint(p, _colors[rank]);
            //        }
            //    }
            //}

            //return Task.CompletedTask;
        }

        long GetRank(double real, double imaginary, long maxN)
        {
            double r = 0, i = 0, xnew, ynew;
            long n;

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

        private void Canvas_Touch(object sender, SKTouchEventArgs e)
        {
            if (e.ActionType == SKTouchAction.Pressed)
            {
                //               _maxIterations *= 10;
                //             SetColors();

                var newCenter = _canvasInvertMatrix.MapPoint(e.Location);

                RectangleF b = new RectangleF(newCenter.X - CanvasBounds.Width / 4, newCenter.Y - CanvasBounds.Height / 4, CanvasBounds.Width / 2, CanvasBounds.Height / 2);


                this.CanvasBounds = b;
            }
        }
    }
}
