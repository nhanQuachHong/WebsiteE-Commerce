using Microsoft.AspNetCore.Identity;
using WebsiteE_Commerce.Models;

namespace WebsiteE_Commerce.Data
{
    public static class IdentitySeedData
    {
        private const string AdminRole = "Admin";
        private const string UserRole = "User";

        public static async Task SeedAsync(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // 1) Create roles if not exist
            await EnsureRoleAsync(roleManager, AdminRole);
            await EnsureRoleAsync(roleManager, UserRole);

            // 2) Create default admin if not exist
            var adminEmail = config["SeedAdmin:Email"] ?? "admin@ecommerce.local";
            var adminPassword = config["SeedAdmin:Password"] ?? "Admin@12345";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (!createResult.Succeeded)
                {
                    var msg = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Không tạo được admin mặc định: {msg}");
                }
            }

            if (!await userManager.IsInRoleAsync(adminUser, AdminRole))
            {
                var addRoleResult = await userManager.AddToRoleAsync(adminUser, AdminRole);
                if (!addRoleResult.Succeeded)
                {
                    var msg = string.Join("; ", addRoleResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Không gán được role Admin: {msg}");
                }
            }
        }

        private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    var msg = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Không tạo được role '{roleName}': {msg}");
                }
            }
        }
    }
}
