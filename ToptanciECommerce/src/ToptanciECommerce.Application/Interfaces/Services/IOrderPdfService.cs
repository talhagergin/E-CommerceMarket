using ToptanciECommerce.Application.DTOs.Cart;

namespace ToptanciECommerce.Application.Interfaces.Services;

public interface IOrderPdfService
{
    byte[] GenerateOrderList(CartDto cart, string? customerName, string? companyName);
}
