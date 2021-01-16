using System.Collections.Generic;
using System.Linq;

namespace Portfolio.API.Representations
{
    public class ExhibitSummaryRepresentation
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string DescriptionFileName { get; set; }

        public string Anchor { get; set; }

        public WorkRepresentation Promo { get; set; }

        public static ExhibitSummaryRepresentation Create(int id, string name, string description, string anchor, WorkRepresentation promo)
        {
            return new ExhibitSummaryRepresentation { ID = id, Name = name, Description = description, Anchor = anchor, Promo = promo };
        }
    }

    public class ExhibitRepresentation
        : ExhibitSummaryRepresentation
    {
        public IEnumerable<WorkRepresentation> Works { get; set; }

        public static ExhibitRepresentation Create(int id, string name, string description, string anchor, IEnumerable<WorkRepresentation> works)
        {
            return new ExhibitRepresentation { ID = id, Name = name, Description = description, Anchor = anchor, Promo = works.FirstOrDefault(), Works = works };
        }
    }
}
