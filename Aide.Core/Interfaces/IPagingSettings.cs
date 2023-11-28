namespace Aide.Core.Interfaces
{
	public interface IPagingSettings
	{
		int PageNumber { get; set; }
		int PageSize { get; set; }
        string[] SortBy { get; set; }
	}
}
