using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Configuration;

using Portfolio.Business.Models;

namespace Portfolio.Business.Services
{
    public interface IExhibitService
    {
        Exhibit GetExhibit(int id);

        IEnumerable<ExhibitSummary> GetExhibitsSummary();
    }

    public class ExhibitService
        : IExhibitService
    {
        private static Models.Portfolio _portfolio;

        private IConfiguration _configuration;

        public ExhibitService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Models.Portfolio Portfolio
        {
            get
            {
                if (_portfolio == null)
                {
                    _portfolio = Models.Portfolio.Create();
                    _configuration.GetSection(nameof(Models.Portfolio)).Bind(_portfolio);

                    if (!_portfolio.Exhibits.Any())
                    {
                        _portfolio = Models.Portfolio.Error();
                    }
                }

                return _portfolio;
            }
        }

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
                .Where(e => !string.Equals(e.Status, "Archived", System.StringComparison.OrdinalIgnoreCase))
                .Select(e => ExhibitSummary.Create(e.ID, e.Name, e.Description, e.DescriptionFileName, e.Anchor, e.Promo ?? e.Works.First(), e.Special));
        }
    }
}
