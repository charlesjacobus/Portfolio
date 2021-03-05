using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Portfolio.Business.Models;

namespace Portfolio.Business.Serializers
{
    public class LeetSerializer
        : ISerializer<Leet>
    {
        public Leet Deserialize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            try
            {
                var geometries = new List<Geometry>();

                var leet = Leet.Empty();

                using (var reader = new StringReader(value))
                {
                    string hex = string.Empty;
                    bool identified = false;

                    int n;
                    while ((n = reader.Read()) != -1)
                    {
                        char c = (char)n;

                        if (!identified)
                        {
                            if (!Enum.TryParse(c.ToString(), out Leets result))
                            {
                                return null;
                            }
                            leet.Identifier = result;

                            identified = true;
                        }
                        else
                        {
                            hex += c.ToString();

                            if (hex.Length == 8)
                            {
                                geometries.Add(Geometry.Create((uint)geometries.Count() + 1, hex));

                                hex = string.Empty;
                            }
                        }
                    }
                }

                leet.Geometries = geometries;

                return leet;
            }
            catch
            {
                return null;
            }
        }

        public bool IsValid(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var left = Deserialize(value);
            var right = Serialize(left);

            return string.Equals(value, right, StringComparison.OrdinalIgnoreCase);
        }

        public string Serialize(Leet value)
        {
            if (value == null)
            {
                return null;
            }

            var builder = new StringBuilder();

            builder.Append(value.Identifier);

            foreach (var geometry in value.Geometries)
            {
                builder.Append(geometry.ColorHex);
            }

            return builder.ToString();
        }
    }
}
