using System.Collections.Generic;

using Portfolio.Business.Models;

namespace Portfolio.API.Representations
{
    public class WritingRepresentation
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Anchor { get; set; }

        public string FileName { get; set; }

        public IEnumerable<WorkRepresentation> Works { get; set; }

        public static IEnumerable<WritingRepresentation> Create(IEnumerable<Writing> writingSummaries)
        {
            if (writingSummaries == null)
            {
                return null;
            }

            var result = new List<WritingRepresentation>();

            foreach (var writingSummary in writingSummaries)
            {
                var works = new List<WorkRepresentation>();
                foreach (var work in writingSummary.Works)
                {
                    works.Add(WorkRepresentation.Create(work));
                }

                result.Add(new WritingRepresentation
                {
                    ID = writingSummary.ID,
                    Name = writingSummary.Name,
                    Anchor = writingSummary.Anchor,
                    FileName = writingSummary.FileName,
                    Works = works
                });
            }

            return result;
        }
    }
}
