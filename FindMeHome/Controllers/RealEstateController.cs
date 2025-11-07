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
        public RealEstateController(ILogger<RealEstateController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        { // بيانات تجريبية مؤقتة
            var fakeData = new List<RealEstate>
            {
                new RealEstate
                {
                    Id = 1,
                    Title = "شقة فاخرة بالتجمع الخامس",
                    Description = "شقة مفروشة بالكامل بالقرب من الخدمات",
                    Address = "شارع التسعين",
                    City = "القاهرة",
                    Neighborhood = "التجمع الخامس",
                    Price = 2500000,
                    Area = 180,
                    Rooms = 3,
                    Bathrooms = 2,
                    UnitType = UnitType.Residential,
                    IsForSale = true,
                    IsFurnished = true,
                    CreatedAt = DateTime.Now
                },
                new RealEstate
                {
                    Id = 2,
                    Title = "مكتب إداري للإيجار بمدينة نصر",
                    Description = "مكتب بموقع ممتاز قريب من سيتي ستارز",
                    Address = "شارع عباس العقاد",
                    City = "القاهرة",
                    Neighborhood = "مدينة نصر",
                    Price = 15000,
                    Area = 120,
                    Rooms = 4,
                    Bathrooms = 1,
                    UnitType = UnitType.Commercial,
                    IsForSale = false,
                    IsFurnished = false,
                    CreatedAt = DateTime.Now
                },
                new RealEstate
                {
                    Id = 3,
                    Title = "دوبلكس فاخر بالشيخ زايد",
                    Description = "دوبلكس رائع بتشطيب سوبر لوكس وحديقة صغيرة",
                    Address = "كمبوند جرينز",
                    City = "الجيزة",
                    Neighborhood = "الشيخ زايد",
                    Price = 3500000,
                    Area = 250,
                    Rooms = 4,
                    Bathrooms = 3,
                    UnitType = UnitType.Residential,
                    IsForSale = true,
                    IsFurnished = true,
                    CreatedAt = DateTime.Now
                }
            };

            return View(fakeData);
            //var realEstates = await _unitOfWork.RealEstates.GetAllAsync();
            //return View(realEstates);
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
        public async Task<IActionResult> CreateAsync([FromForm] RealEstateDto realEstateDto)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { isSuccess = false, message = "❌ البيانات المدخلة غير صحيحة" });
            }

            var result = await _realStateService.CreateAsync(realEstateDto);

            return Json(new
            {
                isSuccess = result.IsSuccess,
                message = result.Message
            });
        }


    }
}
