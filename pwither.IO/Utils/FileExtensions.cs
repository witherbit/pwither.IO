using pwither.IO.Packages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace pwither.IO.Utils
{
    public static class FileExtensions
    {
        public static Directory Next(this Directory directory, Directory destinationDirectory)
        {
            var path = destinationDirectory.Path;
            var loadType = directory.LoadType;
            var chunkSize = directory.DefaultChunkSize;
            return new Directory(path, loadType, chunkSize)
            {
                Parent = directory,
            };
        }
        public static Directory Next(this Directory directory, int index)
        {
            return directory.Next(directory.Directories[index]);
        }

        public static Directory Next(this Directory directory, string name)
        {
            var pNext = directory.Directories.FirstOrDefault(x => x.Path == $@"{directory.Path}\{name}");
            if (pNext != null)
                return directory.Next(pNext);
            return directory;
        }
        public static Directory Back(this Directory directory)
        {
            var parent = directory.Parent;
            if(parent == null) return directory;
            directory.Dispose();
            directory = null;
            return parent;
        }

        public static Directory MoveTo(this Directory directory, string path)
        {
            path = path.Replace("/", @"\");
            var rPath = directory.Path;
            if (path == null) return directory;
            var type = directory.LoadType;
            var fchunk = directory.DefaultChunkSize;
            System.IO.Directory.Move(rPath, path);
            directory.Dispose();
            directory = null;
            return new Directory(path, type, fchunk);
        }
        public static File MoveTo(this File file, string path)
        {
            path = path.Replace("/", @"\");
            var rPath = file.Path;
            if (path == null) return file;
            var fchunk = file.ChunkSize;
            System.IO.File.Move(rPath, path);
            file.Dispose();
            file = null;
            return new File(path, fchunk);
        }

        public static void InstallX509ToRootUser(this File file, bool removeAfter = false)
        {
            X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(new X509Certificate2(X509Certificate2.CreateFromCertFile(file.Path)));
            store.Close();
            if (removeAfter) file.Remove();
        }
        public static bool InstallX509ToRootUser(this Package package, bool removeAfter = false)
        {
            var info = package.WriteToDestinationDirectory();
            info.Directory.Update();
            var file = info.Directory.Files.FirstOrDefault(x => x.Name == package.Name);
            if (file != null && file.Exist)
            {
                file.InstallX509ToRootUser(removeAfter);
                return true;
            }
            return false;
        }

        public static void InstallX509ToRootMachine(this File file, bool removeAfter = false)
        {
            X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            store.Add(new X509Certificate2(X509Certificate2.CreateFromCertFile(file.Path)));
            store.Close();
            if (removeAfter) file.Remove();
        }
        public static bool InstallX509ToRootMachine(this Package package, bool removeAfter = false)
        {
            var info = package.WriteToDestinationDirectory();
            info.Directory.Update();
            var file = info.Directory.Files.FirstOrDefault(x => x.Name == package.Name);
            if (file != null && file.Exist)
            {
                file.InstallX509ToRootMachine(removeAfter);
                return true;
            }
            return false;
        }

        public static void InstallRegFile(this File file, bool removeAfter = false)
        {
            Process regeditProcess = Process.Start("regedit.exe", "/s \"" + file.Path + "\"");
            regeditProcess.WaitForExit();
            if(removeAfter) file.Remove();
        }
        public static bool InstallRegFile(this Package package, bool removeAfter = false)
        {
            var directory = package.WriteToDestinationDirectory();
            directory.Directory.Update();
            var file = directory.Directory.Files.FirstOrDefault(x => x.Name == package.Name);
            if (file != null && file.Exist)
            {
                file.InstallRegFile(removeAfter);
                return true;
            }
            return false;
        }
    }
}
