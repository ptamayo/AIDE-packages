using System.Collections.Generic;
using System.Linq;

namespace Aide.Core.Data
{
	public class SqlQueryHelper
	{
		private const char singleQuote = '\"';

		public static string BuildWhereOr(string columnName, IEnumerable<int> keyValues)
		{
			var whereExpression = string.Empty;
			whereExpression = $"{columnName} == {string.Join($" || {columnName} == ", keyValues.Select(s => s).ToArray())}";
			return whereExpression;
		}

		public static string BuildWhereOr(string columnName, IEnumerable<string> keyValues)
		{
			var whereExpression = string.Empty;
			whereExpression = $"{columnName} == {singleQuote}{string.Join($" || {columnName} == {singleQuote}", keyValues.Select(s => s + singleQuote).ToArray())}";
			return whereExpression;
		}
	}
}
