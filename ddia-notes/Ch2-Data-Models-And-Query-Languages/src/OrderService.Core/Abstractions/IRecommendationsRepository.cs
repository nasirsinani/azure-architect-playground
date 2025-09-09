namespace OrderService.Core.Abstractions
{
    public interface IRecommendationsRepository
    {
        Task<IReadOnlyList<(string Sku, long Count)>> GetAlsoBoughtAsync(string sku, int topN = 5);
    }
}
