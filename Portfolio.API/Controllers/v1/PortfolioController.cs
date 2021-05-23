using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Portfolio.Api.Builders;
using Portfolio.Api.Representations;
using Portfolio.Business.Services;

namespace Portfolio.Api.Controllers
{
    /// <summary>
    /// Supports operations related to the portfolio
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PortfolioController
        : ControllerBase
    {
        private PortfolioRepresentationBuilder _builder;
        private IConfiguration _configuration;
        private IPortfolioService _portfolioService; 

        public PortfolioController(IConfiguration configuration, IPortfolioService portfolioService, PortfolioRepresentationBuilder builder)
        {
            _builder = builder;
            _configuration = configuration;
            _portfolioService = portfolioService;
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
        /// Gets information about a photo of the artist
        /// </summary>
        /// <param name="id">An optional photo identifier</param>
        /// <returns>Photo information</returns>
        /// <remarks>If the identifier is out of range, then information for a random photo is returned</remarks>
        [HttpGet]
        [Route("Photos/{id}", Name = "GetPhoto")]
        public PhotoRepresentation GetPhoto(int id)
        {
            var photo = _portfolioService.GetPhoto(id);

            var context = HttpContext.Request.Path.Value;

            var representation = PhotoRepresentation.Create(photo);

            return representation;
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
