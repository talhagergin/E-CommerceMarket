using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ToptanciECommerce.Application.DTOs.Customer;
using ToptanciECommerce.Application.Interfaces;
using ToptanciECommerce.Infrastructure.Data;

namespace ToptanciECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CustomersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _uow;

    public CustomersController(UserManager<ApplicationUser> userManager, IUnitOfWork uow)
    {
        _userManager = userManager;
        _uow         = uow;
    }

    // GET: Admin/Customers
    public async Task<IActionResult> Index(string? search)
    {
        var customers = await _userManager.GetUsersInRoleAsync("Customer");

        if (!string.IsNullOrEmpty(search))
        {
            customers = customers
                .Where(u => u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase)
                         || u.CompanyName.Contains(search, StringComparison.OrdinalIgnoreCase)
                         || (u.Email ?? "").Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var orderCounts = new Dictionary<string, int>();
        foreach (var customer in customers)
        {
            var orders = await _uow.Orders.GetByCustomerAsync(customer.Id);
            orderCounts[customer.Id] = orders.Count;
        }

        ViewBag.Search = search;
        ViewBag.OrderCounts = orderCounts;
        return View(customers.OrderByDescending(u => u.CreatedAt).ToList());
    }

    // GET: Admin/Customers/Detail/{id}
    public async Task<IActionResult> Detail(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var orders = await _uow.Orders.GetByCustomerAsync(id);
        ViewBag.Orders = orders;
        return View(user);
    }

    // POST: Admin/Customers/ToggleApproval
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleApproval(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        user.IsApproved = !user.IsApproved;
        await _userManager.UpdateAsync(user);
        TempData["Success"] = $"{user.FullName} kullanıcısı {(user.IsApproved ? "onaylandı" : "onayı kaldırıldı")}.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    // GET: Admin/Customers/Create
    public IActionResult Create() => View(new CreateCustomerDto());

    // POST: Admin/Customers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomerDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        if (dto.Password != dto.ConfirmPassword)
        {
            ModelState.AddModelError("", "Şifreler eşleşmiyor.");
            return View(dto);
        }

        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
        {
            ModelState.AddModelError("", "Bu e-posta adresi zaten kullanımda.");
            return View(dto);
        }

        var user = new ApplicationUser
        {
            FirstName      = dto.FirstName,
            LastName       = dto.LastName,
            CompanyName    = dto.CompanyName,
            Phone          = dto.Phone,
            UserName       = dto.Email,
            Email          = dto.Email,
            Address        = dto.Address,
            City           = dto.City,
            District       = dto.District,
            PostalCode     = dto.PostalCode,
            TaxNumber      = dto.TaxNumber,
            IsApproved     = true,
            EmailConfirmed = true,
            TempPassword   = dto.Password
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            foreach (var err in result.Errors)
                ModelState.AddModelError("", err.Description);
            return View(dto);
        }

        await _userManager.AddToRoleAsync(user, "Customer");
        TempData["Success"] = $"{user.FullName} müşterisi oluşturuldu. Giriş: {dto.Email} / {dto.Password}";
        return RedirectToAction(nameof(Detail), new { id = user.Id });
    }
}
