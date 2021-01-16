using Portfolio.Business.Models.Health;

namespace Portfolio.Business.Services.Health
{
    public interface IHealthService
    {
        HealthCheckResult Verify();
    }
}
