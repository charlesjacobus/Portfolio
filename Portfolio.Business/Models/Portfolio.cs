using System.Collections.Generic;

namespace Portfolio.Business.Models
{
    public class Portfolio
    {
        public IEnumerable<Exhibit> Exhibits { get; set; }

        public static Portfolio Create()
        {
            return new Portfolio();
        }

        public static Portfolio Error()
        {
            return new Portfolio
            {
                Exhibits = new List<Exhibit>
                {
                    Exhibit.Error()
                }
            };
        }
    }
}
