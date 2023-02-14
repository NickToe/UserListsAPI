using Application.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application;

public class AppMappingProfile : Profile
{
    public AppMappingProfile()
    {
        ValueTransformers.Add<string>(s => string.IsNullOrEmpty(s) ? string.Empty : s);

        CreateMap<Game, GameDTO>();
        CreateMap<Movie, MovieDTO>();
    }
}