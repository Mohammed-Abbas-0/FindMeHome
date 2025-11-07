using AutoMapper;
using FindMeHome.Dtos;
using FindMeHome.Models;
using FindMeHome.Repositories.AbstractionLayer;
using FindMeHome.Services.Abstraction;

namespace FindMeHome.Services.Implementation
{
    public class RealStateService : IRealStateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public RealStateService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #region Public Methods


        public async Task<ResultDto> CreateAsync(RealEstateDto dto)
        {
            var validation = ValidateRealStateDto(dto);
            if (!validation.IsSuccess)
                return validation;

            var entity = _mapper.Map<RealEstate>(dto);

            // 🖼️ حفظ صور العقار
            if (dto.Images != null && dto.Images.Count > 0)
            {
                entity.Images = new List<RealEstateImage>();

                foreach (var file in dto.Images)
                {
                    if (file.Length > 0)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var folderPath = Path.Combine("wwwroot", "uploads", "properties");
                        var fullPath = Path.Combine(folderPath, fileName);

                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        entity.Images.Add(new RealEstateImage
                        {
                            ImageUrl = $"/uploads/properties/{fileName}"
                        });
                    }
                }
            }

            // 🪑 حفظ الأثاث
            if (dto.Furnitures != null && dto.Furnitures.Count > 0)
            {
                entity.Furnitures = new List<Furniture>();

                foreach (var furniture in dto.Furnitures)
                {
                    var f = new Furniture
                    {
                        Name = furniture.Name,
                        Price = furniture.Price??0
                    };

                    if (furniture.Image != null)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(furniture.Image.FileName)}";
                        var folderPath = Path.Combine("wwwroot", "uploads", "furnitures");
                        var fullPath = Path.Combine(folderPath, fileName);

                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await furniture.Image.CopyToAsync(stream);
                        }

                        f.ImagePath = $"/uploads/furnitures/{fileName}";
                    }

                    entity.Furnitures.Add(f);
                }
            }

            await _unitOfWork.RealEstates.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return  ResultDto.Success( "✅ تم الحفظ بنجاح");
        }


        #endregion

        #region Private Methods

        private ResultDto ValidateRealStateDto(RealEstateDto dto)
        {
            if (dto == null)
                return ResultDto.Failure("بيانات العقار غير موجودة.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                return ResultDto.Failure("عنوان العقار مطلوب.");

            if (dto.Price == null || dto.Price <= 0)
                return ResultDto.Failure("يجب إدخال سعر صحيح للعقار.");

            if (string.IsNullOrWhiteSpace(dto.City))
                return ResultDto.Failure("المدينة مطلوبة.");

            if (string.IsNullOrWhiteSpace(dto.Address))
                return ResultDto.Failure("العنوان مطلوب.");

            if (dto.Area == null || dto.Area <= 0)
                return ResultDto.Failure("يجب إدخال مساحة صحيحة.");

            if (string.IsNullOrWhiteSpace(dto.IsForSale))
                return ResultDto.Failure("يجب تحديد نوع العرض (بيع أو إيجار).");

            if (dto.Latitude == 0 || dto.Longitude == 0)
                return ResultDto.Failure("يجب تحديد موقع العقار على الخريطة.");

            // ممكن تضيف تحقق إضافي للصور مثلاً
            if (dto.Images == null || dto.Images.Count == 0)
                return ResultDto.Failure("يجب رفع صورة واحدة على الأقل للعقار.");

            return ResultDto.Success("تم التحقق بنجاح ✅");
        }


        #endregion
    }
}
