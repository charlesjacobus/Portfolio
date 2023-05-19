using System.Linq;

using Microsoft.Extensions.Configuration;

namespace Portfolio.Business.Services
{
    public interface IPortfolioItemService
    {
        Models.Portfolio Portfolio { get; }
    }

    public abstract class PortfolioItemService
        : IPortfolioItemService
    {
        private static Models.Portfolio _portfolio;

        protected readonly IConfiguration _configuration;

        public PortfolioItemService(IConfiguration configuration)
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
    }
}
