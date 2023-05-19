using System.Collections.Generic;

namespace Portfolio.Business.Models
{
    public class Writing
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Anchor { get; set; }

        public string FileName { get; set; }

        public string Status { get; set; }

        public int Order { get; set; }

        public IEnumerable<Work> Works { get; set; }

        public static Writing Create(int id, string name, int order, string anchor, string fileName, IEnumerable<Work> works, string status = "Active")
        {
            return new Writing { ID = id, Name = name, Order = order, Anchor = anchor, FileName = fileName, Status = status, Works = works };
        }

        public static Writing Error()
        {
            var works = new List<Work>
            {
                Work.Error()
            };

            return new Writing { ID = -1, Name = "Oops!", Anchor = "error", Works = works };
        }
    }
}
