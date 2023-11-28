using System.Text.RegularExpressions;

namespace Aide.Core.Extensions
{
	public static class StringExtensions
	{
		public static string CleanAllSpecialChars(this string x)
		{
			if (!string.IsNullOrWhiteSpace(x))
			{
				return Regex.Replace(x, @"[^0-9a-zA-ZáéíóúñÁÉÍÓÚÑ]+", "");
			}
			else
			{
				return x;
			}
		}

		public static string ReplaceAllSpecialChars(this string x, string newChar)
		{
			if (!string.IsNullOrWhiteSpace(x))
			{
				return Regex.Replace(x, @"[^0-9a-zA-ZáéíóúñÁÉÍÓÚÑ]+", newChar);
			}
			else
			{
				return x;
			}
		}

		public static string CleanDoubleWhiteSpaces(this string x)
		{
			if (!string.IsNullOrWhiteSpace(x))
			{
				var regex = new Regex("[ ]{2,}", RegexOptions.None);
				return regex.Replace(x, " ").Trim();
			}
			else
			{
				return x;
			}
		}

		public static string DecodeUTF8(this string x)
		{
			if (!string.IsNullOrWhiteSpace(x))
			{
				return System.Web.HttpUtility.UrlDecode(x);
			}
			else
			{
				return x;
			}
		}

		/// <summary>
		/// Escapes the special chars in a given string.
		/// For further details see:
		/// https://www.regular-expressions.info/characters.html
		/// https://stackoverflow.com/questions/37249894/add-prefix-to-special-characters-with-regular-expressions
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		public static string EscapeRegexSpecialChars(this string x)
		{
			if (!string.IsNullOrWhiteSpace(x))
			{
				return Regex.Replace(x, @"[\^$.|?*+()[{]", @"\$&");
			}
			else
			{
				return x;
			}
		}
	}
}
