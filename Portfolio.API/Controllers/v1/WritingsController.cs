using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;

using Portfolio.API.Representations;
using Portfolio.Business.Services;

namespace Portfolio.API.Controllers.v1
{
    /// <summary>
    /// Supports operations related to portfolio writings, including prose and poetry
    /// </summary>
    [ApiController]
    [Route("api/v1/Portfolio/[controller]")]
    public class WritingsController
        : ControllerBase
    {
        private readonly IWritingService _writingService;

        public WritingsController(IWritingService writingService)
        {
            _writingService = writingService;
        }

        /// <summary>
        /// Gets a list of active writings 
        /// </summary>
        /// <returns>A list of writing summaries</returns>
        [HttpGet]
        [Route("", Name = "GetActiveWritings")]
        public IEnumerable<WritingRepresentation> Get()
        {
            var summaries = _writingService.GetWritings();

            var mapped = WritingRepresentation.Create(summaries);

            return mapped;
        }
    }
}
