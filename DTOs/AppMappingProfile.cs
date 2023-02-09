using AutoMapper;
using UserListsAPI.Data.Entities;
using UserListsAPI.JsonModels;

namespace UserListsAPI.DTOs;

public class AppMappingProfile : Profile
{
  public AppMappingProfile()
  {
    ValueTransformers.Add<string>(s => string.IsNullOrEmpty(s) ? String.Empty : s);

    CreateMap<Game, GameDTO>();
    CreateMap<Movie, MovieDTO>();
    CreateMap<GameJson, Game>()
      .ForMember(game => game.MetacriticScore, opt => opt.MapFrom(gameJson => (gameJson.Metacritic == null) ? 0 : gameJson.Metacritic.Score))
      .ForMember(game => game.MetacriticUrl, opt => opt.MapFrom(gameJson => (gameJson.Metacritic == null) ? String.Empty : gameJson.Metacritic.Url))
      .ForMember(game => game.ComingSoon, opt => opt.MapFrom(gameJson => (gameJson.ReleaseDate == null) ? false : gameJson.ReleaseDate.ComingSoon))
      .ForMember(game => game.ReleaseDate, opt => opt.MapFrom(gameJson => (gameJson.ReleaseDate == null) ? String.Empty : gameJson.ReleaseDate.Date))
      .ForMember(game => game.Genres, opt => opt.MapFrom(gameJson => (gameJson.Genres != default) ? gameJson.Genres.Select(genre => genre.Description) : Enumerable.Empty<string>()));
    CreateMap<GameReviewsJson, Game>();
    CreateMap<MovieJson, Movie>();
  }
}