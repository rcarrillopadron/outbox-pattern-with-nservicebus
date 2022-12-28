using Microsoft.Extensions.Hosting;
using NServiceBus;

namespace Reliable.Core.DI
{
    public static class NServiceBusExtension
    {
        public static IHostBuilder AddInventoryEndPoint(this IHostBuilder hostBuilder) =>
            hostBuilder.UseNServiceBus(ctx =>
            {
                var endpointConfiguration = new EndpointConfiguration("Inventory");

                // TODO: ensure the most appropriate serializer is chosen
                // https://docs.particular.net/nservicebus/serialization/
                endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();
                endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

                endpointConfiguration.Conventions()
                    .DefiningCommandsAs(t => t.Namespace != null && t.Namespace.EndsWith("Commands"))
                    .DefiningEventsAs(t => t.Namespace != null && t.Namespace.EndsWith("Events"));

                // TODO: choose a durable transport for production
                // https://docs.particular.net/transports/
                var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
                transport.UseConventionalRoutingTopology(QueueType.Quorum);
                transport.ConnectionString("host=localhost");

                // TODO: choose a durable persistence for production
                // https://docs.particular.net/persistence/
                endpointConfiguration.UsePersistence<LearningPersistence>();

                // TODO: create a script for deployment to production
                endpointConfiguration.EnableInstallers();

                // TODO: replace the license.xml file with your license file
                return endpointConfiguration;
            });

        private static async Task OnCriticalError(ICriticalErrorContext context, CancellationToken cancellationToken)
        {
            // TODO: decide if stopping the endpoint and exiting the process is the best response to a critical error
            // https://docs.particular.net/nservicebus/hosting/critical-errors
            try
            {
                await context.Stop(cancellationToken);
            }
            finally
            {
                FailFast($"Critical error, shutting down: {context.Error}", context.Exception);
            }
        }

        private static void FailFast(string message, Exception exception)
        {
            try
            {
                // TODO: decide what kind of last resort logging is necessary
                // TODO: when using an external logging framework it is important to flush any pending entries prior to calling FailFast
                // https://docs.particular.net/nservicebus/hosting/critical-errors#when-to-override-the-default-critical-error-action
            }
            finally
            {
                Environment.FailFast(message, exception);
            }
        }
    }
}