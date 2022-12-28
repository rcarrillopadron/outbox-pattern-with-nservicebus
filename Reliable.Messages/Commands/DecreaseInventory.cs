namespace Reliable.Messages.Commands;

public class DecreaseInventory 
{
    public DecreaseInventory(int productId, int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        ProductId = productId;
        Quantity = quantity;
    }

    public int ProductId { get; set; }
    public int Quantity { get; set; }
}