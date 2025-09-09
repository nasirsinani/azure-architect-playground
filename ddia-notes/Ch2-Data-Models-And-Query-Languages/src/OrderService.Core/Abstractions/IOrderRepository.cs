using OrderService.Core.Domain;

namespace OrderService.Core.Abstractions
{
    public interface IOrderRepository
    {
        Task<IReadOnlyList<Order>> GetOrdersByCustomerIdAsync(DateTimeOffset from, Guid customerId);
    }
}
