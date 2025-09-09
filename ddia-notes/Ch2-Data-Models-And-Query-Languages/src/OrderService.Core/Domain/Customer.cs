namespace OrderService.Core.Domain;

public record Customer(
    Guid Id, 
    string Email, 
    string Name);
