using Portfolio.Business.Models;

namespace Portfolio.API.Representations
{
    public class WorkRepresentation
    {
        public string FileName { get; set; }

        public string FileNameLarge { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public static WorkRepresentation Create(Work work)
        {
            if (work == null)
            {
                return null;
            }

            return new WorkRepresentation
            {
                FileName = work.FileName,
                FileNameLarge = work.FileNameLarge,
                Name = work.Name,
                Description = work.Description
            };
        }
    }
}
