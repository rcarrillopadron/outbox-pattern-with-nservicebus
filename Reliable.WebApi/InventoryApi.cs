using NServiceBus;
using Reliable.Domain;
using Reliable.Messages.Commands;

namespace Reliable.WebApi;

public class InventoryApi
{
    private readonly ILogger<InventoryApi> _logger;
    private readonly IMessageSession _messageSession;
    private readonly Inventory _inventory;

    public InventoryApi(ILogger<InventoryApi> logger, IMessageSession messageSession, Inventory inventory)
    {
        _logger = logger;
        _messageSession = messageSession;
        _inventory = inventory;
    }

    public void Register(WebApplication app)
    {
        app.MapGet("/inventory/{productId}", GetByProductId);
        app.MapGet("/inventory", GetAll);
        app.MapPut("/inventory", Put);
    }

    private Task<IResult> GetByProductId(int productId)
    {
        _logger.LogInformation("Getting ProductId {0}", productId);
        var item = _inventory.GetItem(productId);
        if (item is not null)
        {
            _logger.LogInformation("{0}", item);
            return Task.FromResult(Results.Ok(item));
        }

        return Task.FromResult(Results.NotFound());
    }

    private Task<IResult> GetAll()
    {
        using (_logger.BeginScope(nameof(GetAll)))
        {
            _logger.LogInformation("Getting all products");
            var items = _inventory.ToList();
            _logger.LogInformation("items: {@items}", items);
            return Task.FromResult(Results.Ok(items));
        }
    }

    private async Task<IResult> Put(ProductQuantity item, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating productId {0} with {1} items", item.ProductId, item.Quantity);
        var currentItem = _inventory.GetItem(item.ProductId);
        if (currentItem is not null)
        {
            if (currentItem.Quantity<item.Quantity)
            {
                int quantity = item.Quantity - currentItem.Quantity;
                _logger.LogInformation("Sending command to increase quantity by {0}", quantity);
                await _messageSession.SendLocal(new IncreaseInventory(item.ProductId, quantity), cancellationToken);
            }
            else if (currentItem.Quantity > item.Quantity)
            {
                int quantity = currentItem.Quantity - item.Quantity;
                _logger.LogInformation("Sending command to decrease quantity by {0}", quantity);
                await _messageSession.SendLocal(new DecreaseInventory(item.ProductId, quantity), cancellationToken);
            }
            else
            {
                _logger.LogInformation("There was no change in quantities");
                return Results.Conflict($"Inventory was not incremented or decremented for {item.ProductId}");
            }
        }

        _inventory.Update(item);
        return Results.Accepted($"/inventory/{item.ProductId}", item);
    }
}