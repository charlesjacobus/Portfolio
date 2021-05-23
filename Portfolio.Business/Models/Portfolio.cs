using System.Collections.Generic;

namespace Portfolio.Business.Models
{
    public class Portfolio
    {
        public About About { get; set; }

        public IEnumerable<Exhibit> Exhibits { get; set; }

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
                }
            };
        }
    }
}
