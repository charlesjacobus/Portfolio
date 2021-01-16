using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Portfolio.Business.Builders
{
    public interface IAugmentingBuilder<T>
        where T : class, new()
    {
        T Create(IConfiguration configuration);

        T CreateAndAugment(IConfiguration configuration, IUrlHelper urlHelper);
    }
}
