namespace OrderService.Core.Domain;

public record Order(
    Guid Id, 
    Guid CustomerId, 
    DateTimeOffset PlacedAt, 
    decimal Total, 
    IReadOnlyList<OrderItem> Items);
