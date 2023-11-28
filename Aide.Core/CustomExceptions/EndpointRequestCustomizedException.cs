using System;

namespace Aide.Core.CustomExceptions
{
	[Serializable]
	public class EndpointRequestCustomizedException : Exception
	{
		public EndpointRequestCustomizedException() : base() { }
		public EndpointRequestCustomizedException(string message) : base(message) { }
	}
}
