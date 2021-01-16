using System.Linq;
using System.Net;

using Microsoft.AspNetCore.Mvc;

using Portfolio.Business.Attributes;

namespace Portfolio.Business.Infrastructure
{
    /// <summary>
    /// Provides support for dynamically augmenting class instances by setting properties decorated with RouteMap attributes. 
    /// </summary>
    public class RouteMapAugmenter
    {
        /// <summary>
        /// Augments <paramref name="instance"/> by setting all properties decorated with a <see cref="RouteMapAttribute"/> for named routes that can be dynamically resolved by <paramref name="urlHelper"/>.
        /// </summary>
        /// <typeparam name="T">Any class with "href" properties decorated with a RouteMap attribute.</typeparam>
        /// <param name="urlHelper">A <see cref="IUrlHelper"/> instance, which is used to dynamically resolved named routes.</param>
        /// <param name="instance">The instance to augment.</param>
        /// <returns>An augmented {T} instance.</returns>
        /// <remarks>
        /// This method will only resolve parameterless routes.
        /// For parameterized routes, use the overload of this method that takes parameter values.
        /// </remarks>
        public static T AugmentRoutes<T>(IUrlHelper urlHelper, T instance)
            where T : class
        {
            if (urlHelper == null)
            {
                return instance;
            }

            var instanceType = instance.GetType();

            var propertyInfos = instanceType
                .GetProperties()
                .Where(prop => prop.IsDefined(typeof(RouteMapAttribute), false));

            foreach (var propertyInfo in propertyInfos)
            {
                var routeMapAttribute = (RouteMapAttribute)propertyInfo
                    .GetCustomAttributes(typeof(RouteMapAttribute), false)
                    .First();

                var routeValue = urlHelper.Link(routeMapAttribute.Name, new { });
                if (routeValue == null)
                {
                    continue;
                }

                instanceType.GetProperty(propertyInfo.Name).SetValue(instance, routeValue);
            }

            return instance;
        }

        /// <summary>
        /// Augments <paramref name="instance"/> by setting a property that's decorated with a <see cref="RouteMapAttribute"/> with a value of <paramref name="routeName"/> with the supplied parameters, if any.
        /// </summary>
        /// <typeparam name="T">Any class with "href" properties decorated with a RouteMap attribute.</typeparam>
        /// <param name="urlHelper">A <see cref="IUrlHelper"/> instance, which is used to dynamically resolved the named route.</param>
        /// <param name="instance">The instance to augment.</param>
        /// <param name="routeName">The name of the route to augment</param>
        /// <param name="parameters">The route parameters, if any, which can be a concrete instance or an anonymous type.</param>
        /// <returns>An augmented {T} instance.</returns>
        /// <remarks>
        /// This method will resolve parameterized or parameterless routes.
        /// </remarks>
        public static T AugmentRoute<T>(IUrlHelper urlHelper, T instance, string routeName, object parameters = null)
            where T : class
        {
            if (urlHelper == null)
            {
                return instance;
            }

            var instanceType = instance.GetType();

            var propertyInfo = instanceType
                .GetProperties()
                .Where(prop =>
                        prop.IsDefined(typeof(RouteMapAttribute), false)
                        &&
                        ((RouteMapAttribute)prop.GetCustomAttributes(typeof(RouteMapAttribute), false).First()).Name.Equals(routeName))
                .FirstOrDefault();
            if (propertyInfo == null)
            {
                return instance;
            }

            var routeMapAttribute = (RouteMapAttribute)propertyInfo
                    .GetCustomAttributes(typeof(RouteMapAttribute), false)
                    .First();

            // The IUrlHelper Link method expects an empty anonymous type when there are no parameters
            var routeParameters = parameters ?? new { };
            var routeValue = WebUtility.UrlDecode(urlHelper.Link(routeMapAttribute.Name, routeParameters));
            if (routeValue == null)
            {
                return instance;
            }

            instanceType.GetProperty(propertyInfo.Name).SetValue(instance, routeValue);

            return instance;
        }
    }
}
