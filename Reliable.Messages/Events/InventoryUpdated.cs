namespace Reliable.Messages.Events;

public class InventoryUpdated
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}