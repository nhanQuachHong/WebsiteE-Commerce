using Microsoft.AspNetCore.Authorization;

namespace WebsiteE_Commerce.Security
{
    public sealed class RequireTwoFactorEnabledRequirement : IAuthorizationRequirement
    {
    }
}
