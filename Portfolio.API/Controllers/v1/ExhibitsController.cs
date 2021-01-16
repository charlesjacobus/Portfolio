using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;

using AutoMapper;

using Portfolio.API.Representations;
using Portfolio.Business.Services;

namespace Portfolio.API.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ExhibitsController
        : ControllerBase
    {
        private readonly IExhibitService _exhibitService;
        private readonly IMapper _mapper;

        public ExhibitsController(IExhibitService exhibitService, IMapper mapper)
        {
            _exhibitService = exhibitService;
            _mapper = mapper;
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

            var mapped = _mapper.Map<IEnumerable<ExhibitSummaryRepresentation>>(summaries);

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

            var mapped = _mapper.Map<ExhibitRepresentation>(exhibit);

            return mapped;
        }
    }
}
