using System.Collections.Generic;
using System.Linq;

namespace Portfolio.Business.Models.Health
{
    public enum HealthCheckStatus
    {
        Unverified = 0,
        Healthy = 1,
        Unhealthy = 2
    }

    public class HealthCheckResult
    {
        public HealthCheckResult()
        {
            Status = HealthCheckStatus.Unverified;
        }

        public HealthCheckStatus Status { get; private set; }

        public string Message { get; private set; }

        public static HealthCheckResult GetHealthyResult()
        {
            return new HealthCheckResult
            {
                Status = HealthCheckStatus.Healthy
            };
        }

        public static HealthCheckResult GetUnhealthyResult(string message)
        {
            return new HealthCheckResult
            {
                Status = HealthCheckStatus.Unhealthy,
                Message = message
            };
        }
    }

    public class HealthCheckResults
        : List<HealthCheckResult>
    {
        public override string ToString()
        {
            if (!this.Any() || this.All(s => s.Status == HealthCheckStatus.Healthy))
            {
                return "OK";
            }

            var result = string
                .Join(",", this
                .Where(s => s.Status != HealthCheckStatus.Healthy)
                .Select(s => s.Message));

            return result;
        }
    }
}
