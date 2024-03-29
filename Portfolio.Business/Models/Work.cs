﻿namespace Portfolio.Business.Models
{
    public class Work
    {
        public string FileName { get; set; }

        public string FileNameLarge { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public static Work Error()
        {
            return new Work { FileName = "Error.md", Name = null, Description = null };
        }
    }
}
