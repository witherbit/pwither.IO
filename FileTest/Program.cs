using pwither.IO.Packages;
using pwither.IO;
using File = pwither.IO.File;
using Directory = pwither.IO.Directory;
using pwither.IO.Utils;

namespace FileTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WritePackage();
            //ReadPackage();
            Console.WriteLine("OK");
        }

        static void WritePackage()
        {
            Packager pkg = new Packager(new PackagerInfo
            {
                Name = "test",
                Additional = "Witherbit",
                AppName = "Witherbit",
                Version = "1.0.0",
            });
            var file = new File(@"C:\Users\Tanukii\Downloads\cs2-1.0.0.3.zip", 1024);
            var dir = new Directory($@"{PackageConsts.PathToRoaming}\{PackageConsts.AppName}");
            pkg.Packages.Add(new Package(file, dir));
            pkg.Write(@"C:\Witherbit\ziptest.xsap");
            var pkg2 = Packager.Read(@"C:\Witherbit\ziptest.xsap");
            pkg2.Packages[0].WriteToDestinationDirectory();
        }
    }
}
