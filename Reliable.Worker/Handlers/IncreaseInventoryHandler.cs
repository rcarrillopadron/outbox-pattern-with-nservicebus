using NServiceBus;
using Reliable.Messages.Commands;

namespace Reliable.Worker.Handlers
{
    internal class IncreaseInventoryHandler : IHandleMessages<IncreaseInventory>
    {
        private readonly ILogger<Worker> _logger;

        public IncreaseInventoryHandler(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        public Task Handle(IncreaseInventory message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Increase Inventory command received [{0}] = +{1}", message.ProductId, message.Quantity);
            return Task.CompletedTask;
        }
    }


    internal class DecreaseInventoryHandler : IHandleMessages<DecreaseInventory>
    {
        private readonly ILogger<Worker> _logger;

        public DecreaseInventoryHandler(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        public Task Handle(DecreaseInventory message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Decrease Inventory command received [{0}] = -{1}", message.ProductId, message.Quantity);
            return Task.CompletedTask;
        }
    }
}
