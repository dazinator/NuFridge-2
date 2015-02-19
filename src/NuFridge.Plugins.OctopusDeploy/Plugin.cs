using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using NuFridge.Service.Plugins.Model;
using NuFridge.Service.Plugins.Settings;
using Octopus.Client;
using Octopus.Client.Model;

namespace NuFridge.Plugins.OctopusDeploy
{
    [Export]
    public class Plugin : PackagePublishBase
    {
        private const string OctopusApiUrlId = "octoUrl";
        private const string OctopusApiKeyId = "octoApiKey";
        private const string OctopusMappingId = "octoMapping";

        private const string PackageIdColumnId = "packageName";
        private const string OctopusProjectNameColumnId = "projectName";
        
        public override void Execute(List<PackageData> packages)
        {
            var mappingGrid = GetSettingValue<GridSetting>(OctopusMappingId);

            foreach (var package in packages)
            {
                foreach (var row in mappingGrid.Rows)
                {
                    if (row.GetValue<string>(PackageIdColumnId) == package.Id)
                    {
                        ProcessMatch(row, package);
                    }
                }
            }
        }

        private void ProcessMatch(GridSetting.GridColumnRow row, PackageData package)
        {
            var url = GetSettingValue<string>(OctopusApiUrlId);
            var apiKey = GetSettingValue<string>(OctopusApiKeyId);

            var endpoint = new OctopusServerEndpoint(url, apiKey);
            var repo = new OctopusRepository(endpoint);

            var projectName = row.GetValue<string>(OctopusProjectNameColumnId);

            var project = repo.Projects.FindByName(projectName);

            if (project == null)
            {
                return;
            }

            var deploymentProcess = repo.DeploymentProcesses.Get(project.DeploymentProcessId);
            var template = repo.DeploymentProcesses.GetTemplate(deploymentProcess);

            var release = new ReleaseResource
            {
                ProjectId = project.Id, Version = template.NextVersionIncrement, ReleaseNotes = string.Empty};

            Dictionary<string, List<string>> feedVersions = GetPackageVersions(template, release);

            bool foundPackage;
            Dictionary<string, string> packageVersions;
            if (!DoesReleaseMatchNuGetFeed(package, feedVersions, repo, out foundPackage, out packageVersions) || !foundPackage) 
                return;

            SetPackageVersions(template, release, packageVersions);

            repo.Releases.Create(release);
        }

        private static void SetPackageVersions(ReleaseTemplateResource template, ReleaseResource release,
            Dictionary<string, string> packageVersions)
        {
            foreach (var nuGetPackage in template.Packages)
            {
                var selectedPackage = release.SelectedPackages.Single(sg => sg.StepName == nuGetPackage.StepName);
                selectedPackage.Version = packageVersions[nuGetPackage.NuGetPackageId];
            }
        }

        private static bool DoesReleaseMatchNuGetFeed(PackageData package, Dictionary<string, List<string>> feedVersions, OctopusRepository repo,
            out bool foundPackage, out Dictionary<string, string> packageVersions)
        {
            foundPackage = false;
            packageVersions = new Dictionary<string, string>();
            foreach (var feedVersion in feedVersions)
            {
                var feed = repo.Feeds.Get(feedVersion.Key);
                var versions = repo.Feeds.GetVersions(feed, feedVersion.Value.ToArray());

                foreach (var packageResource in versions)
                {
                    if (packageVersions.ContainsKey(packageResource.NuGetPackageId)) continue;
                    if (packageResource.NuGetPackageId == package.Id)
                    {
                        foundPackage = true;
                        if (packageResource.Version == package.Version.SemanticVersion.Version.ToString())
                        {
                            packageVersions.Add(packageResource.NuGetPackageId, packageResource.Version);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        packageVersions.Add(packageResource.NuGetPackageId, packageResource.Version);
                    }
                }
            }
            return true;
        }

        private Dictionary<string, List<string>> GetPackageVersions(ReleaseTemplateResource template, ReleaseResource release)
        {
            var feedVersions = new Dictionary<string, List<string>>();

            foreach (var nugetPackage in template.Packages)
            {
                var selectedPackage = new SelectedPackage {StepName = nugetPackage.StepName};

                if (!feedVersions.ContainsKey(nugetPackage.NuGetFeedId))
                {
                    feedVersions.Add(nugetPackage.NuGetFeedId, new List<string> {nugetPackage.NuGetPackageId});
                }
                else
                {
                    feedVersions[nugetPackage.NuGetFeedId].Add(nugetPackage.NuGetPackageId);
                }
                release.SelectedPackages.Add(selectedPackage);
            }
            return feedVersions;
        }

        private IEnumerable<GridSetting.GridColumn> GetGridColumns()
        {
            Dictionary<string, string> columns = new Dictionary<string, string>
            {
                {PackageIdColumnId, "Package ID"},
                {OctopusProjectNameColumnId, "Project Name"}
            };

            return columns.Select(column => new GridSetting.GridColumn(column.Key, column.Value));
        }

        public override void GenerateSettings()
        {
            var octopusApiUrlSetting = new TextBoxSetting(OctopusApiUrlId, "Octopus API Url");
            AddSetting(octopusApiUrlSetting);

            var octopusApiKeySetting = new TextBoxSetting(OctopusApiKeyId, "Octopus API Key");
            AddSetting(octopusApiKeySetting);

            var mappingSetting = new GridSetting(OctopusMappingId, "Octopus Mappings");
            mappingSetting.WithColumns(GetGridColumns());
            AddSetting(mappingSetting);
        }
    }
}