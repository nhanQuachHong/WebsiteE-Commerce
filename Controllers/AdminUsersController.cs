using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteE_Commerce.Models;
using WebsiteE_Commerce.Models.ViewModels.Admin;
using WebsiteE_Commerce.Security;
using WebsiteE_Commerce.Services.Email;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;

namespace WebsiteE_Commerce.Controllers
{
    [Authorize(Policy = "Admin2FA")]
    public class AdminUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAppEmailSender _emailSender;

        public AdminUsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IAppEmailSender emailSender)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.AsNoTracking().ToListAsync();
            var vm = new List<AdminUserListItemViewModel>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                vm.Add(new AdminUserListItemViewModel
                {
                    Id = u.Id,
                    Email = u.Email ?? u.UserName ?? "(no email)",
                    EmailConfirmed = u.EmailConfirmed,
                    TwoFactorEnabled = u.TwoFactorEnabled,
                    IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow,
                    LockoutEnd = u.LockoutEnd,
                    Roles = roles
                });
            }

            return View(vm);
        }

        // -------------------------
        // CREATE USER (Admin tạo User/Seller)
        // -------------------------

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateUserViewModel { Role = AppRoles.User });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            await EnsureCoreRolesAsync();

            var email = model.Email.Trim();
            if (await _userManager.FindByEmailAsync(email) != null)
            {
                ModelState.AddModelError(string.Empty, "Email đã tồn tại.");
                return View(model);
            }

            // Tạo với mật khẩu tạm (không gửi mật khẩu), sẽ gửi link reset để user tự đặt
            var tempPassword = $"Temp@{Guid.NewGuid():N}";

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = false // bắt buộc confirm email
            };

            var createResult = await _userManager.CreateAsync(user, tempPassword);
            if (!createResult.Succeeded)
            {
                foreach (var e in createResult.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            var roleToAssign = model.Role;
            if (!await _roleManager.RoleExistsAsync(roleToAssign))
            {
                ModelState.AddModelError(string.Empty, "Role không tồn tại.");
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, roleToAssign);

            // gửi email xác thực + link đặt mật khẩu
            await SendConfirmAndSetPasswordAsync(user);

            TempData["Info"] = "Tạo tài khoản thành công. Hệ thống đã gửi email xác thực và link đặt mật khẩu.";
            return RedirectToAction(nameof(Index));
        }

        private async Task SendConfirmAndSetPasswordAsync(ApplicationUser user)
        {
            // Confirm email token
            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var emailEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailToken));
            var confirmUrl = Url.Action(
                "ConfirmEmail",
                "Account",
                new { userId = user.Id, token = emailEncoded },
                protocol: Request.Scheme);

            // Reset password token
            var pwdToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var pwdEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(pwdToken));
            var resetUrl = Url.Action(
                "ResetPassword",
                "Account",
                new { email = user.Email, token = pwdEncoded },
                protocol: Request.Scheme);

            var safeConfirm = HtmlEncoder.Default.Encode(confirmUrl ?? "");
            var safeReset = HtmlEncoder.Default.Encode(resetUrl ?? "");

            await _emailSender.SendEmailAsync(
                user.Email!,
                "Tài khoản mới - WebsiteE-Commerce",
                $"1) Xác thực email: <a href='{safeConfirm}'>Xác thực</a><br/>" +
                $"2) Đặt mật khẩu: <a href='{safeReset}'>Đặt mật khẩu</a>");
        }

        // -------------------------
        // EDIT ROLES
        // -------------------------

        [HttpGet]
        public async Task<IActionResult> EditRoles(string id)
        {
            await EnsureCoreRolesAsync();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

            var vm = new EditUserRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? user.UserName ?? "",
                Roles = allRoles.Select(r => new EditUserRolesViewModel.RoleSelection
                {
                    RoleName = r,
                    Selected = userRoles.Contains(r)
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoles(EditUserRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            var selected = model.Roles.Where(r => r.Selected).Select(r => r.RoleName).ToList();

            // Không cho admin tự gỡ role admin của chính mình (tránh tự lock hệ thống)
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == user.Id && currentRoles.Contains(AppRoles.Admin) && !selected.Contains(AppRoles.Admin))
            {
                ModelState.AddModelError(string.Empty, "Bạn không thể tự gỡ role Admin của chính mình.");
                return View(model);
            }

            var toRemove = currentRoles.Except(selected).ToList();
            var toAdd = selected.Except(currentRoles).ToList();

            if (toRemove.Any())
                await _userManager.RemoveFromRolesAsync(user, toRemove);

            if (toAdd.Any())
                await _userManager.AddToRolesAsync(user, toAdd);

            TempData["Info"] = "Cập nhật role thành công.";
            return RedirectToAction(nameof(Index));
        }

        // -------------------------
        // LOCK / UNLOCK
        // -------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Không cho admin tự khóa chính mình
            if (_userManager.GetUserId(User) == user.Id)
            {
                TempData["Info"] = "Không thể khóa chính tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Index));
            }

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);

            await _userManager.UpdateAsync(user);

            TempData["Info"] = "Đã khóa tài khoản.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);

            TempData["Info"] = "Đã mở khóa tài khoản.";
            return RedirectToAction(nameof(Index));
        }

        private async Task EnsureCoreRolesAsync()
        {
            foreach (var r in new[] { AppRoles.Admin, AppRoles.Seller, AppRoles.User })
            {
                if (!await _roleManager.RoleExistsAsync(r))
                    await _roleManager.CreateAsync(new IdentityRole(r));
            }
        }
    }
}
