using System;
using System.Collections.Concurrent;

namespace NuFridge.Shared.NuGet.Repository
{
    public class InternalPackageRepositoryFactory : IInternalPackageRepositoryFactory
    {
        private readonly object _sync = new object();
        private readonly ConcurrentDictionary<int, IInternalPackageRepository> _packageRepositories = new ConcurrentDictionary<int, IInternalPackageRepository>();
        private Func<int, IInternalPackageRepository> CreateRepoFunc { get; }

        public InternalPackageRepositoryFactory(Func<int, IInternalPackageRepository> createRepoFunc)
        {
            CreateRepoFunc = createRepoFunc;
        }

        public IInternalPackageRepository Create(int feedId)
        {
            IInternalPackageRepository packageRepository;

            lock (_sync)
            {
                if (_packageRepositories.ContainsKey(feedId))
                {
                    packageRepository = _packageRepositories[feedId];
                }
                else
                {
                    packageRepository = CreateRepoFunc(feedId);
                    _packageRepositories.TryAdd(feedId, packageRepository);
                }
            }

            return packageRepository;
        }
    }

    public interface IInternalPackageRepositoryFactory
    {
        IInternalPackageRepository Create(int feedId);
    }
}