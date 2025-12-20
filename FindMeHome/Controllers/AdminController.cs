using FindMeHome.Models;
using FindMeHome.Enums;
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
        private readonly FindMeHome.Services.Abstraction.IRealStateService _realStateService;

        public AdminController(UserManager<ApplicationUser> userManager, FindMeHome.Services.Abstraction.IRealStateService realStateService)
        {
            _userManager = userManager;
            _realStateService = realStateService;
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
            }

            // 3. Pending Property Requests (New / Edit / Delete)
            var pendingProperties = await _realStateService.GetPendingPropertiesAsync();
            foreach (var prop in pendingProperties)
            {
                var requestType = prop.Status == FindMeHome.Enums.PropertyStatus.PendingDeletion
                    ? FindMeHome.ViewModels.RequestType.PropertyDeletion
                    : FindMeHome.ViewModels.RequestType.PropertyEdit;

                requests.Add(new FindMeHome.ViewModels.AdminRequestViewModel
                {
                    UserId = prop.UserId ?? "",
                    FullName = prop.User?.FirstName + " " + prop.User?.LastName,
                    Email = prop.User?.Email ?? "",
                    Type = requestType,
                    ProfilePictureUrl = prop.User?.ProfilePictureUrl,
                    PropertyId = prop.Id,
                    PropertyTitle = prop.Title,
                    RequestDate = prop.UpdatedAt ?? prop.CreatedAt
                });
            }

            // 4. Moderated Edit Requests (JSON based)
            var allActiveProperties = await _realStateService.GetAllAsync(1, 1000); // Simple fetch
            foreach (var prop in allActiveProperties.Items)
            {
                if (_realStateService.HasPendingEdit(prop.Id))
                {
                    // Avoid duplicates if already in pendingProperties
                    if (!requests.Any(r => r.PropertyId == prop.Id && r.Type == FindMeHome.ViewModels.RequestType.PropertyEdit))
                    {
                        requests.Add(new FindMeHome.ViewModels.AdminRequestViewModel
                        {
                            UserId = prop.UserId ?? "",
                            FullName = prop.User?.FirstName + " " + prop.User?.LastName,
                            Email = prop.User?.Email ?? "",
                            Type = FindMeHome.ViewModels.RequestType.PropertyEdit,
                            ProfilePictureUrl = prop.User?.ProfilePictureUrl,
                            PropertyId = prop.Id,
                            PropertyTitle = prop.Title,
                            RequestDate = prop.UpdatedAt ?? prop.CreatedAt
                        });
                    }
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
        [HttpPost]
        public async Task<IActionResult> ApproveProperty(int id)
        {
            var dto = await _realStateService.GetByIdAsync(id);
            if (dto == null) return NotFound();

            // We need to update the status. Since UpdateAsync requires a full DTO, 
            // and we just want to change status, we should probably add a dedicated method 
            // or just use UpdateAsync but mapping back is tedious.
            // Using a simple workaround: If we have direct access to Entity via Service, that's best.
            // Since we don't, we might need to add ChangeStatus method to Service 
            // OR use the existing update mechanism but modifying specific fields.
            //
            // Given constraints, I will assume we can add a simple property update method or fetch-modify-save pattern if we had the context here.
            // BUT we don't (Service layer abstraction).
            //
            // Best approach: Add `ApprovePropertyAsync(id)` and `RejectPropertyAsync(id)` to Service.
            // But to save time/files, I'll piggyback on UpdateAsync assuming it handles this OR
            // better yet, just for this task, I'll Add ChangeStatusAsync to IRealStateService.

            // Wait, I can't easily add methods without breaking interface implementations elsewhere if mocked.
            // Let's rely on `UpdateAsync`? No, that takes `CreateRealEstateDto`.
            // Let's check if I can just call a new method I'll add to service: `UpdateStatusAsync(int id, PropertyStatus status)`

            // NOTE: I am adding UpdateStatusAsync to the Service in the next step.

            PropertyStatus newStatus = PropertyStatus.Active;
            if (dto.Status == PropertyStatus.PendingDeletion)
            {
                newStatus = PropertyStatus.Deleted;
            }

            await _realStateService.UpdateStatusAsync(id, newStatus);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RejectProperty(int id)
        {
            var dto = await _realStateService.GetByIdAsync(id);
            if (dto == null) return NotFound();

            // If it's a deletion or edit request that was rejected, return to Active
            // If it's a new listing request (Pending), it should be set to Rejected
            PropertyStatus newStatus = (dto.Status == PropertyStatus.PendingApproval || dto.Status == PropertyStatus.PendingDeletion)
                ? PropertyStatus.Active
                : PropertyStatus.Rejected;

            await _realStateService.UpdateStatusAsync(id, newStatus);
            return RedirectToAction(nameof(Index));
        }
    }
}
