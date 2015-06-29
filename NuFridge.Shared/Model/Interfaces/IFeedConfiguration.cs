using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuFridge.Shared.Model.Interfaces
{
    public interface IFeedConfiguration
    {
        int Id { get; set; }

        int FeedId { get; set; }

        string PackagesDirectory { get; set; }

        bool RetentionPolicyEnabled { get; set; }

        int MaxPrereleasePackages { get; set; }
        int MaxReleasePackages { get; set; }
    }
}