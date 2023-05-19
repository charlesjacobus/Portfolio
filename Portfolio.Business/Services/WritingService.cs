using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Configuration;

using Portfolio.Business.Models;

namespace Portfolio.Business.Services
{
    public interface IWritingService
    {
        Writing GetWriting(int id);

        IEnumerable<Writing> GetWritings();
    }

    public class WritingService
        : PortfolioItemService, IWritingService
    {
        public WritingService(IConfiguration configuration)
            : base(configuration) { }

        public Writing GetWriting(int id)
        {
            if (Portfolio?.Writings == null)
            {
                return Models.Portfolio.Error().Writings.First();
            }

            return Portfolio.Writings.FirstOrDefault(e => e.ID == id);
        }

        public IEnumerable<Writing> GetWritings()
        {
            if (Portfolio?.Writings == null)
            {
                return Models.Portfolio.Error().Writings;
            }

            return Portfolio.Writings
                .Where(w => string.Equals(w.Status, "Active", System.StringComparison.OrdinalIgnoreCase))
                .Select(w => Writing.Create(w.ID, w.Name, w.Order, w.Anchor, w.FileName, w.Works))
                .OrderBy(w => w.Order).ThenBy(e => e.Name);
        }
    }
}
