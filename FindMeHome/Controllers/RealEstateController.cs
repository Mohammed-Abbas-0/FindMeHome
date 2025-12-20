using FindMeHome.Dtos;
using FindMeHome.Enums;
using FindMeHome.Models;
using FindMeHome.Services.Abstraction;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FindMeHome.Controllers
{
    public class RealEstateController : Controller
    {
        private readonly ILogger<RealEstateController> _logger;
        private readonly IRealStateService _realStateService;
        private readonly UserManager<ApplicationUser> _userManager;

        public RealEstateController(ILogger<RealEstateController> logger, IRealStateService realStateService, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _realStateService = realStateService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var result = await _realStateService.GetAllAsync(page, 9);

            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                var wishlist = await _realStateService.GetWishlistAsync(userId);
                ViewBag.WishlistIds = wishlist.Select(w => w.Id).ToList();

                // Get liked property IDs
                var likedIds = new List<int>();
                foreach (var estate in result.Items)
                {
                    if (await _realStateService.IsLikedByUserAsync(estate.Id, userId))
                    {
                        likedIds.Add(estate.Id);
                    }
                }
                ViewBag.LikedIds = likedIds;
            }
            else
            {
                ViewBag.WishlistIds = new List<int>();
                ViewBag.LikedIds = new List<int>();
            }

            return View(result);
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> AdvancedSearch(string? query, decimal? priceFrom, decimal? priceTo, double? areaFrom, double? areaTo, int? rooms, int? bathrooms, string? city, string? neighborhood, UnitType? unitType, bool? isFurnished, string? location, int page = 1)
        {
            var results = await _realStateService.SearchAsync(query, priceFrom, priceTo, areaFrom, areaTo, rooms, bathrooms, city, neighborhood, unitType, isFurnished, location, page, 9);

            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                var wishlist = await _realStateService.GetWishlistAsync(userId);
                ViewBag.WishlistIds = wishlist.Select(w => w.Id).ToList();

                // Get liked property IDs
                var likedIds = new List<int>();
                foreach (var estate in results.Items)
                {
                    if (await _realStateService.IsLikedByUserAsync(estate.Id, userId))
                    {
                        likedIds.Add(estate.Id);
                    }
                }
                ViewBag.LikedIds = likedIds;
            }
            else
            {
                ViewBag.WishlistIds = new List<int>();
                ViewBag.LikedIds = new List<int>();
            }

            // Preserve filter values in ViewBag to repopulate the form
            ViewBag.Query = query;
            ViewBag.PriceFrom = priceFrom;
            ViewBag.PriceTo = priceTo;
            ViewBag.AreaFrom = areaFrom;
            ViewBag.AreaTo = areaTo;
            ViewBag.Rooms = rooms;
            ViewBag.Bathrooms = bathrooms;
            ViewBag.City = city;
            ViewBag.Neighborhood = neighborhood;
            ViewBag.UnitType = unitType;
            ViewBag.IsFurnished = isFurnished;
            ViewBag.Location = location;

            return View("Index", results);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Seller")]
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> CreateAsync([FromForm] CreateRealEstateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { isSuccess = false, message = "❌ البيانات المدخلة غير صحيحة" });
            }
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null) return Json(new { isSuccess = false, message = "❌ يجب تسجيل الدخول أولاً" });

                var result = await _realStateService.CreateAsync(createDto, userId);

                return Json(new
                {
                    isSuccess = result.IsSuccess,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    isSuccess = false,
                    message = "❌ حدث خطأ غير متوقع: " + ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var realEstate = await _realStateService.GetByIdAsync(id);
            if (realEstate == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                ViewBag.IsInWishlist = await _realStateService.IsInWishlistAsync(id, userId);
            }

            // Fetch seller verification status
            if (!string.IsNullOrEmpty(realEstate.UserId))
            {
                var seller = await _userManager.FindByIdAsync(realEstate.UserId);
                if (seller != null)
                {
                    ViewBag.SellerVerificationStatus = seller.VerificationStatus;
                    ViewBag.SellerPhoneNumber = seller.PhoneNumber;
                }
            }

            return View(realEstate);
        }

        [HttpPost]
        public async Task<IActionResult> AddToWishlist(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { isSuccess = false, message = "يجب تسجيل الدخول أولاً" });

            var result = await _realStateService.AddToWishlistAsync(id, userId);

            // Get updated count
            var wishlist = await _realStateService.GetWishlistAsync(userId);
            var count = wishlist.Count;

            return Json(new
            {
                isSuccess = result.IsSuccess,
                message = result.Message,
                count = count
            });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromWishlist(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { isSuccess = false, message = "يجب تسجيل الدخول أولاً" });

            var result = await _realStateService.RemoveFromWishlistAsync(id, userId);

            // Get updated count
            var wishlist = await _realStateService.GetWishlistAsync(userId);
            var count = wishlist.Count;

            return Json(new
            {
                isSuccess = result.IsSuccess,
                message = result.Message,
                count = count
            });
        }

        [HttpGet]
        public async Task<IActionResult> Wishlist()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToAction("Login", "Account");

            var wishlist = await _realStateService.GetWishlistAsync(userId);
            return View(wishlist);
        }

        [HttpGet]
        public async Task<IActionResult> GetWishlistCount()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { count = 0 });

            var wishlist = await _realStateService.GetWishlistAsync(userId);
            return Json(new { count = wishlist.Count() });
        }

        [HttpPost]
        public async Task<IActionResult> LikeProperty(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { isSuccess = false, message = "يجب تسجيل الدخول أولاً" });

            var result = await _realStateService.LikePropertyAsync(id, userId);

            // Get updated count
            var count = await _realStateService.GetLikesCountAsync(id);

            return Json(new
            {
                isSuccess = result.IsSuccess,
                message = result.Message,
                count = count
            });
        }

        [HttpPost]
        public async Task<IActionResult> UnlikeProperty(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { isSuccess = false, message = "يجب تسجيل الدخول أولاً" });

            var result = await _realStateService.UnlikePropertyAsync(id, userId);

            // Get updated count
            var count = await _realStateService.GetLikesCountAsync(id);

            return Json(new
            {
                isSuccess = result.IsSuccess,
                message = result.Message,
                count = count
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetLikesCount(int id)
        {
            var count = await _realStateService.GetLikesCountAsync(id);
            return Json(new { count = count });
        }

        [HttpGet]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> MyProperties()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToAction("Login", "Account");

            var properties = await _realStateService.GetByUserIdAsync(userId);
            return View(properties);
        }
        [HttpGet]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToAction("Login", "Account");

            var realEstate = await _realStateService.GetByIdAsync(id);
            if (realEstate == null) return NotFound();

            // Check ownership (or admin) - Service also checks, but good to check here for UI redirection
            // Note: GetByIdAsync returns DTO which doesn't have UserId directly exposed usually, 
            // but we can check if the current user is the owner if we expose UserId in DTO or check via service.
            // For now, let's assume the service `UpdateAsync` will handle the security check, 
            // but for GET we should also verify. 
            // Let's add UserId to RealEstateDto if not present, or fetch entity to check.
            // Wait, RealEstateDto doesn't have UserId. Let's check the service implementation.
            // The service `GetByUserIdAsync` filters by user. 
            // Ideally `GetByIdAsync` should return owner info or we use a specific method `GetForEditAsync`.
            // For simplicity, let's proceed and let the view/post handle it, or fetch via a new service method if needed.
            // Actually, let's trust the user for the GET (or better, check ownership).
            // Since I can't easily check ownership without modifying DTO, I will rely on the POST action for strict security,
            // and for GET, I'll just show the view. If they try to save, it will fail.
            // BETTER: Fetch all user properties and check if ID is in there.
            var userProperties = await _realStateService.GetByUserIdAsync(userId);
            if (!userProperties.Any(p => p.Id == id) && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Map DTO to CreateRealEstateDto for the edit view
            var editDto = new CreateRealEstateDto
            {
                Title = realEstate.Title,
                Description = realEstate.Description,
                Address = realEstate.Address,
                City = realEstate.City,
                Neighborhood = realEstate.Neighborhood,
                Price = realEstate.Price,
                Area = realEstate.Area,
                ApartmentType = realEstate.ApartmentType,
                CanBeFurnished = realEstate.CanBeFurnished,
                Rooms = realEstate.Rooms,
                Bathrooms = realEstate.Bathrooms,
                UnitType = realEstate.UnitType,
                WhatsAppNumber = realEstate.WhatsAppNumber,
                // Images and Furniture are handled separately or just displayed
            };

            ViewBag.Id = id;
            ViewBag.ExistingImages = realEstate.Images;

            // Check for pending edit draft
            var pendingDraft = await _realStateService.GetEditRequestAsync(id);
            if (pendingDraft != null)
            {
                editDto = pendingDraft;
                ViewBag.IsDraft = true;
                ViewBag.DraftMessage = "⚠️ أنت تشاهد حالياً مسودة تعديلات قيد المراجعة. يمكنك تعديلها أو انتظار موافقة المسؤول.";
            }

            return View(editDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Edit(int id, [FromForm] CreateRealEstateDto dto)
        {
            if (!ModelState.IsValid)
                return Json(new { isSuccess = false, message = "❌ البيانات المدخلة غير صحيحة" });

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { isSuccess = false, message = "❌ يجب تسجيل الدخول أولاً" });

            try
            {
                var result = await _realStateService.UpdateAsync(id, dto, userId);
                return Json(new { isSuccess = result.IsSuccess, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = "❌ حدث خطأ غير متوقع: " + ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { isSuccess = false, message = "❌ يجب تسجيل الدخول أولاً" });

            try
            {
                var result = await _realStateService.DeleteAsync(id, userId);
                return Json(new { isSuccess = result.IsSuccess, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = "❌ حدث خطأ غير متوقع: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLocations(string term)
        {
            var locations = await _realStateService.GetLocationsAsync(term);
            return Json(locations);
        }
    }
}
