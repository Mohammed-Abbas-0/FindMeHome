using AutoMapper;
using FindMeHome.Dtos;
using FindMeHome.Enums;
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

        public async Task<ResultDto> CreateAsync(CreateRealEstateDto dto, string userId)
        {
            var validation = ValidateCreateRealEstateDto(dto);
            if (!validation.IsSuccess)
                return validation;

            var entity = new RealEstate
            {
                Title = dto.Title,
                Description = dto.Description,
                Address = dto.Address,
                City = dto.City,
                Neighborhood = dto.Neighborhood,
                Price = dto.Price,
                Area = dto.Area,
                ApartmentType = dto.ApartmentType,
                CanBeFurnished = dto.CanBeFurnished,
                Rooms = dto.Rooms,
                Bathrooms = dto.Bathrooms,
                UnitType = dto.UnitType,
                WhatsAppNumber = dto.WhatsAppNumber,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UserId = userId
            };

            await SaveImages(dto, entity);

            await _unitOfWork.RealEstates.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            // Save furniture after entity is saved (so we have the ID)
            await SaveFurnitures(dto, entity);
            await _unitOfWork.CompleteAsync();

            return ResultDto.Success("✅ تم الحفظ بنجاح");
        }

        public async Task<RealEstateDto?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.RealEstates
                .GetByIdAsync(id, includeProperties: "Images,Furnitures");

            if (entity == null)
                return null;

            return MapToDto(entity);
        }

        public async Task<List<RealEstateDto>> GetAllAsync()
        {
            var entities = await _unitOfWork.RealEstates
                .GetAllAsync(includeProperties: "Images,Furnitures");

            return entities.Select(MapToDto).ToList();
        }

        public async Task<List<RealEstateDto>> GetByUserIdAsync(string userId)
        {
            var entities = await _unitOfWork.RealEstates
                .FindAsync(e => e.UserId == userId, includeProperties: "Images,Furnitures");

            return entities.Select(MapToDto).ToList();
        }

        public async Task<List<RealEstateDto>> SearchAsync(string? query, decimal? minPrice, decimal? maxPrice, double? minArea, double? maxArea, int? rooms, int? bathrooms, string? city, string? neighborhood, UnitType? unitType, bool? isFurnished)
        {
            var entities = await _unitOfWork.RealEstates
                .GetAllAsync(includeProperties: "Images,Furnitures");

            var filtered = entities.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();
                filtered = filtered.Where(x => x.Title.ToLower().Contains(query) ||
                                             x.Description.ToLower().Contains(query) ||
                                             x.Address.ToLower().Contains(query));
            }

            if (minPrice.HasValue)
                filtered = filtered.Where(x => x.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                filtered = filtered.Where(x => x.Price <= maxPrice.Value);

            if (minArea.HasValue)
                filtered = filtered.Where(x => x.Area >= minArea.Value);

            if (maxArea.HasValue)
                filtered = filtered.Where(x => x.Area <= maxArea.Value);

            if (rooms.HasValue)
                filtered = filtered.Where(x => x.Rooms >= rooms.Value);

            if (bathrooms.HasValue)
                filtered = filtered.Where(x => x.Bathrooms >= bathrooms.Value);

            if (!string.IsNullOrWhiteSpace(city))
                filtered = filtered.Where(x => x.City.Contains(city));

            if (!string.IsNullOrWhiteSpace(neighborhood))
                filtered = filtered.Where(x => x.Neighborhood.Contains(neighborhood));

            if (unitType.HasValue)
                filtered = filtered.Where(x => x.UnitType == unitType.Value);

            if (isFurnished.HasValue)
                filtered = filtered.Where(x => x.CanBeFurnished == isFurnished.Value); // Assuming CanBeFurnished implies furnished option, or we might need a separate IsFurnished field if the model had it. The model has CanBeFurnished. Let's assume this filters for properties that CAN be furnished or are furnished. 
                                                                                       // Wait, the model has `CanBeFurnished`. The user might want to search for furnished apartments. 
                                                                                       // If the user selects "Furnished", they probably want `CanBeFurnished == true`.

            return filtered.Select(MapToDto).ToList();
        }

        public async Task<ResultDto> AddToWishlistAsync(int realEstateId, string userId)
        {
            var exists = await _unitOfWork.Wishlists
                .FindAsync(w => w.RealEstateId == realEstateId && w.UserId == userId);

            if (exists.Any())
                return ResultDto.Failure("العقار موجود بالفعل في قائمة المفضلة");

            var realEstate = await _unitOfWork.RealEstates.GetByIdAsync(realEstateId);
            if (realEstate == null)
                return ResultDto.Failure("العقار غير موجود");

            var wishlist = new Wishlist
            {
                RealEstateId = realEstateId,
                UserId = userId,
                AddedAt = DateTime.Now
            };

            await _unitOfWork.Wishlists.AddAsync(wishlist);
            await _unitOfWork.CompleteAsync();

            return ResultDto.Success("تمت الإضافة إلى قائمة المفضلة بنجاح");
        }

        public async Task<ResultDto> RemoveFromWishlistAsync(int realEstateId, string userId)
        {
            var wishlist = await _unitOfWork.Wishlists
                .FindAsync(w => w.RealEstateId == realEstateId && w.UserId == userId);

            var item = wishlist.FirstOrDefault();
            if (item == null)
                return ResultDto.Failure("العقار غير موجود في قائمة المفضلة");

            _unitOfWork.Wishlists.Remove(item);
            await _unitOfWork.CompleteAsync();

            return ResultDto.Success("تمت الإزالة من قائمة المفضلة بنجاح");
        }

        public async Task<bool> IsInWishlistAsync(int realEstateId, string userId)
        {
            var wishlist = await _unitOfWork.Wishlists
                .FindAsync(w => w.RealEstateId == realEstateId && w.UserId == userId);

            return wishlist.Any();
        }

        public async Task<List<RealEstateDto>> GetWishlistAsync(string userId)
        {
            var wishlists = await _unitOfWork.Wishlists
                .FindAsync(w => w.UserId == userId, includeProperties: "RealEstate.Images,RealEstate.Furnitures");

            return wishlists.Select(w => MapToDto(w.RealEstate)).ToList();
        }

        #endregion

        #region Private Methods

        private ResultDto ValidateCreateRealEstateDto(CreateRealEstateDto dto)
        {
            if (dto == null)
                return ResultDto.Failure("بيانات العقار غير موجودة.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                return ResultDto.Failure("عنوان العقار مطلوب.");

            if (dto.Price <= 0)
                return ResultDto.Failure("يجب إدخال سعر صحيح للعقار.");

            if (string.IsNullOrWhiteSpace(dto.City))
                return ResultDto.Failure("المدينة مطلوبة.");

            if (string.IsNullOrWhiteSpace(dto.Address))
                return ResultDto.Failure("العنوان مطلوب.");

            if (dto.Area <= 0)
                return ResultDto.Failure("يجب إدخال مساحة صحيحة.");

            if (string.IsNullOrWhiteSpace(dto.WhatsAppNumber))
                return ResultDto.Failure("رقم الواتساب مطلوب.");

            if (dto.Images == null || dto.Images.Count == 0)
                return ResultDto.Failure("يجب رفع صورة واحدة على الأقل للعقار.");

            return ResultDto.Success("تم التحقق بنجاح ✅");
        }

        private async Task SaveImages(CreateRealEstateDto dto, RealEstate entity)
        {
            // 🖼️ حفظ صور العقار
            if (dto.Images != null && dto.Images.Count > 0)
            {
                try
                {
                    entity.Images = new List<RealEstateImage>();

                    foreach (var file in dto.Images)
                    {
                        if (file != null && file.Length > 0)
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
                catch (Exception ex)
                {
                    throw new Exception("حدث خطأ أثناء حفظ صور العقار.", ex);
                }
            }
        }

        private async Task SaveFurnitures(CreateRealEstateDto dto, RealEstate entity)
        {
            // 🪑 حفظ الأثاث
            if (dto.Furnitures != null && dto.Furnitures.Count > 0 && dto.CanBeFurnished)
            {
                foreach (var furniture in dto.Furnitures)
                {
                    if (string.IsNullOrWhiteSpace(furniture.Name) || furniture.Price == null || furniture.Price <= 0)
                        continue;

                    var f = new Furniture
                    {
                        Name = furniture.Name,
                        Price = furniture.Price.Value,
                        RealEstateId = entity.Id
                    };

                    if (furniture.Image != null && furniture.Image.Length > 0)
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

                    await _unitOfWork.Furnitures.AddAsync(f);
                }
            }
        }

        private RealEstateDto MapToDto(RealEstate entity)
        {
            return new RealEstateDto(
                entity.Id,
                entity.Title,
                entity.Description,
                entity.Address,
                entity.City,
                entity.Neighborhood,
                entity.Price,
                entity.Area,
                entity.CanBeFurnished,
                entity.ApartmentType,
                entity.CanBeFurnished,
                entity.Furnitures?.Select(f => new FurnitureDto(f.Id, f.Name, f.Price, f.ImagePath, null)).ToList(),
                entity.Rooms,
                entity.Bathrooms,
                entity.UnitType,
                entity.CreatedAt,
                entity.ExpirationDate,
                entity.IsActive,
                entity.WhatsAppNumber,
                entity.Images?.Select(img => new RealEstateImageDto(img.Id, Path.GetFileName(img.ImageUrl), img.ImageUrl)).ToList()
            );
        }

        #endregion
    }
}
