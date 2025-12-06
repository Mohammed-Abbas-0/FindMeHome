using AutoMapper;
using FindMeHome.Dtos;
using FindMeHome.Models;

namespace FindMeHome.Mappers
{
    public class MappingHelper : Profile
    {
        public MappingHelper()
        {
            // ✅ RealEstate ⇄ RealEstateDto
            // CreateMap<RealEstate, RealEstateDto>();
            //    .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
            //    .ForMember(dest => dest.Furnitures, opt => opt.MapFrom(src => src.Furnitures))
            //    //.ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.Reviews))
            //    //.ReverseMap()
            //    .ForMember(dest => dest.Id, opt => opt.Ignore()) // لو بتستخدمه في create
            //    .ForMember(dest => dest.Images, opt => opt.Ignore())
            //    .ForMember(dest => dest.Furnitures, opt => opt.Ignore());
            //    //.ForMember(dest => dest.Reviews, opt => opt.Ignore());

            //// ✅ RealEstateImage ⇄ RealEstateImageDto
            //CreateMap<RealEstateImage, RealEstateImageDto>().ReverseMap();

            //// ✅ Furniture ⇄ FurnitureDto
            //CreateMap<Furniture, FurnitureDto>().ReverseMap();

            // ✅ Review ⇄ ReviewDto (لو عندك الكلاس)
            //CreateMap<Review, ReviewDto>().ReverseMap();

        }
    }
}
