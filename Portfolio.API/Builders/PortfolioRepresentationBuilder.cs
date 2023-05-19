using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Portfolio.Api.Representations;
using Portfolio.Business.Builders;
using Portfolio.Business.Infrastructure;

namespace Portfolio.Api.Builders
{
    public class PortfolioRepresentationBuilder
        : IAugmentingBuilder<PortfolioRepresentation>
    {
        public PortfolioRepresentation Create(IConfiguration configuration)
        {
            var representation = new PortfolioRepresentation
            {
                Settings = ApplicationSettingsRepresentation.Configure(configuration)
            };

            return representation;
        }

        public PortfolioRepresentation CreateAndAugment(IConfiguration configuration, IUrlHelper urlHelper)
        {
            var instance = Create(configuration);

            var augmented = RouteMapAugmenter.AugmentRoutes(urlHelper, instance);
            augmented = RouteMapAugmenter.AugmentRoute(urlHelper, augmented, "GetExhibit", new { id = "{id}" });
            augmented = RouteMapAugmenter.AugmentRoute(urlHelper, augmented, "GetPhoto", new { id = "{id}" });
            augmented = RouteMapAugmenter.AugmentRoute(urlHelper, augmented, "GetWriting", new { id = "{id}" });

            return augmented;
        }
    }
}
