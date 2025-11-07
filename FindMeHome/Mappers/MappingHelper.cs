using AutoMapper;
using FindMeHome.Dtos;
using FindMeHome.Models;

namespace FindMeHome.Mappers
{
    public class MappingHelper : Profile
    {
        public MappingHelper()
        {
            CreateMap<RealEstateDto, RealEstate>()
              .ForMember(dest => dest.Id, opt => opt.Ignore())
              .ForMember(dest => dest.Images, opt => opt.Ignore())
              .ForMember(dest => dest.Furnitures, opt => opt.Ignore());

            CreateMap<RealEstateImageDto, RealEstateImage>();
            CreateMap<FurnitureDto, Furniture>();

        }
    }
}
