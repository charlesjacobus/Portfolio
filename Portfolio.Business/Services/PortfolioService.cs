using System;
using System.Linq;

using Portfolio.Business.Models;

namespace Portfolio.Business.Services
{
    public interface IPortfolioService
    {
        Photo GetPhoto(int id);
    }

    public class PortfolioService
        : IPortfolioService
    {
        private static readonly Random _random = new Random(); // Note too that IPortfolioService is registered as a singleton

        private readonly IExhibitService _exhibitService;

        public PortfolioService(IExhibitService exhibitService)
        {
            _exhibitService = exhibitService;
        }

        public Photo GetPhoto(int id)
        {
            var first = About.Default().Photos.First();

            var portfolio = _exhibitService.Portfolio;
            if (portfolio?.About?.Photos != null && portfolio.About.Photos.Any())
            {
                var photo = portfolio.About.Photos.FirstOrDefault(p => p.ID == id);
                if (photo != null)
                {
                    return photo;
                }

                var index = _random.Next(0, portfolio.About.Photos.Count());
                photo = portfolio.About.Photos.ElementAt(index);
                if (photo != null)
                {
                    return photo;
                }
            }

            return first;
        }
    }
}
