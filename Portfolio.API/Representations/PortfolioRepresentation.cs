using Portfolio.Business.Attributes;

namespace Portfolio.Api.Representations
{
    public class PortfolioRepresentation
    {
        public ApplicationSettingsRepresentation Settings { get; set; }

        [RouteMap("GetActiveExhibits")]
        public string HrefGetActiveExhibits { get; set; }

        [RouteMap("GetActiveWritings")]
        public string HrefGetActiveWritings { get; set; }

        [RouteMap("GetExhibit")]
        public string HrefGetExhibit { get; set; }

        [RouteMap("GetLeet")]
        public string HrefGetLeet { get; set; }

        [RouteMap("GetPhoto")]
        public string HrefGetPhoto { get; set; }

        public static PortfolioRepresentation Create()
        {
            return new PortfolioRepresentation();
        }
    }
}
