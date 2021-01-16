using AutoMapper;

using Portfolio.API.Representations;
using Portfolio.Business.Models;

namespace Portfolio.API.Mappers
{
    public class ExhibitProfile
        : Profile
    {
        public ExhibitProfile()
        {
            CreateMap<Exhibit, ExhibitRepresentation>();
            CreateMap<ExhibitSummary, ExhibitSummaryRepresentation>();
        }
    }
}
