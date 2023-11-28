using Aide.Core.Interfaces;

namespace Aide.Core.Data
{
	public class PagingSettings : IPagingSettings
	{
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public string[] SortBy { get; set; }
	}
}
