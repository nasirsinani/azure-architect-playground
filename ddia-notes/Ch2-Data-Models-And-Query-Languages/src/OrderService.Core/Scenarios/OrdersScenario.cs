using OrderService.Core.Abstractions;
using OrderService.Core.Dtos;

namespace OrderService.Core.Scenarios
{
    public class OrdersScenario(
        IOrderRepository orderRepository,
        IRecommendationsRepository recommendationsRepository
        ) : IOrdersScenario
    {
        public async Task<OrdersByCustomerResponse> GetOrdersByCustomerLast30dAsync(Guid customerId)
        {
            var from = DateTimeOffset.UtcNow.AddDays(-30);
            var list = await orderRepository.GetOrdersByCustomerIdAsync(from, customerId);

            return new(customerId, list.Count, list.Sum(o => o.Total));
        }

        public async Task<AlsoBoughtResponse> GetAlsoBoughtAsync(string sku, int topN)
        {
            var rows = await recommendationsRepository.GetAlsoBoughtAsync(sku, topN);
            return new(sku, rows.Select(r => new AlsoBoughtItem(r.Sku, r.Count)).ToList());
        }
    }
}
