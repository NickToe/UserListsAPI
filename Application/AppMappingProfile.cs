using AutoMapper;
using UserListsAPI.Application.DTOs;
using UserListsAPI.Domain.Entities;
using UserListsAPI.Infrastructure.Api.JsonModels;

namespace UserListsAPI.Application;

public class AppMappingProfile : Profile
{
    public AppMappingProfile()
    {
        ValueTransformers.Add<string>(s => string.IsNullOrEmpty(s) ? string.Empty : s);

        CreateMap<Game, GameDTO>();
        CreateMap<Movie, MovieDTO>();
        CreateMap<GameJson, Game>()
          .ForMember(game => game.MetacriticScore, opt => opt.MapFrom(gameJson => gameJson.Metacritic == null ? 0 : gameJson.Metacritic.Score))
          .ForMember(game => game.MetacriticUrl, opt => opt.MapFrom(gameJson => gameJson.Metacritic == null ? string.Empty : gameJson.Metacritic.Url))
          .ForMember(game => game.ComingSoon, opt => opt.MapFrom(gameJson => gameJson.ReleaseDate == null ? false : gameJson.ReleaseDate.ComingSoon))
          .ForMember(game => game.ReleaseDate, opt => opt.MapFrom(gameJson => gameJson.ReleaseDate == null ? string.Empty : gameJson.ReleaseDate.Date))
          .ForMember(game => game.Genres, opt => opt.MapFrom(gameJson => gameJson.Genres != default ? gameJson.Genres.Select(genre => genre.Description) : Enumerable.Empty<string>()));
        CreateMap<GameReviewsJson, Game>();
        CreateMap<MovieJson, Movie>();
    }
}