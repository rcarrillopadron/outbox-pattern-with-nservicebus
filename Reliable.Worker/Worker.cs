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
                _logger.LogInformation(_inventory.ToString());
                await Task.Delay(4000, cancellationToken);
            }
        }
    }
} 