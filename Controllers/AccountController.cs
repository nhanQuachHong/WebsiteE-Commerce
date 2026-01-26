using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using WebsiteE_Commerce.Models;
using WebsiteE_Commerce.Models.ViewModels.Account;
using WebsiteE_Commerce.Security;
using WebsiteE_Commerce.Services.Email;

namespace WebsiteE_Commerce.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAppEmailSender _emailSender;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IAppEmailSender emailSender,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        // -------------------------
        // REGISTER + EMAIL CONFIRM
        // -------------------------

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
            => View(new RegisterViewModel { ReturnUrl = returnUrl });

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = model.Email.Trim();

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = false
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);
            if (!createResult.Succeeded)
            {
                foreach (var e in createResult.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            // đảm bảo role User tồn tại
            if (!await _roleManager.RoleExistsAsync(AppRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(AppRoles.User));

            await _userManager.AddToRoleAsync(user, AppRoles.User);

            // gửi email xác thực
            await SendEmailConfirmationAsync(user);

            // Không auto-login vì RequireConfirmedEmail = true
            TempData["Info"] = "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản trước khi đăng nhập.";
            return RedirectToAction(nameof(Login), new { returnUrl = model.ReturnUrl });
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                return BadRequest("Thiếu thông tin xác thực email.");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("Không tìm thấy tài khoản.");

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
            {
                return View("ConfirmEmailSuccess");
            }

            return View("ConfirmEmailFail");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResendEmailConfirmation() => View();

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email.Trim());

            // Không tiết lộ user tồn tại/không tồn tại
            if (user != null && !user.EmailConfirmed)
            {
                await SendEmailConfirmationAsync(user);
            }

            return View("ResendEmailConfirmationSent");
        }

        private async Task SendEmailConfirmationAsync(ApplicationUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var url = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                new { userId = user.Id, token = encoded },
                protocol: Request.Scheme);

            var safeUrl = HtmlEncoder.Default.Encode(url ?? "");
            await _emailSender.SendEmailAsync(
                user.Email!,
                "Xác thực email - WebsiteE-Commerce",
                $"Vui lòng xác thực email bằng cách nhấn vào link sau: <a href='{safeUrl}'>Xác thực</a>");
        }

        // -------------------------
        // LOGIN / LOGOUT + 2FA
        // -------------------------

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
            => View(new LoginViewModel { ReturnUrl = returnUrl });

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = model.Email.Trim();
            var user = await _userManager.FindByEmailAsync(email);

            // thông báo chung chống dò tài khoản
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(nameof(LoginWith2fa), new
                {
                    returnUrl = model.ReturnUrl,
                    rememberMe = model.RememberMe
                });
            }

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in.", email);
                return RedirectToLocal(model.ReturnUrl);
            }

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản chưa được xác thực email. Vui lòng kiểm tra email để xác thực.");
                return View(model);
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản tạm khóa do đăng nhập sai nhiều lần. Vui lòng thử lại sau.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> LoginWith2fa(bool rememberMe, string? returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) return RedirectToAction(nameof(Login));

            return View(new LoginWith2faViewModel
            {
                RememberMe = rememberMe,
                ReturnUrl = returnUrl
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) return RedirectToAction(nameof(Login));

            var code = model.TwoFactorCode.Replace(" ", "").Replace("-", "");

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
                code,
                model.RememberMe,
                model.RememberMachine);

            if (result.Succeeded)
                return RedirectToLocal(model.ReturnUrl);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản bị khóa tạm thời.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Mã 2FA không đúng.");
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult LoginWithRecoveryCode(string? returnUrl = null)
            => View(new LoginWithRecoveryCodeViewModel { ReturnUrl = returnUrl });

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null) return RedirectToAction(nameof(Login));

            var code = model.RecoveryCode.Replace(" ", "").Replace("-", "");

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(code);

            if (result.Succeeded)
                return RedirectToLocal(model.ReturnUrl);

            ModelState.AddModelError(string.Empty, "Recovery Code không đúng.");
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(string? returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToLocal(returnUrl);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // -------------------------
        // FORGOT / RESET PASSWORD
        // -------------------------

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = model.Email.Trim();
            var user = await _userManager.FindByEmailAsync(email);

            // Không tiết lộ user có tồn tại hay không
            if (user == null || !user.EmailConfirmed)
            {
                return View("ForgotPasswordConfirmation");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var url = Url.Action(
                nameof(ResetPassword),
                "Account",
                new { email = email, token = encoded },
                protocol: Request.Scheme);

            var safeUrl = HtmlEncoder.Default.Encode(url ?? "");
            await _emailSender.SendEmailAsync(
                email,
                "Đặt lại mật khẩu - WebsiteE-Commerce",
                $"Nhấn vào link để đặt lại mật khẩu: <a href='{safeUrl}'>Đặt lại mật khẩu</a>");

            return View("ForgotPasswordConfirmation");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            return View(new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email.Trim());
            if (user == null)
            {
                // Không tiết lộ user
                return View("ResetPasswordConfirmation");
            }

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.Password);

            if (result.Succeeded)
                return View("ResetPasswordConfirmation");

            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);

            return View(model);
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}
