﻿using System.Collections.Generic;
using System.Linq;

namespace Portfolio.Business.Models
{
    public class ExhibitSummary
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string DescriptionFileName { get; set; }

        public string Anchor { get; set; }

        public Work Promo { get; set; }

        public string Status { get; set; }

        public bool TextIsDefault { get; set; }

        public string TextLabel { get; set; }

        public string TextRoute { get; set; }

        public int Order { get; set; }

        public static Exhibit Create(int id, string name, string description, string descriptionFileName, int order, string anchor, Work promo, bool textIsDefault, string textLabel, string textRoute, string status = "Active")
        {
            return new Exhibit { ID = id, Name = name, Description = description, DescriptionFileName = descriptionFileName, Order = order, Anchor = anchor, Promo = promo, TextIsDefault = textIsDefault, TextLabel = textLabel, TextRoute = textRoute, Status = status };
        }
    }

    public class Exhibit
        : ExhibitSummary
    {
        public IEnumerable<Work> Works { get; set; }

        public static Exhibit Error()
        {
            var works = new List<Work>
            {
                Work.Error()
            };

            return new Exhibit { ID = -1, Name = "Oops!", Description = null, Anchor = "error", Promo = works.FirstOrDefault(), Works = works };
        }
    }
}
