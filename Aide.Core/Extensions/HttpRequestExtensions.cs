using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Aide.Core.Extensions
{
	public static class HttpRequestExtensions
	{
		public const string AuthorizationHeader = "Authorization";
		public const string TimezoneHeader = "Timezone";
		public const string TimezoneAbbrHeader = "TimezoneAbbr";
		public const string LocaleHeader = "Locale";

		public static string GetHeader(this HttpRequest request, string key)
		{
			Microsoft.Extensions.Primitives.StringValues stringValues;
			request.Headers.TryGetValue(key, out stringValues);
			return stringValues.FirstOrDefault();
		}

		public static string GetAuthorizationHeader(this HttpRequest request)
		{
			return GetHeader(request, AuthorizationHeader);
		}

		public static string GetTimezoneHeader(this HttpRequest request)
		{
			return GetHeader(request, TimezoneHeader);
		}

		public static string GetTimezoneAbbrHeader(this HttpRequest request)
		{
			return GetHeader(request, TimezoneAbbrHeader);
		}

		public static string GetLocaleHeader(this HttpRequest request)
		{
			return GetHeader(request, LocaleHeader);
		}
	}
}
