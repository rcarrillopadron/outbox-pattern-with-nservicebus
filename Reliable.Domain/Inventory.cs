using System.Collections;
using BetterConsoleTables;
using Bogus;

namespace Reliable.Domain;

public class Inventory : IEnumerable<ProductQuantity>
{
    private readonly List<ProductQuantity> _list = new();
    
    public void Update(ProductQuantity item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        ProductQuantity? currentItem = this.SingleOrDefault(x => x.ProductId == item.ProductId);

        if (currentItem != null)
            currentItem.Quantity = item.Quantity;
        else
            _list.Add(item);
    }

    public ProductQuantity? GetItem(int productId) => _list.Find(x => x.ProductId == productId);

    public void GenerateRandomItems(int count = 10)
    {
        int id = 1;
        var items = new Faker<ProductQuantity>()
            .CustomInstantiator(f => new ProductQuantity(id++, f.Random.Int(20, 50).OrDefault(f)))
            .Generate(count);
        _list.AddRange(items);
    }
    

    public IEnumerator<ProductQuantity> GetEnumerator() => _list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

    public override string ToString()
    {
        var table = new Table("Product Id", "Quantity");
        _list.ForEach(item => table.AddRow(item.ProductId, item.Quantity));
        return table.ToString();
    }
}