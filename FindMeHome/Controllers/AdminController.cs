using FindMeHome.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FindMeHome.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var pendingSellers = await _userManager.Users
                .Where(u => u.IsSellerRequest)
                .ToListAsync();

            // Filter out those who are already Sellers
            var result = new List<ApplicationUser>();
            foreach (var user in pendingSellers)
            {
                if (!await _userManager.IsInRoleAsync(user, "Seller"))
                {
                    result.Add(user);
                }
            }

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveSeller(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (!await _userManager.IsInRoleAsync(user, "Seller"))
            {
                await _userManager.AddToRoleAsync(user, "Seller");
            }

            // Optional: Reset the request flag if you want, or keep it as history
            // user.IsSellerRequest = false; 
            // await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }
    }
}
