using System.Collections.Generic;

namespace Aide.Core.Interfaces
{
	public interface IPagedResult<T> where T : class
	{
		IEnumerable<T> Results { get; set; }
		int CurrentPage { get; set; }
		double PageCount { get; set; }
		int PageSize { get; set; }
		double RowCount { get; set; }
	}
}
