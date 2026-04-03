using ToptanciECommerce.Application.DTOs.Cart;
using ToptanciECommerce.Application.DTOs.Order;
using ToptanciECommerce.Domain.Entities;

namespace ToptanciECommerce.Application.Interfaces.Services;

public interface IOrderService
{
    Task<Order> CreateFromCartAsync(CartDto cart, string userId, ShippingInfoDto shipping);
    Task<Order> CreateManualAsync(ManualOrderDto dto);
}
