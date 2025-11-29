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

        public async Task<IActionResult> Index()
        {
            var realEstates = await _realStateService.GetAllAsync();

            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                var wishlist = await _realStateService.GetWishlistAsync(userId);
                ViewBag.WishlistIds = wishlist.Select(w => w.Id).ToList();
            }
            else
            {
                ViewBag.WishlistIds = new List<int>();
            }

            return View(realEstates);
        }

        [HttpGet("AdvancedSearch")]
        public async Task<IActionResult> AdvancedSearch(string? query, decimal? priceFrom, decimal? priceTo, double? areaFrom, double? areaTo, int? rooms, int? bathrooms, string? city, string? neighborhood, UnitType? unitType, bool? isFurnished)
        {
            var results = await _realStateService.SearchAsync(query, priceFrom, priceTo, areaFrom, areaTo, rooms, bathrooms, city, neighborhood, unitType, isFurnished);

            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                var wishlist = await _realStateService.GetWishlistAsync(userId);
                ViewBag.WishlistIds = wishlist.Select(w => w.Id).ToList();
            }
            else
            {
                ViewBag.WishlistIds = new List<int>();
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

            return View("Index", results);
        }

        [HttpGet("Create")]
        [Authorize(Roles = "Admin,Seller")]
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost("Create")]
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

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var realEstate = await _realStateService.GetByIdAsync(id);
            if (realEstate == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            ViewBag.IsInWishlist = userId != null && await _realStateService.IsInWishlistAsync(id, userId);

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

        [HttpGet("Wishlist")]
        public async Task<IActionResult> Wishlist()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToAction("Login", "Account");

            var wishlist = await _realStateService.GetWishlistAsync(userId);
            return View(wishlist);
        }

        [HttpGet("GetWishlistCount")]
        public async Task<IActionResult> GetWishlistCount()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { count = 0 });

            var wishlist = await _realStateService.GetWishlistAsync(userId);
            return Json(new { count = wishlist.Count() });
        }

        [HttpGet("MyProperties")]
        [Authorize(Roles = "Seller,Admin")]
        public async Task<IActionResult> MyProperties()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToAction("Login", "Account");

            var properties = await _realStateService.GetByUserIdAsync(userId);
            return View(properties);
        }
    }
}
