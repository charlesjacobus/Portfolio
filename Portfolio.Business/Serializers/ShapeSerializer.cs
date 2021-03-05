using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Portfolio.Business.Models;

namespace Portfolio.Business.Serializers
{
    public class ShapeSerializer
        : ISerializer<Shape>
    {
        private const char ValueDelimiterMarker = ',';
        private const char ValueBeginMarker = '[';
        private const char ValueEndMarker = ']';

        private uint depth = 0;

        public Shape Deserialize(string value)
        {
            try
            {
                List<Point> points = null;

                var shape = Shape.Create();

                using (var reader = new StringReader(value))
                {
                    bool delimited = false;
                    string x = string.Empty;
                    string y = string.Empty;

                    int n;
                    while ((n = reader.Read()) != -1)
                    {
                        char c = (char)n;

                        if (c == ValueBeginMarker)
                        {
                            depth++;
                            delimited = false;

                            if (depth == 2)
                            {
                                points = new List<Point>();
                            }
                        }
                        else if (c == ValueEndMarker)
                        {
                            depth--;
                            delimited = false;

                            if (depth == 2)
                            {
                                points.Add(new Point(float.Parse(x), float.Parse(y)));
                                x = string.Empty;
                                y = string.Empty;
                            }

                            if (depth == 1)
                            {
                                var p = new Points();
                                p.AddRange(points);
                                shape.Points.Add(p);
                            }
                        }
                        else if (c == ValueDelimiterMarker)
                        {
                            delimited = depth == 3;
                        }
                        else
                        {
                            if (depth == 3)
                            {
                                if (!delimited)
                                {
                                    x += c.ToString();
                                }
                                else
                                {
                                    y += c.ToString();
                                }
                            }
                        }
                    }
                }

                return shape;
            }
            catch
            {
                return null;
            }
        }

        public bool IsValid(string value)
        {
            try
            {
                var o = Deserialize(value);
                var s = Serialize(o);

                string left = Regex.Replace(value, @"\s", string.Empty);
                string right = Regex.Replace(s, @"\s", string.Empty);

                return string.Equals(left, right, System.StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public string Serialize(Shape shape)
        {
            var builder = new StringBuilder();
            
            builder.Append(ValueBeginMarker);

            for (int i = 0; i < shape.Points.Count(); i++)
            {
                var points = shape.Points[i];

                builder.Append(ValueBeginMarker);

                for (int j = 0; j < points.Count(); j++)
                {
                    builder.Append(ValueBeginMarker);
                    builder.Append(points[j].X);
                    builder.Append(ValueDelimiterMarker);
                    builder.Append(points[j].Y);
                    builder.Append(ValueEndMarker);

                    if (j < points.Count() - 1)
                    {
                        builder.Append(ValueDelimiterMarker);
                    }
                }

                builder.Append(ValueEndMarker);

                if (i < shape.Points.Count() - 1)
                {
                    builder.Append(ValueDelimiterMarker);
                }
            }

            builder.Append(ValueEndMarker);

            return builder.ToString();
        }
    }
}
