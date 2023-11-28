using System.Collections.Generic;
using Aide.Core.Interfaces;

namespace Aide.Core.Data
{
	public class PagedResult<T> : IPagedResult<T> where T : class
	{
		public IEnumerable<T> Results { get; set; }
		public int CurrentPage { get; set; }
		public double PageCount { get; set; }
		public int PageSize { get; set; }
		public double RowCount { get; set; }
	}
}
