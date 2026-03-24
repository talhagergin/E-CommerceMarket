using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ToptanciECommerce.Application.DTOs.Cart;
using ToptanciECommerce.Application.DTOs.Order;
using ToptanciECommerce.Application.Interfaces;
using ToptanciECommerce.Application.Interfaces.Services;
using ToptanciECommerce.Infrastructure.Data;

namespace ToptanciECommerce.Web.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cart;
    private readonly IUnitOfWork _uow;
    private readonly IOrderPdfService _pdfService;
    private readonly IOrderService _orderService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartController(
        ICartService cart,
        IUnitOfWork uow,
        IOrderPdfService pdfService,
        IOrderService orderService,
        UserManager<ApplicationUser> userManager)
    {
        _cart         = cart;
        _uow          = uow;
        _pdfService   = pdfService;
        _orderService = orderService;
        _userManager  = userManager;
    }

    // GET: /Cart
    public IActionResult Index()
    {
        var cart = _cart.GetCart();
        return View(cart);
    }

    // GET: /Cart/Count
    [HttpGet]
    public IActionResult Count() => Json(_cart.GetItemCount());

    // POST: /Cart/Add
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Add([FromBody] AddToCartRequest request)
    {
        if (request.Quantity <= 0)
            return BadRequest("Miktar 0'dan büyük olmalıdır.");

        var product = await _uow.Products.GetWithImagesAsync(request.ProductId);
        if (product == null || !product.IsActive)
            return NotFound("Ürün bulunamadı.");

        if (product.StockQuantity == 0)
            return BadRequest("Bu ürün stokta bulunmamaktadır.");

        var mainImg = product.Images.FirstOrDefault(i => i.IsMain)
                   ?? product.Images.FirstOrDefault();

        _cart.AddItem(new CartItemDto
        {
            ProductId        = product.Id,
            ProductName      = product.Name,
            ProductSlug      = product.Slug,
            SKU              = product.SKU,
            ImageUrl         = mainImg?.ImageUrl,
            UnitPrice        = product.WholesalePrice,
            TaxRate          = product.TaxRate,
            Quantity         = request.Quantity,
            QuantityStep     = product.QuantityStep,
            MinOrderQuantity = product.MinOrderQuantity,
            StockQuantity    = product.StockQuantity
        });

        return Ok(new { count = _cart.GetItemCount() });
    }

    // POST: /Cart/Update
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult Update([FromBody] UpdateCartItemDto dto)
    {
        _cart.UpdateQuantity(dto.ProductId, dto.Quantity);
        var cart = _cart.GetCart();
        var item = cart.Items.FirstOrDefault(x => x.ProductId == dto.ProductId);
        return Ok(new
        {
            count     = _cart.GetItemCount(),
            lineTotal = item?.LineTotal.ToString("N2"),
            subTotal  = cart.SubTotal.ToString("N2"),
            taxAmount = cart.TaxAmount.ToString("N2"),
            total     = cart.Total.ToString("N2")
        });
    }

    // POST: /Cart/Remove
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult Remove([FromBody] RemoveFromCartRequest request)
    {
        _cart.RemoveItem(request.ProductId);
        var cart = _cart.GetCart();
        return Ok(new
        {
            count     = _cart.GetItemCount(),
            subTotal  = cart.SubTotal.ToString("N2"),
            taxAmount = cart.TaxAmount.ToString("N2"),
            total     = cart.Total.ToString("N2")
        });
    }

    // POST: /Cart/Clear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        _cart.Clear();
        TempData["Success"] = "Sepet temizlendi.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Cart/OrderInfo — Shipping form before PDF (login required)
    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> OrderInfo()
    {
        var cart = _cart.GetCart();
        if (!cart.Items.Any())
        {
            TempData["Error"] = "Sepetiniz boş.";
            return RedirectToAction(nameof(Index));
        }

        // Pre-fill from user profile
        var dto = new ShippingInfoDto();
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                dto.CustomerName    = user.FullName;
                dto.CustomerCompany = user.CompanyName;
                dto.CustomerEmail   = user.Email ?? "";
                dto.CustomerPhone   = user.Phone ?? "";
                dto.Address         = user.Address ?? "";
                dto.City            = user.City ?? "";
                dto.District        = user.District ?? "";
                dto.PostalCode      = user.PostalCode ?? "";
            }
        }

        ViewBag.Cart = cart;
        return View(dto);
    }

    // POST: /Cart/OrderInfo — Create order + generate PDF (login required)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> OrderInfo(ShippingInfoDto dto)
    {
        var cart = _cart.GetCart();
        if (!cart.Items.Any())
        {
            TempData["Error"] = "Sepetiniz boş.";
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Cart = cart;
            return View(dto);
        }

        try
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "Oturum bilgisi alınamadı. Lütfen tekrar giriş yapın.");
                ViewBag.Cart = cart;
                return View(dto);
            }

            var order = await _orderService.CreateFromCartAsync(cart, userId, dto);
            _cart.Clear();

            TempData["OrderSuccess"] = $"Siparişiniz oluşturuldu! Sipariş No: #{order.OrderNumber}";
            return RedirectToAction("Detail", "Orders", new { id = order.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Sipariş oluşturulurken hata oluştu: " + ex.Message);
            ViewBag.Cart = cart;
            return View(dto);
        }
    }
}

public record AddToCartRequest(int ProductId, int Quantity);
public record RemoveFromCartRequest(int ProductId);
