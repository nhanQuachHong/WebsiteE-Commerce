using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using WebsiteE_Commerce.Models;

namespace WebsiteE_Commerce.Security
{
    public sealed class RequireTwoFactorEnabledHandler : AuthorizationHandler<RequireTwoFactorEnabledRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RequireTwoFactorEnabledHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RequireTwoFactorEnabledRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
                return;

            var user = await _userManager.GetUserAsync(context.User);
            if (user == null)
                return;

            // Nếu user đã bật 2FA => đạt yêu cầu
            if (user.TwoFactorEnabled)
                context.Succeed(requirement);
        }
    }
}
