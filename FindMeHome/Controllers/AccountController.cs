using FindMeHome.Dtos;
using FindMeHome.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FindMeHome.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
                ProfilePictureUrl = user.ProfilePictureUrl
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
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ProfilePicture.FileName)}";
                var folderPath = Path.Combine("wwwroot", "uploads", "profiles");
                var fullPath = Path.Combine(folderPath, fileName);

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.ProfilePicture.CopyToAsync(stream);
                }

                user.ProfilePictureUrl = $"/uploads/profiles/{fileName}";
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
    }
}
