using System.Collections.Generic;
using System.IO;
using System.Linq;

using Portfolio.Business.Serializers;

namespace Portfolio.Business.Models
{
    public enum Leets
    {
        M = 1, // Meer
        G = 2, // G
        F = 3  // Fatigue
    }

    public class LeetIdentifier
    {
        public virtual Leets Identifier { get; set; }

        public virtual string Code { get; set; }
    }

    public class LeetStream
        : LeetIdentifier
    {
        public MemoryStream Stream { get; set; }

        public static LeetStream Create(Leets identifier, string code, MemoryStream stream)
        {
            return new LeetStream
            {
                Identifier = identifier,
                Code = code,
                Stream = stream
            };
        }
    }

    public class Leet
        : LeetIdentifier
    {
        public const string FileMimeType = "image/jpeg";
        public const string FileName = "Leet.jpg";

        public const int Height = 2100;
        public const int Width = 1500;

        public override string Code
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Code) && Geometries == null || !Geometries.Any())
                {
                    base.Code = new LeetSerializer().Serialize(this);
                }

                return base.Code;
            }
            set => base.Code = value;
        }

        public IEnumerable<Geometry> Geometries { get; set; }

        public string ShapesConfiguration { get; set; }

        public override string ToString()
        {
            if (Geometries == null || !Geometries.Any())
            {
                return base.ToString();
            }

            return new LeetSerializer().Serialize(this);
        }

        public static Leet Create(Leets leet, IEnumerable<Geometry> geometries)
        {
            var result = new Leet
            {
                Identifier = leet,
                Geometries = geometries,
                ShapesConfiguration = Shape.GetConfiguration(leet)
            };

            return result;
        }

        public static Leet Empty()
        {
            return new Leet();
        }
    }
}
