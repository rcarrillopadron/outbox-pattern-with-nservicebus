using NServiceBus.Installation;
using Reliable.Domain;

namespace Reliable.Worker.NsbInstallers
{
    internal class PopulateInventoryInstaller : INeedToInstallSomething
    {
        private readonly Inventory _inventory;

        public PopulateInventoryInstaller(Inventory inventory)
        {
            _inventory = inventory;
        }

        public Task Install(string identity, CancellationToken cancellationToken = new())
        {
            _inventory.AddRandomItems(15);
            return Task.CompletedTask;
        }
    }
}
