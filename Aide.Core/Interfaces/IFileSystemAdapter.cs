using Aide.Core.Adapters;
using System.IO;

namespace Aide.Core.Interfaces
{
	public interface IFileSystemAdapter
	{
		bool DirectoryExists(string path);
		FileSystemAdapter.Result CreateDirectoryRecursively(string path);
		bool FileExists(string path);
		string GenerateUniqueFileName(string path, string fileName);
		FileSystemAdapter.Result FileReadAllBytes(string path);
		FileSystemAdapter.Result FileWriteAllBytes(string path, byte[] bytes);
		FileSystemAdapter.Result FileDelete(string path);
		FileInfo GetFileInfo(string fileName);
	}
}
