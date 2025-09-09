namespace OrderService.Core.Domain;

public record OrderItem(
    string Sku, 
    string Title, 
    decimal UnitPrice, 
    int Qty);
