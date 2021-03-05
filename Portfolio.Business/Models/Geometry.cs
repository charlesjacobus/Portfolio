namespace Portfolio.Business.Models
{
    public class Geometry
    {
        public uint Identifier { get; set; }

        public string ColorHex { get; set; }

        public static Geometry Create(uint identifier, string colorHex)
        {
            return new Geometry
            {
                Identifier = identifier,
                ColorHex = colorHex
            };
        }
    }
}
