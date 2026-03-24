using ToptanciECommerce.Application.DTOs.Cart;

namespace ToptanciECommerce.Application.Interfaces.Services;

public interface ICartService
{
    CartDto GetCart();
    void AddItem(CartItemDto item);
    void UpdateQuantity(int productId, int quantity);
    void RemoveItem(int productId);
    void Clear();
    int GetItemCount();
}
