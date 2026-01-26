namespace WebsiteE_Commerce.Models.ViewModels.Admin
{
    public class AdminUserListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public bool EmailConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }

        public bool IsLockedOut { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }

        public IList<string> Roles { get; set; } = new List<string>();
    }
}
