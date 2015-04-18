using System;
using System.IO;
using System.Linq;
using WixSharp;
using WixSharp.CommonTasks;
using File = WixSharp.File;

public class Script
{
    public static void Main(string[] args)
    {
#if DEBUG
        bool isDebug = true;
#else
        bool isDebug = false;
#endif

        var rootPath = GetPathToSrcFolder();


        Feature topLevelFeature = new Feature("NuFridge");

        Version version = new Version(isDebug ? "1.0.0" : args.Single());
        string nufridgeInstallFolder = @"%ProgramFiles%\NuFridge";
        string projectName = "NuFridge";
        Guid projectGuid = new Guid("13a9f73e-6b58-4dc5-ba8f-a006491450b2");
        Guid projectUpgradeCode = new Guid("ae76838e-3ac2-4d3e-a28f-7293aec3b95d");

        Feature serviceFeature = new Feature("Windows Service", true, false);
        serviceFeature.Attributes.Add("AllowAdvertise", "no");
        string windowsServiceName = "NuFridge Server Service";
        string serviceFileFolder = string.Format(@"{0}NuFridge.Service\bin\{1}\*.*", rootPath, isDebug ? "Debug" : "Release");
        Predicate<string> serviceFileFilter = f => !f.EndsWith(".pdb") && !f.EndsWith(".xml") && !f.EndsWith(".sdf") && !f.Contains(".vshost.") && !f.EndsWith(".txt");
        topLevelFeature.Children.Add(serviceFeature);

        Feature controlPanelFeature = new Feature("Control Panel", true, true);
        controlPanelFeature.Attributes.Add("AllowAdvertise", "no");
        string controlPanelFileFolder = string.Format(@"{0}NuFridge.ControlPanel\bin\{1}\*.*", rootPath, isDebug ? "Debug" : "Release");
        Predicate<string> controlPanelFileFilter = f => !f.EndsWith(".pdb") && !f.EndsWith(".xml") && !f.EndsWith(".sdf") && !f.Contains(".vshost.");
        topLevelFeature.Children.Add(controlPanelFeature);

        //Not in use yet
        //Feature websiteFeature = new Feature("Website", true, true);
        //topLevelFeature.Children.Add(websiteFeature);

        Project project = new Project(projectName,
            new Dir(topLevelFeature, nufridgeInstallFolder,
                new Dir("Service")
                {
                    FileCollections = new Files[]
                    {
                        new Files(serviceFeature, serviceFileFolder, serviceFileFilter)
                    }
                },
                new Dir("ControlPanel")
                {
                    FileCollections = new Files[]
                    {
                        new Files(controlPanelFeature, controlPanelFileFolder, controlPanelFileFilter)
                    }
                })
            );


        project.UI = WUI.WixUI_FeatureTree;
        project.GUID = projectGuid;
        project.UpgradeCode = projectUpgradeCode;
        project.Version = version;

        project.ResolveWildCards();

        //Use Single because this check only works while we have one install location.
        if (project.Dirs.Single().Dirs.Single().Dirs.Any(dr => dr.Files.Count() == 0))
        {
            throw new Exception(string.Format("No files are in the {0} folder.", project.Dirs.Single().Dirs.Single().Dirs.First(dr => dr.Files.Count() == 0).Name));
        }

        File service = project.AllFiles.Single(f => f.Name.EndsWith("NuFridge.Service.exe"));

        service.ServiceInstaller = new ServiceInstaller
        {
            Name = windowsServiceName,
            StartOn = SvcEvent.Install_Wait,
            StopOn = SvcEvent.Uninstall_Wait,
            RemoveOn = SvcEvent.Uninstall_Wait,
            DisplayName = windowsServiceName,
            StartType = SvcStartType.auto,
            Description = string.Format("Version: {0}. Built on {1} by {2}.", version, DateTime.Now.ToShortDateString(), Environment.MachineName)
        };

        project.MajorUpgradeStrategy = new MajorUpgradeStrategy
        {
            UpgradeVersions = VersionRange.OlderThanThis,
            PreventDowngradingVersions = VersionRange.NewerThanThis,
            NewerProductInstalledErrorMessage = "Newer version already installed.",
            RemoveExistingProductAfter = Step.InstallInitialize
        };

        Compiler.BuildMsi(project);
    }

    private static string GetPathToSrcFolder()
    {
        DirectoryInfo currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        string path = string.Empty;

        while (currentDirectory.Name.ToLower() != "src")
        {
            path += @"..\";

            currentDirectory = Directory.GetParent(currentDirectory.FullName);
        }

        return path;
    }
}


