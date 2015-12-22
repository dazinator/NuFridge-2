using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuFridge.Shared.Database;
using NuFridge.Shared.Database.Services;
using NuFridge.Shared.NuGet.Repository;
using NuFridge.Shared.Scheduler.Jobs.Definitions;

namespace NuFridge.Tests.Scheduler.Jobs.Definitions
{
    public class TestRunPackageRetentionPoliciesJob : RunPackageRetentionPoliciesJob
    {
        public TestRunPackageRetentionPoliciesJob(IStore store, IInternalPackageRepositoryFactory packageRepositoryFactory, IFeedService feedService, IFeedConfigurationService feedConfigurationService, IPackageService packageService) : base(store, packageRepositoryFactory, feedService, feedConfigurationService, packageService)
        {
        }

        protected override bool IsPackageDirectoryValid(string path)
        {
            return true;
        }
    }
}