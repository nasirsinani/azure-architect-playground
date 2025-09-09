using OrderService.Core.Dtos;

namespace OrderService.Core.Abstractions
{
    public interface IOrdersScenario
    {
        Task<OrdersByCustomerResponse> GetOrdersByCustomerLast30dAsync(Guid customerId);
        Task<AlsoBoughtResponse> GetAlsoBoughtAsync(string sku, int topN);
    }
}
