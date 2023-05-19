using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Configuration;

using Portfolio.Business.Models;

namespace Portfolio.Business.Services
{
    public interface IExhibitService
    {
        Models.Portfolio Portfolio { get; }

        Exhibit GetExhibit(int id);

        IEnumerable<ExhibitSummary> GetExhibitsSummary();
    }

    public class ExhibitService
        : PortfolioItemService, IExhibitService
    {
        public ExhibitService(IConfiguration configuration)
            : base(configuration) { }

        public Exhibit GetExhibit(int id)
        {
            if (Portfolio?.Exhibits == null)
            {
                return Models.Portfolio.Error().Exhibits.First();
            }

            return Portfolio.Exhibits.FirstOrDefault(e => e.ID == id);
        }

        public IEnumerable<ExhibitSummary> GetExhibitsSummary()
        {
            if (Portfolio?.Exhibits == null)
            {
                return Models.Portfolio.Error().Exhibits;
            }

            return Portfolio.Exhibits
                .Where(e => string.Equals(e.Status, "Active", System.StringComparison.OrdinalIgnoreCase))
                .Select(e => ExhibitSummary.Create(e.ID, e.Name, e.Description, e.DescriptionFileName, e.Order, e.Anchor, e.Promo ?? e.Works.First(), e.TextIsDefault, e.TextLabel, e.TextRoute))
                .OrderBy(e => e.Order).ThenBy(e => e.Name);
        }
    }
}
