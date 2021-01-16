using Portfolio.Business.Models.Health;

namespace Portfolio.Business.Services.Health
{
    public class NoOpHealthService
        : IHealthService
    {
        public static NoOpHealthService GetInstance()
        {
            return new NoOpHealthService();
        }

        public HealthCheckResult Verify()
        {
            return HealthCheckResult.GetHealthyResult();
        }
    }
}
