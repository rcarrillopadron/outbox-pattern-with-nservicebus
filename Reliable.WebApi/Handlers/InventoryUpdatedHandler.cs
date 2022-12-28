using NServiceBus;
using Reliable.Domain;
using Reliable.Messages.Events;

namespace Reliable.WebApi.Handlers
{
    public class InventoryUpdatedHandler : IHandleMessages<InventoryUpdated>
    {
        private readonly ILogger<InventoryUpdatedHandler> _logger;
        private readonly Inventory _inventory;

        public InventoryUpdatedHandler(ILogger<InventoryUpdatedHandler> logger, Inventory inventory)
        {
            _logger = logger;
            _inventory = inventory;
        }

        public Task Handle(InventoryUpdated message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Updated inventory in frontend from backend");
            foreach (var backendItem in message.Items)
            {
                var item = new ProductQuantity(backendItem.Key, backendItem.Value);
                _inventory.Update(item);
            }
            _logger.LogInformation("Inventory was updated in frontend from backend");
            _logger.LogInformation(_inventory.ToString());
            return Task.CompletedTask;
        }
    }
}
