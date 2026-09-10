using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FabricDevelopmentHub.ViewModels;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "Company")]
    public string? Company { get; set; }

    [Required]
    [Display(Name = "Username")]
    public string? Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password",
        ErrorMessage = "The password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }

    public List<SelectListItem> Companies { get; set; } = new();
}