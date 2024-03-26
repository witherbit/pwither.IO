using pwither.formatter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace pwither.IO
{
    [BitSerializable]
    public class FilePart : IDisposable
    {
        private bool disposedValue;
        public byte[] Part { get; set; }
        public long Size {  get; set; }
        public int Index { get; set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Index = default;
                    Size = default;
                }
                Part = Array.Empty<byte>();
                Part = null;
                GC.Collect();
                disposedValue = true;
            }
        }

        ~FilePart()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
