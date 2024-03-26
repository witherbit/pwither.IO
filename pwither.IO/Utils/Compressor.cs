
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using pwither.formatter;
using pwither.IO.Packages;

namespace pwither.IO.Utils
{
    internal static class Compressor
    {
        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }
        public static void ZipToFile(byte[] bytes, string path)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new DeflateStream(mso, CompressionLevel.SmallestSize))
                {
                    CopyTo(msi, gs);
                }

                var arr = mso.ToArray();
                using (var fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    fs.Write(arr, 0, arr.Length);
                }
            }
        }
        public static byte[] UnzipFromFile(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open))
            {
                byte[] array = new byte[fs.Length];
                fs.Read(array, 0, array.Length);
                using (var msi = new MemoryStream(array))
                using (var mso = new MemoryStream())
                {
                    using (var gs = new DeflateStream(msi, CompressionMode.Decompress))
                    {
                        CopyTo(gs, mso);
                    }

                    return mso.ToArray();
                }
            }
        }
        public static byte[] Zip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new DeflateStream(mso, CompressionLevel.SmallestSize))
                {
                    CopyTo(msi, gs);
                }

                var arr = mso.ToArray();
                return arr;
            }
        }
        public static byte[] Unzip(byte[] array)
        {
            using (var msi = new MemoryStream(array))
            using (var mso = new MemoryStream())
            {
                using (var gs = new DeflateStream(msi, CompressionMode.Decompress))
                {
                    CopyTo(gs, mso);
                }

                return mso.ToArray();
            }
        }

        public static byte[] PackagerToByteArray(this Packager obj)
        {
            using (var ms = new MemoryStream())
            {
                var bf = new BitBinaryFormatter();
                bf.SurrogateSelector = new ConverterSelector();
                bf.Control.IsSerializableHandlers = new IsSerializableHandlers();
                bf.Control.IsSerializableHandlers.Handlers.OfType<SerializeAllowedTypes>().Single().AllowedTypes.Add(typeof(object));
                var b = new AllowedTypesBinder();
                b.AddAllowedType(typeof(Packager));
                b.AddAllowedType(typeof(PackagerInfo));
                b.AddAllowedType(typeof(Package));
                b.AddAllowedType(typeof(FilePart));
                bf.Binder = b;
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static Packager ByteArrayToPackager(this byte[] arrBytes)
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(arrBytes, 0, arrBytes.Length);
                ms.Seek(0, SeekOrigin.Begin);
                var bf = new BitBinaryFormatter();
                bf.SurrogateSelector = new ConverterSelector();
                bf.Control.IsSerializableHandlers = new IsSerializableHandlers();
                bf.Control.IsSerializableHandlers.Handlers.OfType<SerializeAllowedTypes>().Single().AllowedTypes.Add(typeof(object));
                var b = new AllowedTypesBinder();
                b.AddAllowedType(typeof(Packager));
                b.AddAllowedType(typeof(PackagerInfo));
                b.AddAllowedType(typeof(Package));
                b.AddAllowedType(typeof(FilePart));
                bf.Binder = b;
                return (Packager)bf.Deserialize(ms);
            }
        }
    }
}
