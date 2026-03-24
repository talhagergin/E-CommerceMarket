using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ToptanciECommerce.Infrastructure.Data;

namespace ToptanciECommerce.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // GET: /Account/Login
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password,
        bool rememberMe = false, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View();

        var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

            return LocalRedirect(returnUrl ?? "/");
        }

        ModelState.AddModelError("", "E-posta veya şifre hatalı.");
        return View();
    }

    // GET: /Account/Register
    public IActionResult Register() => View();

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string firstName, string lastName,
        string companyName, string email, string password, string confirmPassword)
    {
        if (password != confirmPassword)
        {
            ModelState.AddModelError("", "Şifreler eşleşmiyor.");
            return View();
        }

        var user = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName,
            CompanyName = companyName,
            UserName = email,
            Email = email,
            IsApproved = false // Admin approval required for B2B
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Customer");
            TempData["Info"] = "Hesabınız oluşturuldu. Admin onayından sonra giriş yapabilirsiniz.";
            return RedirectToAction(nameof(Login));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View();
    }

    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied() => View();
}
