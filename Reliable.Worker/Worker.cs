using BetterConsoleTables;
using Reliable.Domain;

namespace Reliable.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly Inventory _inventory;


        public Worker(ILogger<Worker> logger, Inventory inventory)
        {
            _logger = logger;
            _inventory = inventory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var table = new Table(TableConfiguration.Markdown(), "Product Id", "Quantity");
                _inventory.Select(item => table.AddRow(item.ProductId, item.Quantity));
                _logger.LogTrace(table.ToString());

                await Task.Delay(10000, cancellationToken);
            }
        }
    }
} 