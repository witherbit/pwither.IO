using pwither.IO.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace pwither.IO
{
    public sealed class File : IDisposable
    {
        private bool disposedValue;
        public string Path {  get; private set; }

        public string Name { get; private set; }

        public string Extension { get; private set; }

        public FilePart[] Parts { get; private set; }

        public int Count => Parts != null ? Parts.Length : 0;

        public long Size { get; private set; }

        public int ChunkSize { get; private set; }

        public bool Exist { get; private set; }

        public File(string path, int chunkSize = int.MaxValue)
        {
            Path = path.Replace("/", @"\");
            ChunkSize = chunkSize;
            Initialize(Path);
        }

        public FilePart[] Read()
        {
            var result = new List<FilePart>();
            if(Size <= ChunkSize)
            {
                result.Add(new FilePart
                {
                    Part = System.IO.File.ReadAllBytes(Path),
                    Size = Size,
                    Index = 0,
                });
            }
            else
            {
                long totalRead = 0;
                int index = 0;
                using (var fs = new FileStream(Path, FileMode.Open))
                {
                    while (totalRead < Size)
                    {
                        var part = new byte[Size - totalRead < ChunkSize ? Size - totalRead : ChunkSize];

                        totalRead += fs.Read(part, 0, part.Length);

                        result.Add(new FilePart
                        {
                            Part = part,
                            Size = part.Length,
                            Index = index++,
                        });
                    }
                }
            }

            Parts = result.ToArray();
            return Parts;
        }
        public FilePart ReadPart(int partIndex)
        {
            var c = GetPartsCount();
            if (c <= partIndex)
                throw new ArgumentException($"The number of parts is less than the index of the part - part counts is {c}, but index is {partIndex}");
            using (var fs = new FileStream(Path, FileMode.Open))
            {
                var part = new byte[GetPartsSizes()[partIndex]];
                fs.Position = partIndex * ChunkSize;
                fs.Read(part, 0, part.Length);
                return new FilePart
                {
                    Part = part,
                    Size = part.Length,
                    Index = partIndex,
                };
            }
        }

        public int GetPartsCount()
        {
            if (!Exist) return 0;
            if (Size <= ChunkSize)
                return 1;
            else
                return (int)(Size / ChunkSize) + (Size % ChunkSize == 0 ? 0 : 1);
        }
        public int[] GetPartsSizes()
        {
            if (!Exist) return Array.Empty<int>();
            List<int> sizes = new List<int>();
            if (Size <= ChunkSize)
            {
                sizes.Add((int)Size);
            }
            else
            {
                bool lastNotFull = Size % ChunkSize != 0;
                var count = (Size / ChunkSize); //+ (lastNotFull ? 1 : 0);
                for (var i = 0; i < count; i++)
                {
                    sizes.Add(ChunkSize);
                }
                if(lastNotFull)
                    sizes.Add((int)(Size - (ChunkSize * count)));
            }
            return sizes.ToArray();
        }

        public void Write(string destinationPath)
        {
            Initialize(destinationPath);
            if (Exist) Remove(destinationPath);
            using (var fs = new FileStream(destinationPath, FileMode.OpenOrCreate))
            {
                foreach (var part in Parts)
                    fs.Write(part.Part, 0, part.Part.Length);
            }
            Initialize(destinationPath);
        }
        public void Write()
        {
            Write(Path);
        }
        public Directory WriteToDirectory(Directory directory)
        {
            directory.Create();
            Write($"{directory.Path}\\{Name}");
            directory.Update(directory.LoadType, directory.DefaultChunkSize);
            return directory;
        }
        public Directory WriteToDirectory(string directory)
        {
            Write($"{directory}\\{Name}");
            return new Directory($"{directory}");
        }

        public void WritePart(string destinationPath, FilePart part)
        {
            Initialize(destinationPath);
            using (var fs = new FileStream(destinationPath, FileMode.Append))
            {
                fs.Write(part.Part, 0, part.Part.Length);
            }
            Initialize(destinationPath);
        }
        public void WritePart(FilePart part)
        {
            WritePart(Path, part);
        }
        public Directory WritePartToDirectory(string directory, FilePart part)
        {
            WritePart($"{directory}\\{Name}", part);
            return new Directory($"{directory}");
        }
        public Directory WritePartToDirectory(Directory directory, FilePart part)
        {
            directory.Create();
            WritePart($"{directory.Path}\\{Name}", part);
            directory.Update(directory.LoadType, directory.DefaultChunkSize);
            return directory;
        }

        public void Remove(string destinationPath)
        {
            Initialize(destinationPath);
            System.IO.File.Delete(destinationPath);
            Initialize(destinationPath);
        }
        public void Remove()
        {
            Remove(Path);
        }

        private void Initialize(string path)
        {
            var fileInfo = new System.IO.FileInfo(path);
            Exist = fileInfo.Exists;
            if (Exist)
            {
                Size = fileInfo.Length;
                Name = fileInfo.Name;
                Extension = fileInfo.Extension;
            }
        }

        public static File FromFileParts(string path, IEnumerable<FilePart> parts, int chunkSize = int.MaxValue)
        {
            var result = new File(path, chunkSize);
            result.Parts = parts.ToArray();
            return result;
        }
        public static File Create(string path, int chunkSize = int.MaxValue)
        {
            if (!System.IO.File.Exists(path))
                System.IO.File.Create(path);
            return new File(path, chunkSize);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Path = null;
                    Name = null;
                    Extension = null;
                    Size = default;
                    ChunkSize = default;
                }

                if(Parts != null && Parts.Length > 0)
                {
                    foreach (var part in Parts)
                        part.Dispose();
                    Parts = null;
                }
                GC.Collect();
                disposedValue = true;
            }
        }

        ~File()
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
