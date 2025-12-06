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
            var requests = new List<FindMeHome.ViewModels.AdminRequestViewModel>();

            // 1. Seller Registration Requests
            var pendingSellers = await _userManager.Users
                .Where(u => u.IsSellerRequest)
                .ToListAsync();

            foreach (var user in pendingSellers)
            {
                if (!await _userManager.IsInRoleAsync(user, "Seller"))
                {
                    requests.Add(new FindMeHome.ViewModels.AdminRequestViewModel
                    {
                        UserId = user.Id,
                        FullName = $"{user.FirstName} {user.LastName}",
                        Email = user.Email,
                        Type = FindMeHome.ViewModels.RequestType.SellerRegistration,
                        ProfilePictureUrl = user.ProfilePictureUrl
                    });
                }
            }

            // 2. Verification Requests
            var pendingVerification = await _userManager.Users
                .Where(u => u.VerificationStatus == FindMeHome.Enums.VerificationStatus.Pending)
                .ToListAsync();

            foreach (var user in pendingVerification)
            {
                // Avoid duplicates if user is in both lists (though unlikely to be pending seller AND pending verification simultaneously in normal flow, but possible)
                if (!requests.Any(r => r.UserId == user.Id && r.Type == FindMeHome.ViewModels.RequestType.SellerRegistration))
                {
                    requests.Add(new FindMeHome.ViewModels.AdminRequestViewModel
                    {
                        UserId = user.Id,
                        FullName = $"{user.FirstName} {user.LastName}",
                        Email = user.Email,
                        Type = FindMeHome.ViewModels.RequestType.Verification,
                        ProfilePictureUrl = user.ProfilePictureUrl
                    });
                }
                else if (requests.Any(r => r.UserId == user.Id))
                {
                    // If user is already in list as SellerRegistration, we might want to show both or just one. 
                    // For now, let's add it as a separate request type if they are distinct actions.
                    requests.Add(new FindMeHome.ViewModels.AdminRequestViewModel
                    {
                        UserId = user.Id,
                        FullName = $"{user.FirstName} {user.LastName}",
                        Email = user.Email,
                        Type = FindMeHome.ViewModels.RequestType.Verification,
                        ProfilePictureUrl = user.ProfilePictureUrl
                    });
                }
            }

            return View(requests);
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

        public async Task<IActionResult> PendingRequests()
        {
            var pendingUsers = await _userManager.Users
                .Where(u => u.VerificationStatus == FindMeHome.Enums.VerificationStatus.Pending)
                .ToListAsync();

            return View(pendingUsers);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveVerification(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.VerificationStatus = FindMeHome.Enums.VerificationStatus.Verified;
            await _userManager.UpdateAsync(user);

            if (!await _userManager.IsInRoleAsync(user, "Seller"))
            {
                await _userManager.AddToRoleAsync(user, "Seller");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RejectVerification(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.VerificationStatus = FindMeHome.Enums.VerificationStatus.Rejected;
            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }
    }
}
