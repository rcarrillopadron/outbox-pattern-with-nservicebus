using System.Collections;
using Bogus;

namespace Reliable.Domain;

public class Inventory : IEnumerable<ProductQuantity>
{
    private readonly List<ProductQuantity> _list = CreateInventory().ToList();
    
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

    private static IEnumerable<ProductQuantity> CreateInventory()
    {
        int id = 1;
        return new Faker<ProductQuantity>()
            .CustomInstantiator(f => new ProductQuantity(id++, f.Random.Int(20, 50).OrDefault(f)))
            .Generate(10);
    }

    public IEnumerator<ProductQuantity> GetEnumerator() => _list.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
}