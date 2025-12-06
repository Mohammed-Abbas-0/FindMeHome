using FindMeHome.Dtos;
using FindMeHome.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using System.Threading.Tasks;

namespace FindMeHome.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IStringLocalizer<SharedResource> localizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _localizer = localizer;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                IsSellerRequest = model.IsSeller
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Always assign User role initially. 
                // If IsSellerRequest is true, Admin will have to approve it later.
                await _userManager.AddToRoleAsync(user, "User");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "RealEstate");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Note: SignInManager uses Username by default, but we are passing Email. 
            // We need to find the user by email first to get the username, or configure Identity to allow Email as Username.

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
                if (signInResult.Succeeded)
                {
                    return RedirectToAction("Index", "RealEstate");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "RealEstate");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null) // && await _userManager.IsEmailConfirmedAsync(user))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResetLink = Url.Action("ResetPassword", "Account", new { email = model.Email, token = token }, Request.Scheme);

                    // Mock Email Sending - Log to Console/Logger
                    // In a real app, use an email service here
                    System.Diagnostics.Debug.WriteLine($"RESET PASSWORD LINK: {passwordResetLink}");

                    return RedirectToAction("ForgotPasswordConfirmation");
                }

                // Don't reveal that the user does not exist or is not confirmed
                return RedirectToAction("ForgotPasswordConfirmation");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid password reset token");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("ResetPasswordConfirmation");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var model = new ProfileDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Username = user.UserName,
                PhoneNumber = user.PhoneNumber,
                Bio = user.Bio,
                ProfilePictureUrl = user.ProfilePictureUrl,
                VerificationStatus = user.VerificationStatus
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(ProfileDto model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(model);

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Bio = model.Bio;

            if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
            {
                user.ProfilePictureUrl = await SaveProfilePicture(model.ProfilePicture);
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                if (model.ProfilePicture != null)
                {
                    TempData["SuccessMessage"] = _localizer["ProfilePictureUpdatedSuccess"].Value;
                }
                else
                {
                    TempData["SuccessMessage"] = _localizer["ProfileUpdatedSuccess"].Value;
                }
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfilePicture(IFormFile profilePicture)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (profilePicture != null && profilePicture.Length > 0)
            {
                try
                {
                    user.ProfilePictureUrl = await SaveProfilePicture(profilePicture);
                    await _userManager.UpdateAsync(user);
                    return Json(new { success = true, message = _localizer["ProfilePictureUpdatedSuccess"].Value, newUrl = user.ProfilePictureUrl });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = string.Format(_localizer["ErrorUploadingImage"].Value, ex.Message) });
                }
            }

            return Json(new { success = false, message = _localizer["NoImageSelected"].Value });
        }

        private async Task<string> SaveProfilePicture(IFormFile file)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var folderPath = Path.Combine("wwwroot", "uploads", "profiles");
            var fullPath = Path.Combine(folderPath, fileName);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/profiles/{fileName}";
        }

        [HttpPost]
        [Authorize(Roles = "Seller")]
        public async Task<IActionResult> RequestVerification()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            if (user.VerificationStatus == FindMeHome.Enums.VerificationStatus.None ||
                user.VerificationStatus == FindMeHome.Enums.VerificationStatus.Rejected)
            {
                user.VerificationStatus = FindMeHome.Enums.VerificationStatus.Pending;
                await _userManager.UpdateAsync(user);
                TempData["SuccessMessage"] = _localizer["VerificationRequestSent"].Value;
            }
            else if (user.VerificationStatus == FindMeHome.Enums.VerificationStatus.Pending)
            {
                TempData["InfoMessage"] = _localizer["VerificationRequestPending"].Value;
            }

            return RedirectToAction("Profile");
        }
    }
}
