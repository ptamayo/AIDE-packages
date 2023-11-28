using Aide.Core.Domain;
using System.Collections.Generic;

namespace Aide.Core.Interfaces
{
	public interface IZipArchiveAdapter
	{
		void ZipHostedFileList(List<HostedFile> files, string outputFileName);
	}
}
