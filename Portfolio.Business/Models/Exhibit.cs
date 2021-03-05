using System.Collections.Generic;
using System.Linq;

namespace Portfolio.Business.Models
{
    public class ExhibitSummary
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string DescriptionFileName { get; set; }

        public string Anchor { get; set; }

        public Work Promo { get; set; }

        public string Status { get; set; }

        public Special Special { get; set; }

        public static Exhibit Create(int id, string name, string description, string descriptionFileName, string anchor, Work promo, Special special, string status = "Active")
        {
            return new Exhibit { ID = id, Name = name, Description = description, DescriptionFileName = descriptionFileName, Anchor = anchor, Promo = promo, Special = special, Status = status };
        }
    }

    public class Exhibit
        : ExhibitSummary
    {
        public IEnumerable<Work> Works { get; set; }

        public static Exhibit Error()
        {
            var works = new List<Work>
            {
                Work.Error()
            };

            return new Exhibit { ID = -1, Name = "Oops!", Description = null, Anchor = "error", Promo = works.FirstOrDefault(), Works = works };
        }
    }
}
