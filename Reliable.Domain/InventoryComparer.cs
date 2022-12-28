namespace Reliable.Domain;

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