using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Security;

namespace NuFridge.Shared.Server.Security
{
    public class Claims
    {
        public const string SystemAdministrator = "SystemAdministrator";
        public const string CanInsertFeed = "CanInsertFeed";
        public const string CanUpdateFeed = "CanUpdateFeed";
        public const string CanViewUsers = "CanViewUsers";
        public const string CanDeleteFeed = "CanDeleteFeed";
        public const string CanViewFeeds = "CanViewFeeds";
        public const string CanViewDashboard = "CanViewDashboard";
        public const string CanViewFeedHistory = "CanViewFeedHistory";
        public const string CanUploadPackages = "CanUploadPackages";
        public const string CanReindexPackages = "CanReindexPackages";
        public const string CanDeletePackages = "CanDeletePackages";
        public const string CanDownloadPackages = "CanDownloadPackages";
        public const string CanViewPackages = "CanViewPackages";
        public const string CanExecuteRetentionPolicy = "CanExecuteRetentionPolicy";
        public const string CanChangePackageDirectory = "CanChangePackageDirectory";
    }
}