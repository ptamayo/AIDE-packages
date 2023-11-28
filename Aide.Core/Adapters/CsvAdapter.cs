using CsvHelper;
using CsvHelper.Configuration;
using Aide.Core.Interfaces;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System;

namespace Aide.Core.Adapters
{
    public class CsvAdapter : ICsvAdapter
    {
        private readonly IFileSystemAdapter _fsa;

        public CsvAdapter(IFileSystemAdapter fsa)
        {
            _fsa = fsa ?? throw new ArgumentNullException(nameof(fsa));
        }

        /// <summary>
        /// Export records to a CSV file.
        /// </summary>
        /// <param name="records">Generic collection that contains the records to export.</param>
        /// <param name="filename">Filename including the full path and extension.</param>
        /// <param name="overwriteFile">
        /// False (default) = If the file exists then it will append the records.
        /// True = If the file exist then it will be overwritten.
        /// </param>
        public void Write(IEnumerable<object> records, string filename, bool overwriteFile = false)
        {
            var fileMode = overwriteFile ? FileMode.Create : FileMode.Append;

            var fileExists = _fsa.FileExists(filename);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // If the file exists then DON'T write the header again.
                HasHeaderRecord = !fileExists
                // IMPORTANT: It is highly advised to explore all setting (they are interesting)
            };

            using (var stream = File.Open(filename, fileMode))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(records);
            }
        }

        /// <summary>
        /// Export records to a stream.
        /// </summary>
        /// <param name="records">Generic collection that contains the records to export.</param>
        /// <param name="addHeader">Indicate if the headers should be included.</param>
        /// <returns>Resturns a stream</returns>
        public byte[] WriteStream(IEnumerable<object> records, bool addHeader)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // If the file exists then DON'T write the header again.
                HasHeaderRecord = addHeader
                // IMPORTANT: It is highly advised to explore all setting (they are interesting)
            };

            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    using (var csv = new CsvWriter(writer, config))
                    {
                        csv.WriteRecords(records);
                    }
                }
                return stream.ToArray();
            }
        }
    }
}
