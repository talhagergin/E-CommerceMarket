using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ToptanciECommerce.Application.Interfaces;
using ToptanciECommerce.Application.Interfaces.Services;
using ToptanciECommerce.Infrastructure.Data;

namespace ToptanciECommerce.Web.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IOrderPdfService _pdfService;

    public OrdersController(IUnitOfWork uow, UserManager<ApplicationUser> userManager, IOrderPdfService pdfService)
    {
        _uow          = uow;
        _userManager  = userManager;
        _pdfService   = pdfService;
    }

    // GET: /Orders
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;
        var orders = await _uow.Orders.GetByCustomerAsync(userId);
        return View(orders);
    }

    // GET: /Orders/Detail/5
    public async Task<IActionResult> Detail(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var order  = await _uow.Orders.GetWithItemsAsync(id);

        if (order == null || order.CustomerId != userId)
            return NotFound();

        return View(order);
    }

    // GET: /Orders/DownloadPdf/5
    public async Task<IActionResult> DownloadPdf(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var order  = await _uow.Orders.GetWithItemsAsync(id);

        if (order == null || order.CustomerId != userId)
            return NotFound();

        var cart = new ToptanciECommerce.Application.DTOs.Cart.CartDto
        {
            Items = order.OrderItems.Select(oi => new ToptanciECommerce.Application.DTOs.Cart.CartItemDto
            {
                ProductId   = oi.ProductId,
                ProductName = oi.ProductName,
                SKU         = oi.ProductSKU,
                UnitPrice   = oi.UnitPrice,
                TaxRate     = oi.TaxRate,
                Quantity    = oi.Quantity
            }).ToList()
        };

        var pdfBytes = _pdfService.GenerateOrderList(cart, order.CustomerName, order.CustomerCompany);
        return File(pdfBytes, "application/pdf", $"Siparis_{order.OrderNumber}.pdf");
    }
}
