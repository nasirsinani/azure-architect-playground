namespace OrderService.Core.Dtos;

public record OrdersByCustomerResponse(Guid CustomerId, int Count, decimal SumTotal);
public record AlsoBoughtItem(string Sku, long Count);
public record AlsoBoughtResponse(string AnchorSku, IReadOnlyList<AlsoBoughtItem> Items);
