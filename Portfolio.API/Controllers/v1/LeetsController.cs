using System;

using Microsoft.AspNetCore.Http;
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
    [Route("api/v1/Portfolio/Exhibits/[controller]"), FormatFilter]
    public class LeetsController
        : ControllerBase
    {
        // An identifier character plus one 8-character hex color per patch, with headroom
        private const int MaxCodeLength = 256;

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<LeetRepresentation> GetLeet(string code)
        {
            var stream = CreateLeetStream(code);
            if (stream == null)
            {
                return BadRequest();
            }

            var file = CreateActionResult(stream);
            if (file == null)
            {
                return BadRequest();
            }

            var result = LeetRepresentation.Create(stream.Code, file);

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetLeetFile(string code)
        {
            var file = CreateActionResult(CreateLeetStream(code));
            if (file == null)
            {
                return BadRequest();
            }

            return file;
        }

        private LeetStream CreateLeetStream(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return _leetService.CreateLeetStream();
            }

            // Reject malformed codes up front (IsValid round-trips the code, so partial or non-canonical codes fail)
            if (code.Length > MaxCodeLength || !_leetSerializer.IsValid(code))
            {
                return null;
            }

            var leet = _leetSerializer.Deserialize(code);

            return _leetService.CreateLeetStream(leet);
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
