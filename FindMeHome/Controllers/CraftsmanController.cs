using FindMeHome.Dtos;
using FindMeHome.Services.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace FindMeHome.Controllers
{
    public class CraftsmanController : Controller
    {
        private readonly ICraftsmanService _craftsmanService;

        public CraftsmanController(ICraftsmanService craftsmanService)
        {
            _craftsmanService = craftsmanService;
        }

        public async Task<IActionResult> Index()
        {
            var craftsmen = await _craftsmanService.GetAllAsync();
            return View(craftsmen);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CraftsmanDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var result = await _craftsmanService.AddAsync(dto);
            if (result)
            {
                TempData["Success"] = "تم إضافة الحرفي بنجاح";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "حدث خطأ أثناء الإضافة");
            return View(dto);
        }
    }
}
