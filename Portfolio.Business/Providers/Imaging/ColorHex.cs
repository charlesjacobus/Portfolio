using System.Drawing;
using System.Globalization;
using System.Linq;

namespace Portfolio.Business.Providers.Imaging
{
    /// <summary>
    /// Converts colors to and from eight-digit hex strings in RRGGBBAA order (the format stored in leet codes)
    /// </summary>
    public static class ColorHex
    {
        private const int Length = 8;

        public static bool TryParse(string value, out Color color)
        {
            color = Color.Empty;

            if (value == null || value.Length != Length || !value.All(char.IsAsciiHexDigit))
            {
                return false;
            }

            var rgba = uint.Parse(value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);

            color = Color.FromArgb((byte)rgba, (byte)(rgba >> 24), (byte)(rgba >> 16), (byte)(rgba >> 8));

            return true;
        }

        public static string ToHex(Color color)
        {
            return $"{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
        }
    }
}
