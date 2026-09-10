using DevExpress.XtraRichEdit.Import.Html;
using FabricDevelopmentHub.Data;
using FabricDevelopmentHub.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FabricDevelopmentHub.Data;
using FabricDevelopmentHub.Models;
using FabricDevelopmentHub.ViewModels;


public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _context = context;
    }
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        var model = new RegisterViewModel();

        LoadCompanies(model);

        return View(model);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                Company = model.Company,
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Check if roles exist; if not, create them
                if (!await _roleManager.RoleExistsAsync("Admin"))
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));

                if (!await _roleManager.RoleExistsAsync("User"))
                    await _roleManager.CreateAsync(new IdentityRole("User"));

                // Assign role
                var userCount = await _userManager.Users.CountAsync();

                if (userCount == 1)
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }

                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("Login", "Account");
            }

            // Display Identity errors
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // IMPORTANT: Reload companies before returning the view
        LoadCompanies(model);

        return View(model);
    }

    [HttpGet]
    public IActionResult Login()
    {
        var model = new LoginViewModel();

        if (Request.Cookies.TryGetValue("RememberedUser", out string? email))
        {
            model.Email = email;
        }

        if (Request.Cookies.TryGetValue("RememberMe", out string? remember))
        {
            model.RememberMe = remember == "true";
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please fill all fields";
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            false);

        if (result.Succeeded)
        {
            if (model.RememberMe)
            {
                var option = new CookieOptions
                {
                    Expires = DateTimeOffset.Now.AddDays(30),
                    HttpOnly = true,
                    IsEssential = true
                };

                Response.Cookies.Append("RememberedUser", model.Email, option);
                Response.Cookies.Append("RememberMe", "true", option);
            }
            else
            {
                Response.Cookies.Delete("RememberedUser");
                Response.Cookies.Delete("RememberMe");
            }

            return RedirectToAction("Index", "Home");
        }

        TempData["ErrorMessage"] = "Invalid username or password";
        return View(model);
    }

    [HttpGet]
    //public async Task<List<SidebarViewModel>> GetUserSidebar(string userId)
    //{
    //    var data = await (
    //        from up in _context.TblUserPermission
    //        where up.UserId == userId

    //        join m in _context.TblModule on up.ModuleId equals m.ModuleId
    //        join me in _context.TblMenu on up.MenuId equals me.MenuId

    //        select new SidebarViewModel
    //        {
    //            ModuleId = m.ModuleId,
    //            ModuleName = m.ModuleName,
    //            MenuId = me.MenuId,
    //            MenuName = me.MenuName,
    //            ActionId = up.ActionId
    //        }
    //    )
    //    .Distinct()
    //    .ToListAsync();

    //    return data;
    //}

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("login", "Account");
    }
    [HttpGet]
    public IActionResult ResetPassword()
    {
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Find the user by email
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("ResetPassword"); // redirect so toaster shows
        }

        // Generate reset token
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // Reset password
        var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = $"Password for {model.Email} has been reset successfully!";
            return RedirectToAction("Login"); // redirect so toaster shows on login page
        }
        else
        {
            TempData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
            return RedirectToAction("ResetPassword");
        }
    }

    [HttpGet]
    private void LoadCompanies(RegisterViewModel model)
    {
        model.Companies = _context.TblCompanyInfo
            .Where(x => x.Status == "Active")
            .Select(x => new SelectListItem
            {
                Value = x.ComName,
                Text = x.ComName
            })
            .ToList();
    }
}
