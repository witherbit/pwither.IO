using pwither.IO.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml.Linq;

namespace pwither.IO
{
    public class Directory : IDisposable
    {
        private bool disposedValue;

        public string Path { get; private set; }
        public string Name { get; private set; }
        private List<Directory> _directories { get; set; }
        private List<File> _files { get; set; }
        public Directory[] Directories { get => _directories != null ? _directories.ToArray() : new Directory[0]; }
        public File[] Files { get => _files != null ? _files.ToArray() : new File[0]; }

        public DirectoryLoadType LoadType { get; private set; }

        public int DefaultChunkSize { get; set; }
        public Directory Parent { get; internal set; }

        public bool Exist { get; private set; }

        public Directory(string path, DirectoryLoadType type = DirectoryLoadType.LoadParentInners, int defaultChunkSize = int.MaxValue)
        {
            SetPath(path, type, defaultChunkSize);
        }

        private void Initialize(string path)
        {
            Name = null;
            var dirInfo = new System.IO.DirectoryInfo(path);
            _directories.Clear();
            _files.Clear();
            Exist = dirInfo.Exists;
            if (Exist)
            {
                Name = dirInfo.Name;
                switch (LoadType)
                {
                    case DirectoryLoadType.LoadParentInners:
                        LoadInnersDirectories(dirInfo, this, DirectoryLoadType.None);
                        LoadInnersFiles(dirInfo);
                        break;
                    case DirectoryLoadType.LoadAllInners:
                        LoadInnersDirectories(dirInfo, this, DirectoryLoadType.LoadAllInners);
                        LoadInnersFiles(dirInfo);
                        break;
                }
            }
        }

        private void LoadInnersDirectories(DirectoryInfo info, Directory parent, DirectoryLoadType type)
        {
            try
            {
                var dirs = info.GetDirectories();
                foreach (var dir in dirs)
                {
                    _directories.Add(new Directory(dir.FullName, type, DefaultChunkSize)
                    {
                        Parent = parent
                    });
                }
            }
            catch { }
        }

        private void LoadInnersFiles(DirectoryInfo info)
        {
            try
            {
                var files = info.GetFiles();
                foreach (var file in files)
                {
                    _files.Add(new File(file.FullName, DefaultChunkSize));
                }
            }
            catch { }
        }

        public void Create()
        {
            System.IO.Directory.CreateDirectory(Path);
            Initialize(Path);
        }
        public void CreateTree()
        {
            Create();
            if(_directories != null && _directories.Count > 0)
                foreach(var dir in _directories)
                    dir.CreateTree();
        }

        public void Remove()
        {
            if (Exist)
                System.IO.Directory.Delete(Path, true);
            Initialize(Path);
        }

        public void Update(DirectoryLoadType type = DirectoryLoadType.LoadParentInners, int defaultChunkSize = int.MaxValue)
        {
            LoadType = type;
            DefaultChunkSize = defaultChunkSize;
            Initialize(Path);
        }

        public void SetPath(string path, DirectoryLoadType type = DirectoryLoadType.LoadParentInners, int defaultChunkSize = int.MaxValue)
        {
            Path = path.Replace("/", @"\");
            LoadType = type;
            _directories = new List<Directory>();
            _files = new List<File>();
            DefaultChunkSize = defaultChunkSize;
            Initialize(Path);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Path = null;
                    LoadType = DirectoryLoadType.None;
                    DefaultChunkSize = default;
                    Name = null;
                    Exist = false;
                }
                if(_directories != null && _directories.Count > 0)
                {
                    foreach (var directory in _directories)
                        directory.Dispose();
                    _directories.Clear();
                    _directories = null;
                }
                if (_files != null && _files.Count > 0)
                {
                    foreach (var file in _files)
                        file.Dispose();
                    _files.Clear();
                    _files = null;
                }
                GC.Collect();
                disposedValue = true;
            }
        }

        ~Directory()
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
