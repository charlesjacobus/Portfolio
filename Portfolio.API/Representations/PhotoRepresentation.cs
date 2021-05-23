using Portfolio.Business.Models;

namespace Portfolio.Api.Representations
{
    public class PhotoRepresentation
    {
        public string Source { get; set; }

        public string CaptionHtml { get; set; }

        public string Title { get; set; }

        public string Orientation { get; set; }

        public static PhotoRepresentation Create(Photo photo)
        {
            return new PhotoRepresentation
            {
                CaptionHtml = photo.CaptionHtml,
                Source = photo.FileName,
                Title = photo.Title,
                Orientation = photo.Orientation
            };
        }
    }
}
