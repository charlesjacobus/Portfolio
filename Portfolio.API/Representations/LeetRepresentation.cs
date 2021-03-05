using Microsoft.AspNetCore.Mvc;

namespace Portfolio.API.Representations
{
    public class LeetRepresentation
    {
        public string Code { get; set; }

        public byte[] File { get; set; }

        public static LeetRepresentation Create(string code, FileContentResult file)
        {
            return new LeetRepresentation
            {
                Code = code,
                File = file?.FileContents
            };
        }
    }
}
