using System;
using System.Net.Http;
using System.Text;

namespace Aide.Core.CustomExceptions
{
	public class ExceptionMessage
	{
		protected ExceptionMessage() { } // Just in case in the future we need a subclass

		public static string ExtractMessageFromInnerException(Exception innerException)
		{
			var exceptionMessage = new StringBuilder();
			while (innerException != null)
			{
				if (!string.IsNullOrEmpty(exceptionMessage.ToString()))
				{
					if (!exceptionMessage.ToString().EndsWith(".")) exceptionMessage.Append(".");
					exceptionMessage.Append(" ");
				}
				exceptionMessage.Append(innerException.Message);
				innerException = innerException.InnerException;
			}
			return exceptionMessage.ToString();
		}

		public static bool HasHttpRequestException(Exception innerException)
		{
			while (innerException != null)
			{
				var etype = innerException.GetType();
				if (etype == typeof(HttpRequestException))
				{
					return true;
				}

				innerException = innerException.InnerException;
			}
			return false;
		}
	}
}
