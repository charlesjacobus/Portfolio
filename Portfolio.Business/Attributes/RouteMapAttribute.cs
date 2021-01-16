using System;

namespace Portfolio.Business.Attributes
{
    public class RouteMapAttribute
        : Attribute
    {
        public RouteMapAttribute(string name, object values = null)
        {
            Name = name;
            Values = values;
        }

        public string Name { get; }

        public object Values { get; }
    }
}
