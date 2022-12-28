namespace Reliable.Messages.Commands;

public class IncreaseInventory
{
    public IncreaseInventory(int productId, int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        ProductId = productId;
        Quantity = quantity;
    }

    public int ProductId { get; set; }
    public int Quantity { get; set; }
}