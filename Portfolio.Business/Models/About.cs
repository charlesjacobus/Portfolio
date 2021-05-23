using System.Collections.Generic;
using System.Linq;

namespace Portfolio.Business.Models
{
    public class About
    {
        public IEnumerable<Photo> Photos { get; set; }

        public static About Default()
        {
            return new About
            {
                Photos = new List<Photo>
                {
                    new Photo
                    {
                        ID = 1,
                        FileName = "GutierezPortrait.jpg",
                        CaptionHtml = "Copyright © Albert Gutierez"
                    }
                }
            };
        }
    }
}
