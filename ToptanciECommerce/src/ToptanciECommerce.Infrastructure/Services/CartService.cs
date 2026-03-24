using System.Text.Json;
using Microsoft.AspNetCore.Http;
using ToptanciECommerce.Application.DTOs.Cart;
using ToptanciECommerce.Application.Interfaces.Services;

namespace ToptanciECommerce.Infrastructure.Services;

public class CartService : ICartService
{
    private const string CartKey = "Cart";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession Session => _httpContextAccessor.HttpContext!.Session;

    public CartDto GetCart()
    {
        var json = Session.GetString(CartKey);
        if (string.IsNullOrEmpty(json)) return new CartDto();
        return JsonSerializer.Deserialize<CartDto>(json) ?? new CartDto();
    }

    public void AddItem(CartItemDto item)
    {
        var cart = GetCart();
        var existing = cart.Items.FirstOrDefault(x => x.ProductId == item.ProductId);
        if (existing != null)
        {
            existing.Quantity += item.Quantity;
        }
        else
        {
            cart.Items.Add(item);
        }
        Save(cart);
    }

    public void UpdateQuantity(int productId, int quantity)
    {
        var cart = GetCart();
        var item = cart.Items.FirstOrDefault(x => x.ProductId == productId);
        if (item == null) return;

        if (quantity <= 0)
            cart.Items.Remove(item);
        else
            item.Quantity = quantity;

        Save(cart);
    }

    public void RemoveItem(int productId)
    {
        var cart = GetCart();
        cart.Items.RemoveAll(x => x.ProductId == productId);
        Save(cart);
    }

    public void Clear()
    {
        Session.Remove(CartKey);
    }

    public int GetItemCount() => GetCart().TotalQuantity;

    private void Save(CartDto cart) =>
        Session.SetString(CartKey, JsonSerializer.Serialize(cart));
}
