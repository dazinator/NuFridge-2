using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.OData.Formatter;
using NuFridge.Shared.Server.Configuration;
using NuFridge.Shared.Server.FileSystem;
using NuFridge.Shared.Server.Storage;
using NuGet;

namespace NuFridge.Shared.Server.NuGet
{
    public class InternalPackageRepositoryFactory : IInternalPackageRepositoryFactory
    {
        private Func<int, IInternalPackageRepository> CreateRepoFunc { get; }

        public InternalPackageRepositoryFactory(Func<int, IInternalPackageRepository> createRepoFunc)
        {
            CreateRepoFunc = createRepoFunc;
        }

        public IInternalPackageRepository Create(int feedId)
        {
            var packageRepository = CreateRepoFunc(feedId);

            return packageRepository;
        }
    }

    public interface IInternalPackageRepositoryFactory
    {
        IInternalPackageRepository Create(int feedId);
    }
}