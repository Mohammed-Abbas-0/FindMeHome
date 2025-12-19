using FindMeHome.AppContext;
using FindMeHome.Dtos;
using FindMeHome.Enums;
using FindMeHome.Models;
using FindMeHome.Repositories.AbstractionLayer;
using FindMeHome.Services.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace FindMeHome.Services.Implementation
{
    public class RealStateService : IRealStateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDBContext _context;

        public RealStateService(IUnitOfWork unitOfWork, AppDBContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
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
                Status = PropertyStatus.Active, // Or Pending if we want to moderate new ones. Prompt implies Edits/Deletes need moderation. Let's start Active.
                CreatedAt = DateTime.Now,
                ExpirationDate = DateTime.Now.AddMonths(2),
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

        public async Task<List<RealEstateDto>> GetPendingPropertiesAsync()
        {
            var entities = await _context.RealEstates
                .Include(r => r.Images)
                .Include(r => r.User)
                .Where(r => r.Status == PropertyStatus.PendingApproval || r.Status == PropertyStatus.PendingDeletion)
                .OrderByDescending(r => r.UpdatedAt)
                .ToListAsync();

            return entities.Select(MapToDto).ToList();
        }

        public async Task<RealEstateDto?> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.RealEstates
                .GetByIdAsync(id, includeProperties: "Images,Furnitures,Likes");
            if (entity == null)
                return null;

            return MapToDto(entity);
        }

        public async Task<PagedResultDto<RealEstateDto>> GetAllAsync(int page = 1, int pageSize = 50)
        {
            var entities = await _unitOfWork.RealEstates
                .GetAllAsync(includeProperties: "Images,Furnitures,Likes");

            // Public listing should only show Active and non-expired
            var query = entities
                .Where(e => (e.Status == 0 || e.Status == PropertyStatus.Active) && (e.ExpirationDate == null || e.ExpirationDate > DateTime.Now))
                .OrderByDescending(e => e.CreatedAt);

            var totalCount = query.Count();
            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToDto)
                .ToList();

            return new PagedResultDto<RealEstateDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<List<RealEstateDto>> GetByUserIdAsync(string userId)
        {
            var entities = await _unitOfWork.RealEstates
                .FindAsync(e => e.UserId == userId, includeProperties: "Images,Furnitures,Likes");

            return entities.Select(MapToDto).ToList();
        }

        public async Task<PagedResultDto<RealEstateDto>> SearchAsync(string? query, decimal? minPrice, decimal? maxPrice, double? minArea, double? maxArea, int? rooms, int? bathrooms, string? city, string? neighborhood, UnitType? unitType, bool? isFurnished, string? location = null, int page = 1, int pageSize = 50)
        {
            var entities = await _unitOfWork.RealEstates
                .GetAllAsync(includeProperties: "Images,Furnitures,Likes");

            var filtered = entities.Where(x => x.Status == PropertyStatus.Active && (x.ExpirationDate == null || x.ExpirationDate > DateTime.Now)).AsQueryable();

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

            if (!string.IsNullOrWhiteSpace(location))
            {
                filtered = filtered.Where(x => (x.City != null && x.City.Contains(location)) ||
                                             (x.Neighborhood != null && x.Neighborhood.Contains(location)));
            }

            if (unitType.HasValue)
                filtered = filtered.Where(x => x.UnitType == unitType.Value);

            if (isFurnished.HasValue)
                filtered = filtered.Where(x => x.CanBeFurnished == isFurnished.Value);

            var totalCount = filtered.Count();
            var items = filtered
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToDto)
                .ToList();

            return new PagedResultDto<RealEstateDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
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

        public async Task<ResultDto> LikePropertyAsync(int realEstateId, string userId)
        {
            var exists = await _unitOfWork.PropertyLikes
                .FindAsync(l => l.RealEstateId == realEstateId && l.UserId == userId);

            if (exists.Any())
                return ResultDto.Failure("لقد أعجبت بهذا العقار بالفعل");

            var realEstate = await _unitOfWork.RealEstates.GetByIdAsync(realEstateId);
            if (realEstate == null)
                return ResultDto.Failure("العقار غير موجود");

            var like = new PropertyLike
            {
                RealEstateId = realEstateId,
                UserId = userId,
                LikedAt = DateTime.Now
            };

            await _unitOfWork.PropertyLikes.AddAsync(like);
            await _unitOfWork.CompleteAsync();

            return ResultDto.Success("تم الإعجاب بالعقار بنجاح ❤️");
        }

        public async Task<ResultDto> UnlikePropertyAsync(int realEstateId, string userId)
        {
            var like = await _unitOfWork.PropertyLikes
                .FindAsync(l => l.RealEstateId == realEstateId && l.UserId == userId);

            var item = like.FirstOrDefault();
            if (item == null)
                return ResultDto.Failure("لم تقم بالإعجاب بهذا العقار من قبل");

            _unitOfWork.PropertyLikes.Remove(item);
            await _unitOfWork.CompleteAsync();

            return ResultDto.Success("تم إلغاء الإعجاب بالعقار");
        }

        public async Task<bool> IsLikedByUserAsync(int realEstateId, string userId)
        {
            var like = await _unitOfWork.PropertyLikes
                .FindAsync(l => l.RealEstateId == realEstateId && l.UserId == userId);

            return like.Any();
        }

        public async Task<int> GetLikesCountAsync(int realEstateId)
        {
            var likes = await _unitOfWork.PropertyLikes
                .FindAsync(l => l.RealEstateId == realEstateId);

            return likes.Count();
        }


        public async Task<ResultDto> UpdateAsync(int id, CreateRealEstateDto dto, string userId)
        {
            var entity = await _unitOfWork.RealEstates
                .GetByIdAsync(id, includeProperties: "Images,Furnitures");

            if (entity == null)
                return ResultDto.Failure("العقار غير موجود");

            if (entity.UserId != userId)
                return ResultDto.Failure("لا تملك صلاحية تعديل هذا العقار");

            // Validate only essential fields (skip image validation if entity already has images)
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

            // Only require new images if the entity has no existing images
            if ((dto.Images == null || dto.Images.Count == 0) && (entity.Images == null || !entity.Images.Any()))
                return ResultDto.Failure("يجب رفع صورة واحدة على الأقل للعقار.");

            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.Address = dto.Address;
            entity.City = dto.City;
            entity.Neighborhood = dto.Neighborhood;
            entity.Price = dto.Price;
            entity.Area = dto.Area;
            entity.ApartmentType = dto.ApartmentType;
            entity.CanBeFurnished = dto.CanBeFurnished;
            entity.Rooms = dto.Rooms;
            entity.Bathrooms = dto.Bathrooms;
            entity.UnitType = dto.UnitType;
            entity.WhatsAppNumber = dto.WhatsAppNumber;
            entity.WhatsAppNumber = dto.WhatsAppNumber;
            entity.Status = PropertyStatus.PendingApproval;
            entity.UpdatedAt = DateTime.Now;

            // Handle Images: For simplicity in this iteration, we are APPENDING new images. 
            // A more complex UI would allow deleting specific images.
            await SaveImages(dto, entity);

            // Handle Furniture: 
            // For simplicity, we are NOT updating furniture in this iteration to avoid complexity with existing IDs.
            // If the user wants to update furniture, they might need a separate UI or we clear and re-add (which is risky for IDs).
            // Let's assume furniture update is out of scope for this simple "Edit Property" request unless specified.

            _unitOfWork.RealEstates.Update(entity);
            await _unitOfWork.CompleteAsync();

            return ResultDto.Success("✅ تم إرسال طلب التعديل إلى المسؤول بنجاح");
        }

        public async Task<ResultDto> DeleteAsync(int id, string userId)
        {
            var entity = await _unitOfWork.RealEstates
                .GetByIdAsync(id, includeProperties: "Images,Furnitures");

            if (entity == null)
                return ResultDto.Failure("العقار غير موجود");

            if (entity.UserId != userId)
                return ResultDto.Failure("لا تملك صلاحية حذف هذا العقار");

            // Soft Delete request
            entity.Status = PropertyStatus.PendingDeletion;
            // entity.DeletedAt = DateTime.Now; // Set this only when actually deleted/approved? No, prompt says "Request delete".
            // So we just change status.

            _unitOfWork.RealEstates.Update(entity); // Update instead of Remove
            await _unitOfWork.CompleteAsync();

            return ResultDto.Success("✅ تم إرسال طلب الحذف إلى المسؤول بنجاح");
        }

        public async Task<ResultDto> UpdateStatusAsync(int id, PropertyStatus status)
        {
            var entity = await _unitOfWork.RealEstates.GetByIdAsync(id);
            if (entity == null)
                return ResultDto.Failure("العقار غير موجود");

            entity.Status = status;
            if (status == PropertyStatus.Deleted)
            {
                entity.DeletedAt = DateTime.Now;
            }

            _unitOfWork.RealEstates.Update(entity);
            await _unitOfWork.CompleteAsync();

            return ResultDto.Success("✅ تم تحديث حالة العقار بنجاح");
        }

        public async Task<List<LocationSuggestionDto>> GetLocationsAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new List<LocationSuggestionDto>();

            term = term.ToLower();

            var allProperties = await _unitOfWork.RealEstates.GetAllAsync();

            var cities = allProperties
                .Where(p => p.City != null && p.City.ToLower().Contains(term))
                .GroupBy(p => p.City)
                .Select(g => new LocationSuggestionDto
                {
                    Name = g.Key,
                    Type = "City",
                    Count = g.Count()
                });

            var neighborhoods = allProperties
                .Where(p => p.Neighborhood != null && p.Neighborhood.ToLower().Contains(term))
                .GroupBy(p => p.Neighborhood)
                .Select(g => new LocationSuggestionDto
                {
                    Name = g.Key,
                    Type = "Neighborhood",
                    Count = g.Count()
                });

            return cities.Concat(neighborhoods)
                         .OrderByDescending(x => x.Count)
                         .ThenBy(x => x.Name)
                         .Take(10)
                         .ToList();
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
                entity.Images?.Select(img => new RealEstateImageDto(img.Id, Path.GetFileName(img.ImageUrl), img.ImageUrl)).ToList(),
                entity.Likes != null ? entity.Likes.Count : 0,
                entity.Status,
                entity.UserId,
                entity.User != null ? new UserDto(entity.User.FirstName, entity.User.LastName, entity.User.Email, entity.User.ProfilePictureUrl) : null,
                entity.UpdatedAt
            );
        }

        #endregion
    }
}
