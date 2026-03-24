using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ToptanciECommerce.Application.Interfaces;
using ToptanciECommerce.Domain.Entities;
using ToptanciECommerce.Infrastructure.Data;

namespace ToptanciECommerce.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class PaymentsController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    public PaymentsController(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
    {
        _uow         = uow;
        _userManager = userManager;
    }

    // GET: Admin/Payments?period=today|week|month|all
    public async Task<IActionResult> Index(string period = "today")
    {
        var (from, to) = GetDateRange(period);
        var payments   = await _uow.Payments.GetByDateRangeAsync(from, to);

        // Enrich with customer names
        var customerIds = payments.Select(p => p.CustomerId).Distinct().ToList();
        var customers   = new Dictionary<string, string>();
        foreach (var cid in customerIds)
        {
            var user = await _userManager.FindByIdAsync(cid);
            customers[cid] = user?.FullName ?? cid;
        }

        ViewBag.Period    = period;
        ViewBag.From      = from;
        ViewBag.To        = to;
        ViewBag.Customers = customers;
        ViewBag.TotalIn   = payments.Where(p => p.PaymentType == "Tahsilat").Sum(p => p.Amount);
        ViewBag.TotalOut  = payments.Where(p => p.PaymentType == "İade").Sum(p => p.Amount);

        return View(payments);
    }

    // GET: Admin/Payments/Create?customerId=xxx
    public async Task<IActionResult> Create(string? customerId = null)
    {
        var customers = await _userManager.GetUsersInRoleAsync("Customer");
        ViewBag.Customers  = customers.OrderBy(u => u.FullName).ToList();
        ViewBag.PreselectedCustomerId = customerId;

        var orders = customerId != null
            ? await _uow.Orders.GetByCustomerAsync(customerId)
            : new List<Order>().AsReadOnly() as IReadOnlyList<Order>;
        ViewBag.CustomerOrders = orders;

        return View();
    }

    // POST: Admin/Payments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string customerId, decimal amount, string paymentMethod,
        string paymentType, string? notes, int? orderId,
        string? paymentDateStr)
    {
        if (string.IsNullOrEmpty(customerId) || amount <= 0)
        {
            TempData["Error"] = "Müşteri seçimi ve tutar zorunludur.";
            return RedirectToAction(nameof(Create));
        }

        var user = await _userManager.FindByIdAsync(customerId);
        if (user == null) return NotFound();

        var paymentDate = DateTime.TryParse(paymentDateStr, out var pd) ? pd : DateTime.Now;

        var payment = new Payment
        {
            CustomerId       = customerId,
            Amount           = amount,
            PaymentMethod    = paymentMethod,
            PaymentType      = paymentType,
            Notes            = notes,
            OrderId          = orderId > 0 ? orderId : null,
            PaymentDate      = paymentDate,
            RecordedByUserId = _userManager.GetUserId(User)
        };

        await _uow.Payments.AddAsync(payment);

        // Update customer balance
        if (paymentType == "Tahsilat")
            user.Balance -= amount;  // Payment received → reduce debt
        else
            user.Balance += amount;  // Refund → increase debt

        await _uow.SaveChangesAsync();
        await _userManager.UpdateAsync(user);

        TempData["Success"] = $"{user.FullName} için {amount:N2} ₺ {paymentType.ToLower()} kaydedildi.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Admin/Payments/CustomerOrders/{customerId} — AJAX
    [HttpGet]
    public async Task<IActionResult> CustomerOrders(string customerId)
    {
        var orders = await _uow.Orders.GetByCustomerAsync(customerId);
        var result = orders.Select(o => new { o.Id, o.OrderNumber, Total = o.TotalAmount });
        return Json(result);
    }

    private static (DateTime from, DateTime to) GetDateRange(string period)
    {
        var now = DateTime.Now;
        return period switch
        {
            "today"  => (now.Date, now.Date.AddDays(1).AddTicks(-1)),
            "week"   => (now.Date.AddDays(-(int)now.DayOfWeek + 1), now.Date.AddDays(7 - (int)now.DayOfWeek).AddTicks(-1)),
            "month"  => (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, 1).AddMonths(1).AddTicks(-1)),
            _        => (DateTime.MinValue, DateTime.MaxValue)
        };
    }
}
