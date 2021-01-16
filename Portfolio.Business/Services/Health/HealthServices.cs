using System.Collections.Generic;

using Portfolio.Business.Models.Health;

namespace Portfolio.Business.Services.Health
{
    public interface IHealthServices
    {
        void Register(params IHealthService[] healthServices);

        IEnumerable<IHealthService> Services { get; }

        HealthCheckResults Verify();
    }

    public class HealthServices
        : IHealthServices
    {
        private List<IHealthService> _services;

        public HealthServices()
        {
            _services = new List<IHealthService>();
        }

        public void Register(params IHealthService[] healthServices)
        {
            if (healthServices == null)
            {
                return;
            }

            _services.AddRange(healthServices);
        }

        public IEnumerable<IHealthService> Services => _services;

        public HealthCheckResults Verify()
        {
            var results = new HealthCheckResults();

            _services.ForEach(s => results.Add(s.Verify()));

            return results;
        }
    }
}
