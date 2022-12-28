namespace Reliable.Domain;

public record ProductQuantity(int ProductId, int Quantity)
{
    public int ProductId { get; } = ProductId;
    public int Quantity { get; set; } = Quantity;
}