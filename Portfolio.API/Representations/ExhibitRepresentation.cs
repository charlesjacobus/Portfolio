using System.Collections.Generic;
using System.Linq;

using Portfolio.Business.Models;

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

        public string TextLabel { get; set; }

        public string TextRoute { get; set; }

        public static ExhibitSummaryRepresentation Create(int id, string name, string description, string descriptionFileName, string anchor, WorkRepresentation promo)
        {
            return new ExhibitSummaryRepresentation { ID = id, Name = name, Description = description, DescriptionFileName = descriptionFileName, Anchor = anchor, Promo = promo };
        }

        public static IEnumerable<ExhibitSummaryRepresentation> Create(IEnumerable<ExhibitSummary> exhibitSummaries)
        {
            if (exhibitSummaries == null)
            {
                return null;
            }

            var result = new List<ExhibitSummaryRepresentation>();

            foreach (var exhibitSummary in exhibitSummaries)
            {
                result.Add(Create(exhibitSummary.ID, exhibitSummary.Name, exhibitSummary.Description, exhibitSummary.DescriptionFileName, exhibitSummary.Anchor, WorkRepresentation.Create(exhibitSummary.Promo)));
            }

            return result;
        }
    }

    public class ExhibitRepresentation
        : ExhibitSummaryRepresentation
    {
        public IEnumerable<WorkRepresentation> Works { get; set; }

        public static ExhibitRepresentation Create(Exhibit exhibit)
        {
            if (exhibit == null)
            {
                return null;
            }

            var works = new List<WorkRepresentation>();
            foreach (var work in exhibit.Works)
            {
                works.Add(WorkRepresentation.Create(work));
            }

            return Create(exhibit.ID, exhibit.Name, exhibit.Description, exhibit.DescriptionFileName, exhibit.Anchor, works, exhibit.TextLabel, exhibit.TextRoute);
        }

        public static ExhibitRepresentation Create(int id, string name, string description, string descriptionFileName, string anchor, IEnumerable<WorkRepresentation> works, string textLabel, string textRoute)
        {
            return new ExhibitRepresentation {
                ID = id,
                Name = name,
                Description = description,
                DescriptionFileName = descriptionFileName,
                Anchor = anchor,
                Promo = works.FirstOrDefault(),
                Works = works,
                TextLabel = textLabel,
                TextRoute = textRoute
            };
        }
    }
}
