using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToptanciECommerce.Application.Interfaces;
using ToptanciECommerce.Domain.Enums;

namespace ToptanciECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly IUnitOfWork _uow;

    public DashboardController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IActionResult> Index()
    {
        var (recentOrders, totalOrderCount) = await _uow.Orders.GetPagedAsync(1, 5);

        ViewBag.TotalProducts = await _uow.Products.CountAsync();
        ViewBag.TotalCategories = await _uow.Categories.CountAsync();
        ViewBag.TotalOrders = await _uow.Orders.CountAsync();
        ViewBag.PendingOrders = await _uow.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
        ViewBag.RecentOrders = recentOrders;

        return View();
    }
}
