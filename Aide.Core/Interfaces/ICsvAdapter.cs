using System.Collections.Generic;

namespace Aide.Core.Interfaces
{
    public interface ICsvAdapter
    {
        void Write(IEnumerable<object> records, string filename, bool overwriteFile = false);
        byte[] WriteStream(IEnumerable<object> records, bool addHeader);
    }
}
