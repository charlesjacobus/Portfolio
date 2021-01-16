using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Portfolio.Api.Builders;
using Portfolio.Api.Representations;

namespace Portfolio.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PortfolioController
        : ControllerBase
    {
        private PortfolioRepresentationBuilder _builder;
        private IConfiguration _configuration;

        public PortfolioController(IConfiguration configuration, PortfolioRepresentationBuilder builder)
        {
            _builder = builder;
            _configuration = configuration;
        }

        /// <summary>
        /// Gets information about the API, including documentation sources and navigable routes (synonym: Info)
        /// </summary>
        /// <returns>An API summary</returns>
        [HttpGet]
        [Route("", Name = "GetPortfolio")]
        public PortfolioRepresentation Get()
        {
            return GetPortfolioRepresentation();
        }

        /// <summary>
        /// Gets information about the API, including documentation sources and navigable routes
        /// </summary>
        /// <returns>An API summary</returns>
        [HttpGet]
        [Route("Info", Name = "GetPortfolioInfo")]
        public PortfolioRepresentation GetInfo()
        {
            return GetPortfolioRepresentation();
        }

        private PortfolioRepresentation GetPortfolioRepresentation()
        {
            var representation = _builder.CreateAndAugment(_configuration, Url);

            return representation;
        }
    }
}
