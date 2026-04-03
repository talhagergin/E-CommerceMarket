using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ToptanciECommerce.Application.DTOs.Order;
using ToptanciECommerce.Application.Interfaces;
using ToptanciECommerce.Application.Interfaces.Services;
using ToptanciECommerce.Domain.Enums;
using ToptanciECommerce.Infrastructure.Data;

namespace ToptanciECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IOrderService _orderService;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(IUnitOfWork uow, IOrderService orderService, UserManager<ApplicationUser> userManager)
    {
        _uow          = uow;
        _orderService = orderService;
        _userManager  = userManager;
    }

    // GET: Admin/Orders
    public async Task<IActionResult> Index(int page = 1, string? search = null, OrderStatus? status = null)
    {
        const int pageSize = 20;
        var (orders, total) = await _uow.Orders.GetPagedAsync(page, pageSize, status, search);

        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
        ViewBag.TotalCount = total;
        ViewBag.Search = search;
        ViewBag.SelectedStatus = status;
        ViewBag.Statuses = Enum.GetValues<OrderStatus>();

        return View(orders);
    }

    // GET: Admin/Orders/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var order = await _uow.Orders.GetWithItemsAsync(id);
        if (order == null) return NotFound();
        return View(order);
    }

    // POST: Admin/Orders/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
    {
        await _uow.Orders.UpdateStatusAsync(id, status);
        await _uow.SaveChangesAsync();
        TempData["Success"] = "Sipariş durumu güncellendi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Admin/Orders/Create?customerId=xxx
    public async Task<IActionResult> Create(string? customerId = null)
    {
        var customers = await _userManager.GetUsersInRoleAsync("Customer");
        var products  = await _uow.Products.GetAllAsync();

        ViewBag.Customers = customers.OrderBy(u => u.FullName).ToList();
        ViewBag.Products  = products.Where(p => p.IsActive).OrderBy(p => p.Name).ToList();
        ViewBag.PreselectedCustomerId = customerId;

        return View(new ManualOrderDto { CustomerId = customerId ?? "" });
    }

    // POST: Admin/Orders/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ManualOrderDto dto)
    {
        // Remove lines with 0 quantity
        dto.Lines = dto.Lines.Where(l => l.Quantity > 0).ToList();

        if (!dto.Lines.Any())
            ModelState.AddModelError("", "En az bir ürün kalemi giriniz.");

        if (!ModelState.IsValid)
        {
            var customers = await _userManager.GetUsersInRoleAsync("Customer");
            var products  = await _uow.Products.GetAllAsync();
            ViewBag.Customers = customers.OrderBy(u => u.FullName).ToList();
            ViewBag.Products  = products.Where(p => p.IsActive).OrderBy(p => p.Name).ToList();
            return View(dto);
        }

        try
        {
            var order = await _orderService.CreateManualAsync(dto);
            TempData["Success"] = $"Sipariş #{order.OrderNumber} başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            var customers = await _userManager.GetUsersInRoleAsync("Customer");
            var products  = await _uow.Products.GetAllAsync();
            ViewBag.Customers = customers.OrderBy(u => u.FullName).ToList();
            ViewBag.Products  = products.Where(p => p.IsActive).OrderBy(p => p.Name).ToList();
            return View(dto);
        }
    }

    // GET: Admin/Orders/ProductSearch?q=... (AJAX)
    [HttpGet]
    public async Task<IActionResult> ProductSearch(string q)
    {
        var all = await _uow.Products.GetAllAsync();
        var results = all
            .Where(p => p.IsActive &&
                   (p.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    p.SKU.Contains(q, StringComparison.OrdinalIgnoreCase)))
            .Take(10)
            .Select(p => new { p.Id, p.Name, p.SKU, Price = p.WholesalePrice });

        return Json(results);
    }
}
