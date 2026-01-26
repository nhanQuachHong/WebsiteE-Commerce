using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebsiteE_Commerce.Models;
using WebsiteE_Commerce.Models.ViewModels.Manage;

namespace WebsiteE_Commerce.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private const string Issuer = "WebsiteE-Commerce";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ManageController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Index() => View();

        // -------------------------
        // CHANGE PASSWORD
        // -------------------------

        [HttpGet]
        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);

                return View(model);
            }

            // cập nhật sign-in cookie
            await _signInManager.RefreshSignInAsync(user);

            TempData["Info"] = "Đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Index));
        }

        // -------------------------
        // 2FA AUTHENTICATOR
        // -------------------------

        [HttpGet]
        public async Task<IActionResult> EnableAuthenticator()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var model = await BuildEnableAuthenticatorModelAsync(user);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                // reload shared key + uri để view render lại
                var reload = await BuildEnableAuthenticatorModelAsync(user);
                reload.Code = model.Code;
                return View(reload);
            }

            var code = model.Code.Replace(" ", "").Replace("-", "");

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                _userManager.Options.Tokens.AuthenticatorTokenProvider,
                code);

            if (!isValid)
            {
                var reload = await BuildEnableAuthenticatorModelAsync(user);
                ModelState.AddModelError(string.Empty, "Mã xác thực không đúng.");
                return View(reload);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);

            // Tạo recovery codes (chỉ hiển thị 1 lần)
            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

            TempData["Info"] = "Đã bật 2FA. Vui lòng lưu Recovery Codes.";
            return View("ShowRecoveryCodes", recoveryCodes.ToArray());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable2fa()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await _userManager.SetTwoFactorEnabledAsync(user, false);

            TempData["Info"] = "Đã tắt 2FA.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetAuthenticatorKey()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);

            TempData["Info"] = "Đã reset Authenticator Key. Vui lòng bật lại 2FA.";
            return RedirectToAction(nameof(EnableAuthenticator));
        }

        private async Task<EnableAuthenticatorViewModel> BuildEnableAuthenticatorModelAsync(ApplicationUser user)
        {
            var key = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrWhiteSpace(key))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                key = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var email = user.Email ?? user.UserName ?? "user";
            return new EnableAuthenticatorViewModel
            {
                SharedKey = FormatKey(key!),
                AuthenticatorUri = GenerateOtpUri(email, key!)
            };
        }

        private static string GenerateOtpUri(string email, string unformattedKey)
        {
            // otpauth://totp/{issuer}:{email}?secret={key}&issuer={issuer}&digits=6
            return $"otpauth://totp/{Uri.EscapeDataString(Issuer)}:{Uri.EscapeDataString(email)}" +
                   $"?secret={Uri.EscapeDataString(unformattedKey)}&issuer={Uri.EscapeDataString(Issuer)}&digits=6";
        }

        private static string FormatKey(string unformattedKey)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < unformattedKey.Length; i += 4)
            {
                var len = Math.Min(4, unformattedKey.Length - i);
                sb.Append(unformattedKey.Substring(i, len)).Append(' ');
            }
            return sb.ToString().Trim().ToLowerInvariant();
        }
    }
}
