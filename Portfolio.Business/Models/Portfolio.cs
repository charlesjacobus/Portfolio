using System.Collections.Generic;
using System.Linq;

namespace Portfolio.Business.Models
{
    public class Portfolio
    {
        public About About { get; set; }

        public IEnumerable<Exhibit> Exhibits { get; set; }

        public IEnumerable<Writing> Writings { get; set; }

        public static Portfolio Create()
        {
            return new Portfolio();
        }

        public static Portfolio Error()
        {
            return new Portfolio
            {
                About = About.Default(),
                Exhibits = new List<Exhibit>
                {
                    Exhibit.Error()
                },
                Writings = new List<Writing>
                {
                    Writing.Error()
                }
            };
        }
    }
}
