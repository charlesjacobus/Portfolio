using AutoMapper;

using Portfolio.API.Representations;
using Portfolio.Business.Models;

namespace Portfolio.API.Mappers
{
    public class WorkProfile
        : Profile
    {
        public WorkProfile()
        {
            CreateMap<Work, WorkRepresentation>();
        }
    }
}
