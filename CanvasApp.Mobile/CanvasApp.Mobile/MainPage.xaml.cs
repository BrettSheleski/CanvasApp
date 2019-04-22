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

            DrawMandelbrot(canvas, info);
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

        public static readonly BindableProperty CanvasBoundsProperty = BindableProperty.Create(nameof(CanvasBounds), typeof(RectangleF), typeof(MainPage), new RectangleF(new PointF(-1.5f, -1), new SizeF(2, 2)), BindingMode.TwoWay, propertyChanged: OnCanvasBoundsPropertyChanged);
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

        void DrawMandelbrot(SKCanvas canvas, SKImageInfo info)
        {

            int divisionsX = 1,
                divisionsY = 16,
                width = info.Width / divisionsX,
                height = info.Height / divisionsY;

            float complexWidth = CanvasBounds.Width / divisionsX,
                complexHeight = CanvasBounds.Height / divisionsY;

            List<Task<BitmapInfo>> tasks = new List<Task<BitmapInfo>>(divisionsY * divisionsX);

            
            for (int x = 0; x < divisionsX; ++x)
            {
                for (int y = 0; y < divisionsY; ++y)
                {
                    System.Drawing.Rectangle drawingRect = new System.Drawing.Rectangle(info.Rect.Left + (x * width), info.Rect.Top + (y * height), width, height);
                    RectangleF complexRect = new RectangleF(CanvasBounds.Left + (x * complexWidth), CanvasBounds.Top + (y * complexHeight), complexWidth, complexHeight);

                    tasks.Add(Task<BitmapInfo>.Run(() =>
                    {
                        return new BitmapInfo
                        {
                            Bitmap = GetMandelbrotBitmap(complexRect, drawingRect),
                            Point = new SKPoint(drawingRect.X, drawingRect.Y)
                        };
                    }));
                }
            }

            Task.WhenAll(tasks).Wait();

            foreach (var t in tasks)
            {
                canvas.DrawBitmap(t.Result.Bitmap, t.Result.Point);
            }
        }

        struct BitmapInfo
        {
            public SKBitmap Bitmap { get; set; }
            public SKPoint Point { get; set; }
        }

        
        public struct RectangleD
        {
            public RectangleD(decimal left, decimal top, decimal right, decimal bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public decimal Left { get; }
            public decimal Right { get;}

            public decimal Top { get; }

            public decimal Bottom { get; }

            public decimal Width { get { return Right - Left; } }
            public decimal Height { get { return Top - Bottom; } }

        }

        SKBitmap GetMandelbrotBitmap(RectangleF complexBounds, System.Drawing.Rectangle drawingBounds)
        {
            

            SKBitmap bmp = new SKBitmap(drawingBounds.Width, drawingBounds.Height);

            float stepR = complexBounds.Width / drawingBounds.Width,
                stepI = complexBounds.Height / drawingBounds.Height;

            float scaleR = drawingBounds.Width / complexBounds.Width,
                scaleI = drawingBounds.Height / complexBounds.Height;

            float step = Math.Max(stepR, stepI);

            PointF offset = new PointF(
                drawingBounds.X - (complexBounds.X * scaleR),
                drawingBounds.Y - (complexBounds.Y * scaleI)
                );

            var px = from y in Enumerable.Range(drawingBounds.Top, drawingBounds.Bottom)
                     from x in Enumerable.Range(drawingBounds.Left, drawingBounds.Right)
                     select GetColorFromRank(GetRank((x - offset.X) * step, (y - offset.Y) * step, _maxIterations), _colors);

            bmp.Pixels = px.ToArray();

            return bmp;
        }

        static readonly SKColor black = Xamarin.Forms.Color.Black.ToSKColor();

        SKColor GetColorFromRank(long rank, SKColor[] colors)
        {
            if (rank < colors.LongLength)
                return colors[rank];

            return black;
        }

        long GetRank(float real, float imaginary, long maxN)
        {
            float r = 0, i = 0, xnew, ynew;
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


        Dictionary<long, SKPoint> touchDictionary = new Dictionary<long, SKPoint>();
        long touchId = -1;
        SKPoint previousPoint;

        private void Canvas_Touch(object sender, SKTouchEventArgs e)
        {

            //switch (e.ActionType)
            //{
            //    case SKTouchAction.Pressed:
            //        touchDictionary[e.Id] = e.Location;
            //        break;
            //    case SKTouchAction.Moved:
            //        if (touchDictionary.ContainsKey(e.Id))
            //        {
            //            // Single-finger drag
            //            if (touchDictionary.Count == 1)
            //            {
            //                SKPoint prevPoint = touchDictionary[e.Id];

                            


            //                // Adjust the matrix for the new position
            //                matrix.TransX += point.X - prevPoint.X;
            //                matrix.TransY += point.Y - prevPoint.Y;
            //                Canvas.InvalidateSurface();
            //            }
            //            // Double-finger scale and drag
            //            else if (touchDictionary.Count >= 2)
            //            {
            //                // Copy two dictionary keys into array
            //                long[] keys = new long[touchDictionary.Count];
            //                touchDictionary.Keys.CopyTo(keys, 0);

            //                // Find index of non-moving (pivot) finger
            //                int pivotIndex = (keys[0] == e.Id) ? 1 : 0;

            //                // Get the three points involved in the transform
            //                SKPoint pivotPoint = touchDictionary[keys[pivotIndex]];
            //                SKPoint prevPoint = touchDictionary[e.Id];
            //                SKPoint newPoint = point;

            //                // Calculate two vectors
            //                SKPoint oldVector = prevPoint - pivotPoint;
            //                SKPoint newVector = newPoint - pivotPoint;

            //                // Scaling factors are ratios of those
            //                float scaleX = newVector.X / oldVector.X;
            //                float scaleY = newVector.Y / oldVector.Y;

            //                if (!float.IsNaN(scaleX) && !float.IsInfinity(scaleX) &&
            //                    !float.IsNaN(scaleY) && !float.IsInfinity(scaleY))
            //                {
            //                    // If something bad hasn't happened, calculate a scale and translation matrix
            //                    SKMatrix scaleMatrix =
            //                        SKMatrix.MakeScale(scaleX, scaleY, pivotPoint.X, pivotPoint.Y);

            //                    SKMatrix.PostConcat(ref matrix, scaleMatrix);
            //                    Canvas.InvalidateSurface();
            //                }
            //            }

            //            // Store the new point in the dictionary
            //            touchDictionary[e.Id] = e.Location;
            //        }

            //        break;

            //    case SKTouchAction.Released:
            //    case SKTouchAction.Cancelled:
            //        touchDictionary.Remove(e.Id);
            //        break;
            //}


            if (e.ActionType == SKTouchAction.Pressed)
            {
                float pw = e.Location.X / Canvas.CanvasSize.Width;
                float ph = e.Location.Y / Canvas.CanvasSize.Height;

                PointF center = new PointF(
                                        CanvasBounds.Left + (pw * CanvasBounds.Width),
                                        CanvasBounds.Top + (ph * CanvasBounds.Height)
                                        );

                float quarterWidth = CanvasBounds.Width / 4,
                    quarterHeight = CanvasBounds.Height / 4;

                

                CanvasBounds = new RectangleF(center.X - quarterWidth, center.Y - quarterHeight, 2 * quarterWidth, 2 * quarterHeight);

            }
        }
    }
}
