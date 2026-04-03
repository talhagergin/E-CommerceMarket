using Microsoft.AspNetCore.Identity;
using ToptanciECommerce.Application.DTOs.Cart;
using ToptanciECommerce.Application.DTOs.Order;
using ToptanciECommerce.Application.Interfaces;
using ToptanciECommerce.Application.Interfaces.Services;
using ToptanciECommerce.Domain.Entities;
using ToptanciECommerce.Domain.Enums;
using ToptanciECommerce.Infrastructure.Data;

namespace ToptanciECommerce.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrderService(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    public async Task<Order> CreateFromCartAsync(CartDto cart, string userId, ShippingInfoDto shipping)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("Kullanıcı bulunamadı.");

        var order = new Order
        {
            OrderNumber      = GenerateOrderNumber(),
            CustomerId       = userId,
            CustomerName     = shipping.CustomerName,
            CustomerEmail    = shipping.CustomerEmail,
            CustomerPhone    = shipping.CustomerPhone,
            CustomerCompany  = shipping.CustomerCompany,
            ShippingAddress  = shipping.Address,
            ShippingDistrict = shipping.District,
            ShippingCity     = shipping.City,
            ShippingPostalCode = shipping.PostalCode,
            ShippingCountry  = "Türkiye",
            SubTotal         = cart.SubTotal,
            TaxAmount        = cart.TaxAmount,
            ShippingAmount   = 0,
            DiscountAmount   = 0,
            TotalAmount      = cart.Total,
            Status           = OrderStatus.Listed,
            Notes            = shipping.Notes,
            PaymentMethod    = "PDF Sipariş Listesi"
        };

        foreach (var item in cart.Items)
        {
            order.OrderItems.Add(new OrderItem
            {
                ProductId   = item.ProductId,
                ProductName = item.ProductName,
                ProductSKU  = item.SKU,
                UnitPrice   = item.UnitPrice,
                Quantity    = item.Quantity,
                TaxRate     = item.TaxRate,
                TaxAmount   = item.TaxAmount,
                LineTotal    = item.LineTotal
            });
        }

        // Update user profile with address if not set
        bool profileChanged = false;
        if (string.IsNullOrEmpty(user.Address))  { user.Address  = shipping.Address;  profileChanged = true; }
        if (string.IsNullOrEmpty(user.City))     { user.City     = shipping.City;     profileChanged = true; }
        if (string.IsNullOrEmpty(user.District)) { user.District = shipping.District; profileChanged = true; }
        if (string.IsNullOrEmpty(user.Phone))    { user.Phone    = shipping.CustomerPhone; profileChanged = true; }
        if (profileChanged) await _userManager.UpdateAsync(user);

        await _uow.Orders.AddAsync(order);
        await _uow.SaveChangesAsync();

        // Update customer balance (add order total as debt)
        user.Balance += order.TotalAmount;
        await _userManager.UpdateAsync(user);

        return order;
    }

    public async Task<Order> CreateManualAsync(ManualOrderDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.CustomerId)
            ?? throw new InvalidOperationException("Müşteri bulunamadı.");

        decimal subTotal = 0, taxTotal = 0;
        var lines = new List<OrderItem>();

        foreach (var line in dto.Lines.Where(l => l.Quantity > 0))
        {
            var product = await _uow.Products.GetByIdAsync(line.ProductId);
            if (product == null) continue;

            var lineTotal = product.WholesalePrice * line.Quantity;
            var taxAmount = lineTotal * (product.TaxRate / 100);

            lines.Add(new OrderItem
            {
                ProductId   = product.Id,
                ProductName = product.Name,
                ProductSKU  = product.SKU,
                UnitPrice   = product.WholesalePrice,
                Quantity    = line.Quantity,
                TaxRate     = product.TaxRate,
                TaxAmount   = taxAmount,
                LineTotal    = lineTotal
            });

            subTotal += lineTotal;
            taxTotal += taxAmount;
        }

        if (!lines.Any())
            throw new InvalidOperationException("En az bir ürün kalemi gereklidir.");

        var order = new Order
        {
            OrderNumber      = GenerateOrderNumber(),
            CustomerId       = dto.CustomerId,
            CustomerName     = user.FullName,
            CustomerEmail    = user.Email ?? "",
            CustomerPhone    = user.Phone ?? "",
            CustomerCompany  = user.CompanyName,
            ShippingAddress  = user.Address ?? "",
            ShippingDistrict = user.District ?? "",
            ShippingCity     = user.City ?? "",
            ShippingPostalCode = user.PostalCode ?? "",
            ShippingCountry  = "Türkiye",
            SubTotal         = subTotal,
            TaxAmount        = taxTotal,
            TotalAmount      = subTotal + taxTotal,
            Status           = OrderStatus.Listed,
            Notes            = dto.Notes,
            PaymentMethod    = dto.PaymentMethod ?? "Manuel",
            SaleType         = dto.SaleType ?? "Veresiye"
        };

        foreach (var line in lines) order.OrderItems.Add(line);

        await _uow.Orders.AddAsync(order);
        await _uow.SaveChangesAsync();

        // Update customer balance only for credit (Veresiye)
        if (order.SaleType == "Veresiye")
        {
            user.Balance += order.TotalAmount;
            await _userManager.UpdateAsync(user);
        }

        return order;
    }

    private static string GenerateOrderNumber() =>
        $"{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
}
