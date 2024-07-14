using AutoMapper;
using BookCollectorWebAPI.Models;

namespace BookCollector
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<BookDTO, Book>()
                .ForMember(b => b.Title, opts =>
                {
                    opts.MapFrom(b => b.Title!.Trim());
                })
                .ForMember(b => b.Author,opts =>
                {
                    opts.MapFrom(b => b.Author!.Trim());
                })
                .ForMember(b => b.Description, opts =>
                {
                    opts.MapFrom(b => b.Description != null ? b.Description.Trim() : null);
                })
                .ForMember(b => b.AmazonRating, opts =>
                {
                    opts.MapFrom(b => int.Parse(b.AmazonRating!));
                })
                .ForMember(b => b.BookOutletRating, opts =>
                {
                    opts.MapFrom(b => int.Parse(b.BookOutletRating!));
                })
                .ReverseMap();

        }
    }
}
