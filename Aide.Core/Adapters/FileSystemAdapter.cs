using Aide.Core.Interfaces;
using System;
using System.IO;
using System.Linq;

namespace Aide.Core.Adapters
{
    public class FileSystemAdapter : IFileSystemAdapter
    {

        public FileSystemAdapter()
        {
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public Result CreateDirectoryRecursively(string path)
        {
            var result = new Result
            {
                OperationCompletedSuccessfully = true
            };

            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                result.OperationCompletedSuccessfully = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public string GenerateUniqueFileName(string path, string fileName)
        {
            var extension = fileName.Split('.').Last();
            var filename = fileName.Replace($".{extension}", "");
            var newFilename = $"{filename}.{extension}";

            var consecutive = 0;
            while (true)
            {
                var filePath = Path.Combine(path, newFilename);
                if (!FileExists(filePath)) break;
                newFilename = $"{filename}({++consecutive}).{extension}";
            }

            return newFilename;
        }

        public Result FileReadAllBytes(string path)
        {
            var result = new Result
            {
                OperationCompletedSuccessfully = true
            };

            try
            {
                result.Value = File.ReadAllBytes(path);
            }
            catch (Exception ex)
            {
                result.OperationCompletedSuccessfully = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public Result FileWriteAllBytes(string path, byte[] bytes)
        {
            var result = new Result
            {
                OperationCompletedSuccessfully = true
            };

            try
            {
                File.WriteAllBytes(path, bytes);
            }
            catch (Exception ex)
            {
                result.OperationCompletedSuccessfully = false;
                result.Message = ex.Message;
            }

            return result;
        }

        public Result FileDelete(string path)
        {
            var result = new Result
            {
                OperationCompletedSuccessfully = true
            };

            try
            {
                File.Delete(path);
            }
            catch (Exception ex)
            {
                result.OperationCompletedSuccessfully = false;
                result.Message = ex.Message;
            }
            return result;
        }

        public FileInfo GetFileInfo(string fileName)
        {
            return new FileInfo(fileName);
        }

        public class Result
        {
            public bool OperationCompletedSuccessfully { get; set; }
            public string Message { get; set; }
            public object Value { get; set; }
        }
    }
}
