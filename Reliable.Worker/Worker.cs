using NServiceBus;
using Reliable.Domain;
using Reliable.Messages.Events;

namespace Reliable.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly Inventory _inventory;
        private readonly IMessageSession _messageSession;

        public Worker(ILogger<Worker> logger, Inventory inventory, IMessageSession messageSession)
        {
            _logger = logger;
            _inventory = inventory;
            _messageSession = messageSession;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(8), cancellationToken);
                var @event = new InventoryUpdated
                {
                    Items = _inventory.ToDictionary(x => x.ProductId, x => x.Quantity)
                };
                await _messageSession.Publish(@event, cancellationToken);
            }, cancellationToken);


            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Current inventory{Environment.NewLine}{_inventory}");
                await Task.Delay(4000, cancellationToken);
            }
        }
    }
} 