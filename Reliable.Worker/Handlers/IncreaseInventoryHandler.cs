using NServiceBus;
using Reliable.Domain;
using Reliable.Messages.Commands;
using Reliable.Messages.Events;

namespace Reliable.Worker.Handlers
{
    internal class IncreaseInventoryHandler : IHandleMessages<IncreaseInventory>
    {
        private readonly ILogger<Worker> _logger;
        private readonly Inventory _inventory;

        public IncreaseInventoryHandler(ILogger<Worker> logger, Inventory inventory)
        {
            _logger = logger;
            _inventory = inventory;
        }

        public Task Handle(IncreaseInventory message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Increase Inventory command received [{0}] = +{1}", message.ProductId, message.Quantity);

            var item = _inventory.GetItem(message.ProductId);
            if (item is not null)
            {
                item.Quantity += message.Quantity;
                _inventory.Update(item);

                var @event = new InventoryUpdated
                {
                    Items = _inventory.ToDictionary(x => x.ProductId, x => x.Quantity)
                };
                return context.Publish(@event);
            }

            _logger.LogWarning("ProductId {0} does not exist, therefore the inventory was not updated", message.ProductId);
            return Task.CompletedTask;
        }
    }
}
