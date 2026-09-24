using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using Portfolio.Business.Models;
using Portfolio.Business.Serializers;

namespace Portfolio.Business.Services
{
    public interface ILeetService
    {
        LeetStream CreateLeetStream();

        LeetStream CreateLeetStream(Leet leet);
    }

    public class LeetService(ISerializer<Shape> shapeSerializer)
                : ILeetService
    {
        private readonly ISerializer<Shape> _shapeSerializer = shapeSerializer;

        private static readonly Random random = new();
        private static readonly Lock syncLock = new();

        public LeetStream CreateLeetStream()
        {
            using var image = new Image<Rgba32>(Leet.Width, Leet.Height);

            var identifier = CreateRandomLeet();

            var patches = DeserializePatches(identifier);

            var leet = CreateLeet(identifier, patches);

            DrawPatches(image, patches);

            var stream = new MemoryStream();
            image.SaveAsJpeg(stream);

            return LeetStream.Create(identifier, leet.ToString(), stream);
        }

        public LeetStream CreateLeetStream(Leet leet)
        {
            if (leet == null)
            {
                return null;
            }

            var patches = DeserializePatches(leet.Identifier, leet.Geometries);
            if (patches == null)
            {
                return null;
            }

            using var image = new Image<Rgba32>(Leet.Width, Leet.Height);

            DrawPatches(image, patches);

            var stream = new MemoryStream();
            image.SaveAsJpeg(stream);

            return LeetStream.Create(leet.Identifier, leet.ToString(), stream);
        }

        protected virtual void DrawPatches(Image image, IEnumerable<Patch> patches)
        {
            image.Mutate(i => i.Paint(canvas =>
            {
                foreach (var p in patches)
                {
                    canvas.Draw(p.Pen, p.Polygon);
                    canvas.Fill(p.Brush, p.Polygon);
                }
            }));
        }

        protected virtual Color CreateColor()
        {
            var r = CreateRandomByte();
            var g = CreateRandomByte();
            var b = CreateRandomByte();
            var a = CreateRandomByte();

            return Color.FromPixel(new Rgba32(r, g, b, a));
        }

        protected virtual Leet CreateLeet(Leets leet, IEnumerable<Patch> patches)
        {
            if (patches == null)
            {
                return null;
            }

            var configuration = Shape.GetConfiguration(leet);
            var shape = _shapeSerializer.Deserialize(configuration);
            if (shape.Points.Count != patches.Count())
            {
                return null;
            }

            var geometries = new List<Geometry>();
            for (int i = 0; i < patches.Count(); i++)
            {
                geometries.Add(new Geometry { Identifier = (uint)i + 1, ColorHex = GetPatchColorHex(patches.ElementAt(i)) });
            }

            return Leet.Create(leet, geometries);
        }

        protected virtual byte CreateRandomByte(int minValue = 0, int maxValue = 255)
        {
            lock (syncLock)
            {
                return (byte)random.Next(minValue, maxValue);
            }
        }

        protected virtual Leets CreateRandomLeet()
        {
            const int multiple = 25;
            var r = CreateRandomByte(1, Enum.GetNames<Leets>().Length * multiple);

            // Adding a multiple seemed to result in a better distribution
            var leet = r >= 1 && r <= multiple ? Leets.M : r >= multiple + 1 && r <= multiple * 2 ? Leets.G : Leets.F;

            return leet;
        }

        protected virtual Patch CreatePatch(Color color, List<PointF> points)
        {
            var brush = Brushes.Solid(color);

            var pen = Pens.Solid(color, (float).00001);

            var segments = new List<ILineSegment>
            {
                new LinearLineSegment([.. points])
            };

            var polygon = new Polygon(segments);

            return Patch.Create(pen, brush, polygon);
        }

        protected virtual List<Patch> DeserializePatches(Leets leet, IEnumerable<Geometry> geometries = null)
        {
            var configuration = Shape.GetConfiguration(leet);

            var shape = _shapeSerializer.Deserialize(configuration);

            // Supplied geometries (e.g., from a user-supplied leet code) must provide exactly one color per patch
            var geometryList = geometries?.ToList();
            if (geometryList != null && geometryList.Count != shape.Points.Count)
            {
                return null;
            }

            var patches = new List<Patch>();

            for (int i = 0; i < shape.Points.Count; i++)
            {
                var points = shape.Points.ElementAt(i);

                var p = new List<PointF>();
                foreach (var point in points)
                {
                    p.Add(new PointF(point.X, point.Y));
                }

                var colorHex = geometryList?[i]?.ColorHex;

                Color color;
                if (string.IsNullOrWhiteSpace(colorHex))
                {
                    color = CreateColor();
                }
                else if (!Color.TryParseHex(colorHex, out color, ColorHexFormat.Rgba))
                {
                    return null;
                }

                patches.Add(CreatePatch(color, p));
            }

            return patches;
        }

        protected virtual string GetPatchColorHex(Patch patch)
        {
            if (patch?.Brush == null)
            {
                return null;
            }

            return ((SolidBrush)patch.Brush).Color.ToHex(ColorHexFormat.Rgba);
        }

        protected class Patch
        {
            public Pen Pen { get; set; }

            public Brush Brush { get; set; }

            public Polygon Polygon { get; set; }

            public static Patch Create(Pen pen, Brush brush, Polygon polygon)
            {
                return new Patch
                {
                    Pen = pen,
                    Brush = brush,
                    Polygon = polygon
                };
            }
        }
    }
}
