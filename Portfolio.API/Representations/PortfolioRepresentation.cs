using Portfolio.Business.Attributes;

namespace Portfolio.Api.Representations
{
    public class PortfolioRepresentation
    {
        public ApplicationSettingsRepresentation Settings { get; set; }

        [RouteMap("GetActiveExhibits")]
        public string HrefGetActiveExhibits { get; set; }

        [RouteMap("GetExhibit")]
        public string HrefGetExhibit { get; set; }

        public static PortfolioRepresentation Create()
        {
            return new PortfolioRepresentation();
        }
    }
}
