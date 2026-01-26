using System.ComponentModel.DataAnnotations;

namespace WebsiteE_Commerce.Models.ViewModels.Account
{
    public class LoginWithRecoveryCodeViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Recovery Code.")]
        public string RecoveryCode { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
