using System;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace NuFridge.Shared.Server.NuGet.FastZipPackage
{
    public abstract class FastZipPackageBase : IDisposable
    {
        private ZipFile _zipFile;

        public abstract Stream GetStream();

        public void Dispose()
        {
            if (_zipFile == null)
                return;
            _zipFile.Close();
            _zipFile = null;
        }

        public Stream GetZipEntryStream(string path)
        {
            if (_zipFile == null)
                _zipFile = new ZipFile(GetStream());
            ZipEntry entry = _zipFile.GetEntry(path.Replace('\\', '/'));
            if (entry == null)
                throw new ArgumentException("Zip file is invalid.", "path");
            return _zipFile.GetInputStream(entry);
        }

        public static DateTimeOffset GetPackageCreatedDateTime(Stream stream)
        {
            return new ZipFile(stream).Cast<ZipEntry>().Where(f => f.Name.EndsWith(".nuspec")).Select(f => f.DateTime).FirstOrDefault();
        }
    }
}
