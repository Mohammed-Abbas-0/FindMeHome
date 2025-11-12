using FindMeHome.Dtos;
using FindMeHome.Enums;
using FindMeHome.Models;
using FindMeHome.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace FindMeHome.Controllers
{
    public class RealEstateController : Controller
    {
        private readonly ILogger<RealEstateController> _logger;
        private readonly IRealStateService _realStateService;
        public RealEstateController(ILogger<RealEstateController> logger, IRealStateService realStateService)
        {
            _logger = logger;
            _realStateService = realStateService;
        }

        public async Task<IActionResult> Index()
        {
            var realEstates = await _realStateService.GetAllAsync();
            return View(realEstates);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        //[HttpPost("Create")]
        //public async Task<IActionResult> CreateAsync([FromForm] RealEstateDto realEstateDto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        var errors = ModelState
        //                             .Where(ms => ms.Value.Errors.Count > 0)
        //                             .Select(ms => new
        //                             {
        //                                 Field = ms.Key, // اسم الحقل اللي فيه المشكلة
        //                                 Errors = ms.Value!.Errors.Select(e => e.ErrorMessage).ToList()
        //                             })
        //                             .ToList();

        //        // ممكن تعرضها في الـ ViewBag مثلاً عشان تشوفها:
        //        ViewBag.ValidationErrors = errors;

        //        // أو لو عايز تطبعها في الـ console أثناء التطوير
        //        foreach (var error in errors)
        //        {
        //            Console.WriteLine($"❌ {error.Field}: {string.Join(", ", error.Errors)}");
        //        }

        //        ModelState.AddModelError("", "❌ البيانات المدخلة غير صحيحة، يرجى التحقق والمحاولة مرة أخرى.");
        //        return View(realEstateDto);
        //    }

        //    var result = await _realStateService.CreateAsync(realEstateDto);

        //    if (!result.IsSuccess)
        //    {
        //        ModelState.AddModelError("", result.Message ?? "");
        //        return View(realEstateDto);
        //    }

        //    TempData["Success"] = "✅ تم حفظ العقار بنجاح";
        //    return RedirectToAction("Index");
        //}

        public IActionResult Privacy()
        {
            return View();
        }
        [HttpPost("Create")]
        public async Task<IActionResult> CreateAsync([FromForm] CreateRealEstateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { isSuccess = false, message = "❌ البيانات المدخلة غير صحيحة" });
            }
            try
            {
                var result = await _realStateService.CreateAsync(createDto);

                return Json(new
                {
                    isSuccess = result.IsSuccess,
                    message = result.Message
                });
            }
            catch(Exception ex)
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

            // Get current user ID (temporary - using session or default)
            var userId = HttpContext.Session.GetString("UserId") ?? "default-user";
            ViewBag.IsInWishlist = await _realStateService.IsInWishlistAsync(id, userId);

            return View(realEstate);
        }

        [HttpPost("AddToWishlist/{id}")]
        public async Task<IActionResult> AddToWishlist(int id)
        {
            var userId = HttpContext.Session.GetString("UserId") ?? "default-user";
            var result = await _realStateService.AddToWishlistAsync(id, userId);
            
            return Json(new
            {
                isSuccess = result.IsSuccess,
                message = result.Message
            });
        }

        [HttpPost("RemoveFromWishlist/{id}")]
        public async Task<IActionResult> RemoveFromWishlist(int id)
        {
            var userId = HttpContext.Session.GetString("UserId") ?? "default-user";
            var result = await _realStateService.RemoveFromWishlistAsync(id, userId);
            
            return Json(new
            {
                isSuccess = result.IsSuccess,
                message = result.Message
            });
        }

        [HttpGet("Wishlist")]
        public async Task<IActionResult> Wishlist()
        {
            var userId = HttpContext.Session.GetString("UserId") ?? "default-user";
            var wishlist = await _realStateService.GetWishlistAsync(userId);
            return View(wishlist);
        }

    }
}
