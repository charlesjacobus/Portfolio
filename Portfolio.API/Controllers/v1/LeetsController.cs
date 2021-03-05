using System;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

using Portfolio.Business.Models;
using Portfolio.Business.Services;
using Portfolio.Business.Serializers;
using Portfolio.API.Representations;

namespace Portfolio.Api.Controllers
{
    /// <summary>
    /// Supports operations related to leets
    /// </summary>
    [ApiController]
    [Route("api/v1/Portfolio/[controller]"), FormatFilter]
    public class LeetsController
        : ControllerBase
    {
        private readonly ILeetService _leetService;
        private readonly ISerializer<Leet> _leetSerializer;

        public LeetsController(ILeetService leetService, ISerializer<Leet> leetSerializer)
        {
            _leetService = leetService;
            _leetSerializer = leetSerializer;
        }

        /// <summary>
        /// Creates a new leet
        /// </summary>
        /// <param name = "code">The leet code</param>
        /// <returns>A leet</returns>
        /// <remarks>
        /// <para>If <paramref name="code"/> is supplied, then the leet is created using the supplied code.</para>
        /// <para>Otherwise, a new leet is created.</para>
        /// <para>In either case, the leet code is returned as an ETag in the response.</para>
        /// </remarks>
        [HttpGet]
        [Route("", Name = "GetLeet")]
        public LeetRepresentation GetLeet(string code)
        {
            LeetStream stream;

            if (string.IsNullOrWhiteSpace(code))
            {
                stream = _leetService.CreateLeetStream();

                code = stream.Code;
            }
            else
            { 
                var leet = _leetSerializer.Deserialize(code);

                stream = _leetService.CreateLeetStream(leet);
            }

            var file = CreateActionResult(stream);
            if (file == null)
            {
                return null;
            }

            var result = LeetRepresentation.Create(code, file);

            return result;
        }

        /// <summary>
        /// Creates a new leet file
        /// </summary>
        /// <param name = "code">The leet code</param>
        /// <returns>A leet file</returns>
        /// <remarks>
        /// <para>This endpoint is an alias for the primary GET endpoint, although this endpoint returns a leet file.</para>
        /// <para>The leet code is returned as an ETag value.</para>
        /// </remarks>
        [HttpGet]
        [Route("Files", Name = "GetLeetFile")]
        public IActionResult GetLeetFile(string code)
        {
            LeetStream stream;

            if (string.IsNullOrWhiteSpace(code))
            {
                stream = _leetService.CreateLeetStream();
            }
            else
            {
                var leet = _leetSerializer.Deserialize(code);

                stream = _leetService.CreateLeetStream(leet);
            }

            var file = CreateActionResult(stream);
            if (file == null)
            {
                return null;
            }

            return file;
        }

        private FileContentResult CreateActionResult(LeetStream leetStream)
        {
            if (string.IsNullOrWhiteSpace(leetStream?.Code) || leetStream?.Stream == null)
            {
                return null;
            }

            var entityTagHeaderValue = new EntityTagHeaderValue(string.Format("\"{0}\"", leetStream.Code));

            return File(leetStream.Stream.ToArray(), Leet.FileMimeType, Leet.FileName, DateTime.UtcNow, entityTagHeaderValue);
        }
    }
}
