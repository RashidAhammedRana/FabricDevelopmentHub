using System.ComponentModel.DataAnnotations;
namespace FabricDevelopmentHub.ViewModels;
public class ResetPasswordViewModel
{
    [Required]
    //[EmailAddress]
    [Display(Name = "Email / Username")]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmNewPassword { get; set; }
}
