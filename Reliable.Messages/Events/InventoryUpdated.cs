namespace Reliable.Messages.Events;

public class InventoryUpdated
{
    public Dictionary<int, int> Items { get; set; } = new Dictionary<int, int>();
}