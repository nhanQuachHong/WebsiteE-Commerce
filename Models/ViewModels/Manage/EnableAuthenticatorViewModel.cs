vusing System.ComponentModel.DataAnnotations;

namespace WebsiteE_Commerce.Models.ViewModels.Manage
{
    public class EnableAuthenticatorViewModel
    {
        public string SharedKey { get; set; } = string.Empty;
        public string AuthenticatorUri { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã xác thực từ ứng dụng Authenticator.")]
        [Display(Name = "Mã xác thực")]
        public string Code { get; set; } = string.Empty;

        public string[]? RecoveryCodes { get; set; }
    }
}
