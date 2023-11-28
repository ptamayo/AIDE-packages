using Azure.Messaging.ServiceBus;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aide.Core.Cloud.Azure.ServiceBus
{
	public class BusService : IBusService, IDisposable
	{
		private readonly ILogger<BusService> _logger;
		private readonly IBusControl _bus;
		private bool _disposed = false;

        /// <summary>
        /// Create an Azure Service Bus client.
        /// </summary>
        /// <param name="logger">logger</param>
        /// <param name="connectionString">connectionString</param>
        /// <param name="transportType">
        /// AmqpWebSockets = To bypass blocked TCP port 5671 and instead use port 443. This is recommended if you have problems in a shared hosting or a firewall in general.
        /// Amqp = Use port TCP 5671.
        /// </param>
        /// <param name="receiveEndpoint">
        ///	A dictionary of ReceiveEndpoint.
        ///	For a sender process this parameter is optional.
        ///	For a subscriber this is required.
        ///	</param>
        public BusService(ILogger<BusService> logger, string connectionString, ServiceBusTransportType transportType, IDictionary<string, Type> receiveEndpoint = null)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));

			Console.WriteLine("+ {0} was created", this.GetType().Name);
			_logger.LogInformation($"{this.GetType().Name} was created");

			_bus = Bus.Factory.CreateUsingAzureServiceBus(cfg =>
			{
				cfg.Host(connectionString, h =>
				{
					h.TransportType = transportType; // THIS IS A VERY IMPORTANT PARAMETER!
				});
				// Configure ReceiveEndpoints if any
				if (receiveEndpoint != null)
				{
					foreach (var key in receiveEndpoint.Keys)
					{
						cfg.ReceiveEndpoint(key, e =>
						{
							e.Consumer(receiveEndpoint[key], type => Activator.CreateInstance(type));
						});
					}
				}
			});

			_bus.Start();

			_logger.LogInformation($"{this.GetType().Name} started");
		}

		/// <summary>
		/// Get the endpoint that will be used to send messages.
		/// </summary>
		/// <param name="endpoint">URL</param>
		/// <returns>ISendEndpoint</returns>
		public async Task<ISendEndpoint> GetSendEndpoint(string endpoint)
		{
			return await _bus.GetSendEndpoint(new Uri(endpoint)).ConfigureAwait(false);
		}

		/// <summary>
		/// This is a very important method because the service bus connection must be closed upon disposal
		/// </summary>
		public void Dispose() // Implement IDisposable
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_bus != null)
                    {
						_bus.StopAsync().GetAwaiter().GetResult();
						_logger.LogInformation($"{this.GetType().Name} stopped");
						_logger.LogInformation($"{this.GetType().Name} was disposed!");
					}
				}
				// Release unmanaged resources.
				// Set large fields to null.                
				_disposed = true;
			}
		}

		~BusService()
        {
			Dispose(false);
        }
	}
}
