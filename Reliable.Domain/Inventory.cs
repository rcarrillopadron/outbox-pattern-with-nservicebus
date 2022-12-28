using System.Collections;
using System.Text;
using Bogus;

namespace Reliable.Domain;

public class Inventory : IEnumerable<ProductQuantity>
{
    private readonly SortedSet<ProductQuantity> _sortedSet = new(new InventoryComparer()) ;
    
    public void Update(ProductQuantity item)
    {
        
        if (item == null) throw new ArgumentNullException(nameof(item));
        ProductQuantity? currentItem = this.SingleOrDefault(x => x.ProductId == item.ProductId);

        if (currentItem != null)
            currentItem.Quantity = item.Quantity;
        else
            _sortedSet.Add(item);
    }

    public ProductQuantity? GetItem(int productId) => _sortedSet.SingleOrDefault(x => x.ProductId == productId);

    public void GenerateRandomItems(int count = 10)
    {
        int id = 1;
        var items = new Faker<ProductQuantity>()
            .CustomInstantiator(f => new ProductQuantity(id++, f.Random.Int(20, 50).OrDefault(f)))
            .Generate(count);
        items.ForEach(item => _sortedSet.Add(item));
    }
    

    public IEnumerator<ProductQuantity> GetEnumerator() => _sortedSet.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _sortedSet.GetEnumerator();

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("| Product Id | Quantity |");
        _sortedSet.ToList().ForEach(item => sb.AppendLine($"| {item.ProductId,10:D} | {item.Quantity,8:D} |"));
        sb.AppendLine();
        return sb.ToString();
    }
}

internal class InventoryComparer : IComparer<ProductQuantity>
{
    public int Compare(ProductQuantity x, ProductQuantity y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(null, y)) return 1;
        if (ReferenceEquals(null, x)) return -1;
        var productIdComparison = x.ProductId.CompareTo(y.ProductId);
        if (productIdComparison != 0) return productIdComparison;
        return x.Quantity.CompareTo(y.Quantity);
    }
}