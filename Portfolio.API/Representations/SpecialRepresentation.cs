using Portfolio.Business.Models;

namespace Portfolio.API.Representations
{
    public class SpecialRepresentation
    {
        public string ComponentName { get; set; }

        public string TabName { get; set; }

        public static SpecialRepresentation Create(Special special)
        {
            return new SpecialRepresentation
            {
                ComponentName = special?.ComponentName,
                TabName = special?.TabName
            };
        }
    }
}
