using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

    public class LeetService
        : ILeetService
    {
        private readonly ISerializer<Shape> _shapeSerializer;

        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();

        public LeetService(ISerializer<Shape> shapeSerializer)
        {
            _shapeSerializer = shapeSerializer;
        }

        public LeetStream CreateLeetStream()
        {
            var image = new Image<Rgba32>(Leet.Width, Leet.Height);

            var identifier = CreateRandomLeet();

            var patches = DeserializePatches(identifier);

            var leet = CreateLeet(identifier, patches);

            patches.ForEach(p => image.Mutate(i => i
                .Draw(p.Pen, p.Polygon)
                .Fill(p.Brush, p.Polygon)));

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

            Image image = new Image<Rgba32>(Leet.Width, Leet.Height);

            var patches = DeserializePatches(leet.Identifier, leet.Geometries);

            patches.ForEach(p => image.Mutate(i => i
                .Draw(p.Pen, p.Polygon)
                .Fill(p.Brush, p.Polygon)));

            var stream = new MemoryStream();
            image.SaveAsJpeg(stream);

            return LeetStream.Create(leet.Identifier, leet.ToString(), stream);
        }

        protected virtual Color CreateColor()
        {
            var r = CreateRandomByte();
            var g = CreateRandomByte();
            var b = CreateRandomByte();
            var a = CreateRandomByte();

            return Color.FromRgba(r, g, b, a);
        }

        protected virtual Leet CreateLeet(Leets leet, IEnumerable<Patch> patches)
        {
            if (patches == null)
            {
                return null;
            }

            var configuration = Shape.GetConfiguration(leet);
            var shape = _shapeSerializer.Deserialize(configuration);
            if (shape.Points.Count() != patches.Count())
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
            var r = CreateRandomByte(1, Enum.GetNames(typeof(Leets)).Length * multiple);

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
                new LinearLineSegment(points.ToArray())
            };

            var polygon = new Polygon(segments);

            return Patch.Create(pen, brush, polygon);
        }

        protected virtual List<Patch> DeserializePatches(Leets leet, IEnumerable<Geometry> geometries = null)
        {
            var configuration = Shape.GetConfiguration(leet);

            var shape = _shapeSerializer.Deserialize(configuration);

            var patches = new List<Patch>();

            for (int i = 0; i < shape.Points.Count(); i++)
            {
                var points = shape.Points.ElementAt(i);

                var p = new List<PointF>();
                foreach (var point in points)
                {
                    p.Add(new PointF(point.X, point.Y));
                }

                var colorHex = geometries?.ElementAt(i)?.ColorHex;
                var color = !string.IsNullOrWhiteSpace(colorHex) ? Color.ParseHex(colorHex) : CreateColor();

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

            return ((SolidBrush)patch.Brush).Color.ToHex();
        }

        protected class Patch
        {
            public IPen Pen { get; set; }

            public IBrush Brush { get; set; }

            public Polygon Polygon { get; set; }

            public static Patch Create(IPen pen, IBrush brush, Polygon polygon)
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
