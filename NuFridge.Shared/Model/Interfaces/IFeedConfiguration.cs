using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Shared.Model.Interfaces
{
    public interface IFeedConfiguration
    {
        int Id { get; }

        int FeedId { get; }

        string PackagesDirectory { get; }

        bool RetentionPolicyEnabled { get; }

        int MaxPrereleasePackages { get; }
        int MaxReleasePackages { get; }
    }
}