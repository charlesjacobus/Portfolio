using Microsoft.AspNetCore.Mvc;

using Portfolio.Business.Services.Health; 

namespace Portfolio.Api.Controllers
{
    [Route("api/v1/Portfolio/[controller]"), FormatFilter]
    [ApiController]
    public class HealthController
        : ControllerBase
    {
        private IHealthServices _healthServices;

        public HealthController(IHealthServices healthServices)
        {
            _healthServices = healthServices;
        }

        /// <summary>
        /// Returns a result indicating the health status of basic API connectivity
        /// </summary>
        /// <returns>A result status</returns>
        [HttpGet]
        [Route("Basic", Name = "GetHealthBasic")]
        public string Health()
        {
            // Basic health checks are for the API itself and should always register only the no-op health service

            _healthServices
                .Register(NoOpHealthService.GetInstance());

            return Check(_healthServices);
        }

        /// <summary>
        /// Returns a result indicating the health status of all back-end resources that the API depends upon (e.g., database connections)
        /// </summary>
        /// <returns>A result status</returns>
        [HttpGet]
        [Route("Resources", Name = "GetHealthResources")]
        public string Resources()
        {
            // Resource checks are for all external resource dependencies (e.g., databases, external web services, queues, etc.)

            _healthServices
                .Register(NoOpHealthService.GetInstance());

            return Check(_healthServices);
        }

        private string Check(IHealthServices healthServices)
        {
            var result = healthServices
                .Verify()
                .ToString();

            return result;
        }
    }
}
