using System;

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