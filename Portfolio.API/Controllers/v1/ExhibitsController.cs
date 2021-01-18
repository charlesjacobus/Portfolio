using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;

using Portfolio.API.Representations;
using Portfolio.Business.Services;

namespace Portfolio.API.Controllers.v1
{
    /// <summary>
    /// Supports operations related to portfolio exhibits
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ExhibitsController
        : ControllerBase
    {
        private readonly IExhibitService _exhibitService;

        public ExhibitsController(IExhibitService exhibitService)
        {
            _exhibitService = exhibitService;
        }

        /// <summary>
        /// Gets a list of active exhibits 
        /// </summary>
        /// <returns>A list of exhibit summaries</returns>
        [HttpGet]
        [Route("", Name = "GetActiveExhibits")]
        public IEnumerable<ExhibitSummaryRepresentation> Get()
        {
            var summaries = _exhibitService.GetExhibitsSummary();

            var mapped = ExhibitSummaryRepresentation.Create(summaries);

            return mapped;
        }

        /// <summary>
        /// Gets details for a specified exhibit
        /// </summary>
        /// <returns>An exhibit</returns>
        [HttpGet]
        [Route("{id}", Name = "GetExhibit")]
        public ExhibitRepresentation Get(int id)
        {
            var exhibit = _exhibitService.GetExhibit(id);

            var mapped = ExhibitRepresentation.Create(exhibit);

            return mapped;
        }
    }
}
