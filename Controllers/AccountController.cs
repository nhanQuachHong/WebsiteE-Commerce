using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebsiteE_Commerce.Models;
using WebsiteE_Commerce.Models.ViewModels.Account;

namespace WebsiteE_Commerce.Controllers
{
    public class AccountController : Controller
    {
        private const string UserRole = "User";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            return View(new RegisterViewModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Tạo user (UserName = Email là cách đơn giản, phổ biến trong đồ án)
            var user = new ApplicationUser
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                EmailConfirmed = true // đồ án: cho qua xác thực email; production: nên bật confirm email
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                foreach (var e in createResult.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);

                return View(model);
            }

            // Đảm bảo role "User" tồn tại (phòng trường hợp seed chưa chạy)
            if (!await _roleManager.RoleExistsAsync(UserRole))
            {
                await _roleManager.CreateAsync(new IdentityRole(UserRole));
            }

            // Gán role mặc định cho user mới
            var roleResult = await _userManager.AddToRoleAsync(user, UserRole);
            if (!roleResult.Succeeded)
            {
                foreach (var e in roleResult.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);

                return View(model);
            }

            // Tự đăng nhập sau đăng ký
            await _signInManager.SignInAsync(user, isPersistent: false);

            _logger.LogInformation("User {Email} registered and signed in.", user.Email);

            return RedirectToLocal(model.ReturnUrl);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = model.Email.Trim();

            // Không để lộ thông tin user tồn tại hay không => thông báo chung
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            // PasswordSignInAsync có lockoutOnFailure để chống brute-force
            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in.", email);
                return RedirectToLocal(model.ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản tạm thời bị khóa do đăng nhập sai nhiều lần. Vui lòng thử lại sau.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(string? returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");

            return RedirectToLocal(returnUrl);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}
