using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WebsiteE_Commerce.Data; // <-- Kiểm tra namespace này cho đúng với dự án của bạn
using WebsiteE_Commerce.Models; // <-- Kiểm tra namespace này cho đúng với dự án của bạn
namespace WebsiteE_Commerce.Controllers
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

        // --- ĐĂNG KÝ (GET) ---
        [HttpGet]
        public IActionResult Register()
        {
            // QUAN TRỌNG: Dấu ~ trỏ ra thư mục gốc Views
            return View("~/Views/Acount/Register.cshtml");
        }

        // --- ĐĂNG KÝ (POST) ---
        [HttpPost]
        public async Task<IActionResult> Register(string email, string password)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = email, Email = email };
                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            // Nếu lỗi, trả về đúng file ở ngoài cùng
            return View("~/Views/Acount/Register.cshtml");
        }

        // --- ĐĂNG NHẬP (GET) ---
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Acount/Login.cshtml");
        }

        // --- ĐĂNG NHẬP (POST) ---
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không đúng.");
            }
            // Nếu lỗi, trả về đúng file ở ngoài cùng
            return View("~/Views/Acount/Login.cshtml");
        }

        // --- ĐĂNG XUẤT ---
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // --- CHECK EMAIL AJAX ---
        [HttpGet]
        public async Task<IActionResult> CheckEmailAvailability(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return Json(new { isTaken = user != null });
        }
    }
}