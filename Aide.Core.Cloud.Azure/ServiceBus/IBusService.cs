using MassTransit;
using System.Threading.Tasks;

namespace Aide.Core.Cloud.Azure.ServiceBus
{
	public interface IBusService
	{
		Task<ISendEndpoint> GetSendEndpoint(string endpoint);
	}
}
