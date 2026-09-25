using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace Portfolio.Business.Providers.Imaging
{
    /// <summary>
    /// A minimal RGBA image that fills polygons and saves as JPEG
    /// </summary>
    /// <remarks>
    /// Pixels start fully transparent, and fills are composited over them (source over).
    /// Polygon edges aren't antialiased, which suits straight edges on whole-pixel coordinates (e.g., leet patches).
    /// </remarks>
    public sealed class RasterImage
    {
        internal const int Channels = 4;

        private readonly byte[] _pixels;

        public RasterImage(int width, int height)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

            Width = width;
            Height = height;

            _pixels = new byte[width * height * Channels];
        }

        public int Width { get; }

        public int Height { get; }

        internal ReadOnlySpan<byte> Pixels => _pixels;

        public Color GetPixel(int x, int y)
        {
            var i = GetIndex(x, y);

            return Color.FromArgb(_pixels[i + 3], _pixels[i], _pixels[i + 1], _pixels[i + 2]);
        }

        /// <summary>
        /// Fills the polygon using the even-odd rule, including each pixel whose center lies inside it
        /// </summary>
        public void FillPolygon(Color color, IReadOnlyList<PointF> points)
        {
            ArgumentNullException.ThrowIfNull(points);

            if (points.Count < 3 || color.A == 0)
            {
                return;
            }

            var minY = double.MaxValue;
            var maxY = double.MinValue;
            foreach (var point in points)
            {
                minY = Math.Min(minY, point.Y);
                maxY = Math.Max(maxY, point.Y);
            }

            var top = Math.Max(0, (int)Math.Ceiling(minY - 0.5));
            var bottom = Math.Min(Height, (int)Math.Ceiling(maxY - 0.5));

            var crossings = new List<double>();

            for (int y = top; y < bottom; y++)
            {
                var sampleY = y + 0.5;

                crossings.Clear();
                for (int i = 0; i < points.Count; i++)
                {
                    var a = points[i];
                    var b = points[(i + 1) % points.Count];

                    // Half-open, so a vertex shared by two edges counts once and horizontal edges are skipped
                    if ((a.Y <= sampleY && sampleY < b.Y) || (b.Y <= sampleY && sampleY < a.Y))
                    {
                        crossings.Add(a.X + (sampleY - a.Y) * (b.X - a.X) / (b.Y - a.Y));
                    }
                }

                crossings.Sort();

                for (int i = 0; i + 1 < crossings.Count; i += 2)
                {
                    var left = Math.Max(0, (int)Math.Ceiling(crossings[i] - 0.5));
                    var right = Math.Min(Width, (int)Math.Ceiling(crossings[i + 1] - 0.5));

                    if (left < right)
                    {
                        BlendRun(_pixels.AsSpan((y * Width + left) * Channels, (right - left) * Channels), color);
                    }
                }
            }
        }

        public void SaveAsJpeg(Stream stream, int quality = JpegEncoder.DefaultQuality)
        {
            JpegEncoder.Encode(this, stream, quality);
        }

        private int GetIndex(int x, int y)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(x);
            ArgumentOutOfRangeException.ThrowIfNegative(y);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, Width);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(y, Height);

            return (y * Width + x) * Channels;
        }

        // Composites the color over a horizontal run of pixels
        private static void BlendRun(Span<byte> run, Color color)
        {
            byte r = color.R, g = color.G, b = color.B, a = color.A;

            ReadOnlySpan<byte> source = [r, g, b, a];
            var packed = MemoryMarshal.Read<uint>(source);

            if (a == byte.MaxValue)
            {
                MemoryMarshal.Cast<byte, uint>(run).Fill(packed);
                return;
            }

            var sourceAlpha = a / 255f;

            for (int i = 0; i < run.Length; i += Channels)
            {
                // Over a transparent pixel (e.g., a fresh image), the result is simply the color
                if (run[i + 3] == 0)
                {
                    MemoryMarshal.Write(run.Slice(i, Channels), in packed);
                    continue;
                }

                var destinationAlpha = run[i + 3] / 255f * (1 - sourceAlpha);
                var alpha = sourceAlpha + destinationAlpha;

                run[i] = Blend(r, run[i], sourceAlpha, destinationAlpha, alpha);
                run[i + 1] = Blend(g, run[i + 1], sourceAlpha, destinationAlpha, alpha);
                run[i + 2] = Blend(b, run[i + 2], sourceAlpha, destinationAlpha, alpha);
                run[i + 3] = (byte)Math.Round(alpha * 255);
            }
        }

        private static byte Blend(byte source, byte destination, float sourceAlpha, float destinationAlpha, float alpha)
        {
            return (byte)Math.Round((source * sourceAlpha + destination * destinationAlpha) / alpha);
        }
    }
}
