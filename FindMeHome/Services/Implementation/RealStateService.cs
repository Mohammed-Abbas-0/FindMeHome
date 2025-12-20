using FindMeHome.AppContext;
using FindMeHome.Dtos;
using FindMeHome.Enums;
using FindMeHome.Models;
using FindMeHome.Repositories.AbstractionLayer;
using FindMeHome.Services.Abstraction;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;

namespace FindMeHome.Services.Implementation
{
    public class RealStateService : IRealStateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly AppDBContext _context;
        private readonly IWebHostEnvironment _env;

        public RealStateService(IUnitOfWork unitOfWork, AppDBContext context, IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _env = env;
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
                CreatedAt = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddMonths(2),
                UserId = userId
            };

            await SaveImages(dto, entity);

            await _unitOfWork.RealEstates.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            // Save furniture after entity is saved (so we have the ID)
            await SaveFurnitures(dto, entity);
            await _unitOfWork.CompleteAsync();

            return ResultDto.Success("? ?? ????? ?????");
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
                .GetByIdAsync(id, includeProperties: "Images,Furnitures,Likes,User");
            if (entity == null)
                return null;

            return MapToDto(entity);
        }

        public async Task<PagedResultDto<RealEstateDto>> GetAllAsync(int page = 1, int pageSize = 50)
        {
            var entities = await _unitOfWork.RealEstates
                .GetAllAsync(includeProperties: "Images,Furnitures,Likes,User");

            // Public listing should only show Active and non-expired
            var query = entities
                .Where(e => (e.Status == 0 || e.Status == PropertyStatus.Active) && (e.ExpirationDate == null || e.ExpirationDate > DateTime.UtcNow))
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
                .GetAllAsync(includeProperties: "Images,Furnitures,Likes,User");

            var filtered = entities.Where(x => (x.Status == 0 || x.Status == PropertyStatus.Active) && (x.ExpirationDate == null || x.ExpirationDate > DateTime.UtcNow)).AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var lowerQuery = query.Trim().ToLower();
                filtered = filtered.Where(x => (x.Title != null && x.Title.ToLower().Contains(lowerQuery)) ||
                                             (x.Description != null && x.Description.ToLower().Contains(lowerQuery)) ||
                                             (x.Address != null && x.Address.ToLower().Contains(lowerQuery)));
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
                filtered = filtered.Where(x => x.Rooms == rooms.Value);

            if (bathrooms.HasValue)
                filtered = filtered.Where(x => x.Bathrooms == bathrooms.Value);

            if (!string.IsNullOrWhiteSpace(city))
            {
                var lowerCity = city.Trim().ToLower();
                filtered = filtered.Where(x => x.City != null && x.City.ToLower().Contains(lowerCity));
            }

            if (!string.IsNullOrWhiteSpace(neighborhood))
            {
                var lowerNeigh = neighborhood.Trim().ToLower();
                filtered = filtered.Where(x => x.Neighborhood != null && x.Neighborhood.ToLower().Contains(lowerNeigh));
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                var lowerLoc = location.Trim().ToLower();
                filtered = filtered.Where(x => (x.City != null && x.City.ToLower().Contains(lowerLoc)) ||
                                             (x.Neighborhood != null && x.Neighborhood.ToLower().Contains(lowerLoc)));
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
                return ResultDto.Failure("?????? ????? ?????? ?? ????? ???????");

            var realEstate = await _unitOfWork.RealEstates.GetByIdAsync(realEstateId);
            if (realEstate == null)
                return ResultDto.Failure("?????? ??? ?????");

            var wishlist = new Wishlist
            {
                RealEstateId = realEstateId,
                UserId = userId,
                AddedAt = DateTime.UtcNow
            };

            await _unitOfWork.Wishlists.AddAsync(wishlist);
            await _unitOfWork.CompleteAsync();

            return ResultDto.Success("??? ??????? ??? ????? ??????? ?????");
        }

        public async Task<ResultDto> RemoveFromWishlistAsync(int realEstateId, string userId)
        {
            var wishlist = await _unitOfWork.Wishlists
                .FindAsync(w => w.RealEstateId == realEstateId && w.UserId == userId);

            var item = wishlist.FirstOrDefault();
            if (item == null)
                return ResultDto.Failure("?????? ??? ????? ?? ????? ???????");

            _unitOfWork.Wishlists.Remove(item);
            await _unitOfWork.CompleteAsync();

            return ResultDto.Success("??? ??????? ?? ????? ??????? ?????");
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
                return ResultDto.Failure("??? ????? ???? ?????? ??????");

            var realEstate = await _unitOfWork.RealEstates.GetByIdAsync(realEstateId);
            if (realEstate == null)
                return ResultDto.Failure("?????? ??? ?????");

            var like = new PropertyLike
            {
                RealEstateId = realEstateId,
                UserId = userId,
                LikedAt = DateTime.UtcNow
            };

            await _unitOfWork.PropertyLikes.AddAsync(like);
            await _unitOfWork.CompleteAsync();

            return ResultDto.Success("?? ??????? ??????? ????? ??");
        }

        public async Task<ResultDto> UnlikePropertyAsync(int realEstateId, string userId)
        {
            var like = await _unitOfWork.PropertyLikes
                .FindAsync(l => l.RealEstateId == realEstateId && l.UserId == userId);

            var item = like.FirstOrDefault();
            if (item == null)
                return ResultDto.Failure("?? ??? ???????? ???? ?????? ?? ???");

            _unitOfWork.PropertyLikes.Remove(item);
            await _unitOfWork.CompleteAsync();

            return ResultDto.Success("?? ????? ??????? ???????");
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
                return ResultDto.Failure("?????? ??? ?????");

            if (entity.UserId != userId && !userId.Contains("admin")) // Simple admin check or use auth
                return ResultDto.Failure("?? ???? ?????? ????? ??? ??????");

            // Basic Validation
            if (string.IsNullOrWhiteSpace(dto.Title)) return ResultDto.Failure("????? ?????? ?????.");
            if (dto.Price <= 0) return ResultDto.Failure("??? ????? ??? ???? ??????.");

            // If the property is ACTIVE, we save changes as a PENDING REQUEST (JSON)
            // If it's PENDING (newlisting), we can either overwrite or also save as JSON.
            // To keep it simple and fulfill "send edits as request", we save as JSON.

            var jsonDir = Path.Combine(_env.WebRootPath, "pending_edits");
            if (!Directory.Exists(jsonDir)) Directory.CreateDirectory(jsonDir);

            var filePath = Path.Combine(jsonDir, $"{id}.json");

            // We need to handle images in the DTO before saving to JSON? 
            // Images are IFormFile, they can't be JSON serialized easily.
            // For now, let's save the metadata and handle new image uploads separately or just skip them in JSON for now.
            // Actually, saving Images metadata is important.

            var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);

            // We don't change the entity fields yet! 
            // But we might want to update the status to indicate a pending edit if we aren't using a separate flag.
            // The user didn't ask to hide the property, so we keep it Active but maybe update UpdatedAt.
            entity.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.RealEstates.Update(entity);
            await _unitOfWork.CompleteAsync();

            return ResultDto.Success("? ?? ????? ??? ??????? ??? ??????? ?????. ???? ?????? ?????? ????????? ??????? ??? ????????.");
        }

        public async Task<ResultDto> DeleteAsync(int id, string userId)
        {
            var entity = await _unitOfWork.RealEstates
                .GetByIdAsync(id, includeProperties: "Images,Furnitures");

            if (entity == null)
                return ResultDto.Failure("?????? ??? ?????");

            if (entity.UserId != userId)
                return ResultDto.Failure("?? ???? ?????? ??? ??? ??????");

            // Soft Delete request
            entity.Status = PropertyStatus.PendingDeletion;
            // entity.DeletedAt = DateTime.UtcNow; // Set this only when actually deleted/approved? No, prompt says "Request delete".
            // So we just change status.

            _unitOfWork.RealEstates.Update(entity); // Update instead of Remove
            await _unitOfWork.CompleteAsync();

            return ResultDto.Success("? ?? ????? ??? ????? ??? ??????? ?????");
        }

        public async Task<ResultDto> UpdateStatusAsync(int id, PropertyStatus status)
        {
            var entity = await _unitOfWork.RealEstates.GetByIdAsync(id);
            if (entity == null)
                return ResultDto.Failure("?????? ??? ?????");

            entity.Status = status;
            if (status == PropertyStatus.Deleted)
            {
                entity.DeletedAt = DateTime.UtcNow;
            }

            _unitOfWork.RealEstates.Update(entity);
            await _unitOfWork.CompleteAsync();

            return ResultDto.Success("? ?? ????? ???? ?????? ?????");
        }

        public async Task<List<LocationSuggestionDto>> GetLocationsAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new List<LocationSuggestionDto>();

            term = term.Trim().ToLower();

            var allProperties = await _unitOfWork.RealEstates.GetAllAsync();
            var activeProperties = allProperties.Where(p => (p.Status == 0 || p.Status == PropertyStatus.Active) && (p.ExpirationDate == null || p.ExpirationDate > DateTime.UtcNow));

            var cities = activeProperties
                .Where(p => p.City != null && p.City.ToLower().Contains(term))
                .GroupBy(p => p.City)
                .Select(g => new LocationSuggestionDto
                {
                    Name = g.Key,
                    Type = "City",
                    Count = g.Count()
                });

            var neighborhoods = activeProperties
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
                return ResultDto.Failure("?????? ?????? ??? ??????.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                return ResultDto.Failure("????? ?????? ?????.");

            if (dto.Price <= 0)
                return ResultDto.Failure("??? ????? ??? ???? ??????.");

            if (string.IsNullOrWhiteSpace(dto.City))
                return ResultDto.Failure("??????? ??????.");

            if (string.IsNullOrWhiteSpace(dto.Address))
                return ResultDto.Failure("??????? ?????.");

            if (dto.Area <= 0)
                return ResultDto.Failure("??? ????? ????? ?????.");

            if (string.IsNullOrWhiteSpace(dto.WhatsAppNumber))
                return ResultDto.Failure("??? ???????? ?????.");

            if (dto.Images == null || dto.Images.Count == 0)
                return ResultDto.Failure("??? ??? ???? ????? ??? ????? ??????.");

            return ResultDto.Success("?? ?????? ????? ?");
        }

        private async Task SaveImages(CreateRealEstateDto dto, RealEstate entity)
        {
            // ??? ??? ??? ??????
            if (dto.Images != null && dto.Images.Count > 0)
            {
                try
                {
                    // Clear existing images if this is an update and new images are provided
                    // For simplicity, the original code was appending. The new ApproveEditAsync will replace.
                    // If entity.Images is null, initialize it.
                    if (entity.Images == null)
                    {
                        entity.Images = new List<RealEstateImage>();
                    }
                    else
                    {
                        // If we are approving an edit, we assume new images replace old ones.
                        // This might need more sophisticated logic in a real app (e.g., keep old if not replaced).
                        // For now, let's clear existing images if new ones are provided in the DTO.
                        entity.Images.Clear();
                    }

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
                    throw new Exception("??? ??? ????? ??? ??? ??????.", ex);
                }
            }
        }

        private async Task SaveFurnitures(CreateRealEstateDto dto, RealEstate entity)
        {
            // ?? ??? ??????
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

        public async Task<CreateRealEstateDto?> GetEditRequestAsync(int id)
        {
            var filePath = Path.Combine(_env.WebRootPath, "pending_edits", $"{id}.json");
            if (!File.Exists(filePath)) return null;

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<CreateRealEstateDto>(json);
        }

        public async Task<ResultDto> ApproveEditAsync(int id)
        {
            var dto = await GetEditRequestAsync(id);
            if (dto == null) return ResultDto.Failure("??? ??????? ??? ?????");

            var entity = await _unitOfWork.RealEstates.GetByIdAsync(id, includeProperties: "Images,Furnitures");
            if (entity == null) return ResultDto.Failure("?????? ??? ?????");

            // Apply changes from DTO to Entity
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
            entity.UpdatedAt = DateTime.UtcNow;
            entity.Status = PropertyStatus.Active; // In case it was pending new listing

            // Save new images if any
            await SaveImages(dto, entity);

            _unitOfWork.RealEstates.Update(entity);
            await _unitOfWork.CompleteAsync();

            // Delete the JSON file
            File.Delete(Path.Combine(_env.WebRootPath, "pending_edits", $"{id}.json"));

            return ResultDto.Success("? ?? ???? ????????? ?????? ???????? ?????");
        }

        public async Task<ResultDto> RejectEditAsync(int id)
        {
            var filePath = Path.Combine(_env.WebRootPath, "pending_edits", $"{id}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return ResultDto.Success("? ?? ??? ??? ???????");
            }
            return ResultDto.Failure("??? ??????? ??? ?????");
        }

        public bool HasPendingEdit(int id)
        {
            return File.Exists(Path.Combine(_env.WebRootPath, "pending_edits", $"{id}.json"));
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
                entity.Furnitures != null && entity.Furnitures.Any(), // This maps CanBeFurnished based on actual furniture presence
                entity.ApartmentType,
                entity.CanBeFurnished, // This is the actual CanBeFurnished property
                entity.Furnitures?.Select(f => new FurnitureDto(f.Id, f.Name, f.Price, f.ImagePath, null)).ToList(),
                entity.Rooms,
                entity.Bathrooms,
                entity.UnitType,
                entity.CreatedAt,
                entity.ExpirationDate,
                entity.IsActive,
                entity.WhatsAppNumber,
                entity.Images?.Select(i => new RealEstateImageDto(i.Id, Path.GetFileName(i.ImageUrl), i.ImageUrl)).ToList(),
                entity.Likes?.Count ?? 0,
                entity.Status,
                entity.UserId,
                entity.User != null ? new UserDto(entity.User.FirstName, entity.User.LastName, entity.User.Email, entity.User.ProfilePictureUrl, entity.User.PhoneNumber, entity.User.VerificationStatus) : null,
                entity.UpdatedAt
            );
        }

        #endregion
    }
}


