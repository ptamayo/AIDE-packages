using Aide.Core.Domain;
using Aide.Core.Extensions;
using Aide.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Aide.Core.Adapters
{
    public class ZipArchiveAdapter : IZipArchiveAdapter, IDisposable
    {
        private readonly IFileSystemAdapter _fsa;
        public const string WhiteSpaceSeparatorChar = "_";
        public const string NewCharForSpecialChars = "_";
        private bool _disposed = false;

        public ZipArchiveAdapter(IFileSystemAdapter fsa)
        {
            _fsa = fsa ?? throw new ArgumentNullException(nameof(_fsa));
        }

        public void ZipHostedFileList(List<HostedFile> files, string outputFileName)
        {
            if (files == null) throw new ArgumentNullException(nameof(files));
            if (!files.Any()) throw new ArgumentNullException(nameof(files));
            if (string.IsNullOrWhiteSpace(outputFileName)) throw new ArgumentNullException(nameof(outputFileName));

            var ms = new MemoryStream();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var file in files)
                {
                    var fileInfo = _fsa.GetFileInfo(file.Filename);
                    var fileExtension = fileInfo.Extension;
                    var newFilename = file.DocumentName
                        .ReplaceAllSpecialChars(NewCharForSpecialChars)
                        .CleanDoubleWhiteSpaces()
                        .Trim()
                        .Trim(NewCharForSpecialChars[0])
                        .Replace(" ", WhiteSpaceSeparatorChar);
                    newFilename = $"{newFilename}{fileExtension}";
                    var entry = archive.CreateEntry(newFilename, CompressionLevel.Fastest);
                    using (var stream = entry.Open())
                    {
                        using (var fileStream = File.OpenRead(file.Filename))
                        {
                            fileStream.CopyTo(stream);
                        }
                    }
                }
            }// disposal of archive will force data to be written to memory stream.
             //reset memory stream position.
            ms.Position = 0;

            using (var fileStream = new FileStream(outputFileName, FileMode.Create))
            {
                ms.Seek(0, SeekOrigin.Begin);
                ms.CopyTo(fileStream);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //if (_wc != null)
                    //{
                    //	_wc.Dispose();
                    //}
                }
                // Release unmanaged resources.
                // Set large fields to null.                
                _disposed = true;
            }
        }

        ~ZipArchiveAdapter()
        {
            Dispose(false);
        }
    }
}
