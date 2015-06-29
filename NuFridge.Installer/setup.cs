using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using WixSharp;
using WixSharp.CommonTasks;
using File = WixSharp.File;
using Microsoft.Deployment.WindowsInstaller;

public class Setup
{
    public static void Main(string[] args)
    {
#if DEBUG
        bool isDebug = true;
#else
        bool isDebug = false;
#endif

        var rootPath = GetPathToSrcFolder(isDebug);

        Console.WriteLine("Root: " + rootPath);

        Feature topLevelFeature = new Feature("NuFridge");

        Version version = new Version(isDebug && !args.Any() ? "1.0.0" : args.First());
        string nufridgeInstallFolder = @"%ProgramFiles64Folder%";
        string projectName = "NuFridge";
        Guid projectGuid = new Guid("13a9f73e-6b58-4dc5-ba8f-a006491450b2");
        Guid projectUpgradeCode = new Guid("ae76838e-3ac2-4d3e-a28f-7293aec3b95d");

      //  Feature serviceFeature = new Feature("Windows Service", true, false);
       // serviceFeature.Attributes.Add("AllowAdvertise", "no");
        string windowsServiceName = "NuFridge Server Service";
        string serviceFileFolder = string.Format(@"NuFridge.Service\bin\{1}\*.*", rootPath, isDebug ? "Debug" : "Release");
        Predicate<string> serviceFileFilter = f => !f.EndsWith(".pdb") && !f.EndsWith(".xml") && !f.EndsWith(".sdf") && !f.Contains(".vshost.") && !f.EndsWith(".txt") && !f.EndsWith(".nupkg");
      //  topLevelFeature.Children.Add(serviceFeature);


        Project project = new Project(projectName,
              new ManagedAction("ReadInstallDir", Return.ignore, When.Before, new Step("AppSearch"), Condition.NOT_Installed, Sequence.InstallExecuteSequence | Sequence.InstallUISequence) { Execute = Execute.firstSequence },
                new ElevatedManagedAction("SaveInstallDir", Return.check, When.After, Step.InstallFiles, Condition.NOT_Installed))

        {
            Dirs = new[]
            {
                new Dir(nufridgeInstallFolder)
                {
                    Dirs = new[]
                    {
                        new Dir("NuFridge",

                            new Dir("NuFridge Server")
                            {
                                Dirs = new Dir[]
                                {
                                    new Dir("Service")
                                    {
                                        FileCollections = new Files[]
                                        {
                                            new Files(topLevelFeature, serviceFileFolder, serviceFileFilter)
                                        },
                                        Id = "INSTALLDIR.Service"
                                    }
                                },
                                Id = "INSTALLDIR"

                            })
                    }
                }
            },
            Properties = new[]
            {
                new Property("NuFridgeVersion", version.ToString()), 
            }
        };
       
        

        

        project.SourceBaseDir = rootPath;
        project.Manufacturer = "NuFridge";
        project.Platform = Platform.x64;
        project.UI = WUI.WixUI_InstallDir;
       
        project.GUID = projectGuid; 
        project.UpgradeCode = projectUpgradeCode;
        project.Version = version;

        Console.WriteLine("Version to build: " + project.Version.ToString());

        project.ResolveWildCards();

        if (!project.AllFiles.Any())
        {
            throw new Exception("No files are in the project.");
        }

        File service = project.AllFiles.Single(f => f.Name.EndsWith("NuFridge.Service.exe"));
    
        service.ServiceInstaller = new ServiceInstaller
        {
            Name = windowsServiceName,
            StopOn = SvcEvent.Uninstall_Wait,
            RemoveOn = SvcEvent.Uninstall_Wait,
            DisplayName = windowsServiceName,
            StartType = SvcStartType.auto,
            Description = string.Format("Version: {0}.", version)
        };

        project.MajorUpgradeStrategy = new MajorUpgradeStrategy
        {
            UpgradeVersions = VersionRange.ThisAndOlder,
            PreventDowngradingVersions = VersionRange.NewerThanThis,
            NewerProductInstalledErrorMessage = "Newer version already installed.",
            RemoveExistingProductAfter = Step.InstallInitialize
        };

        if (isDebug)
        {
            Compiler.PreserveTempFiles = true;
        }

        Compiler.WixSourceGenerated += Compiler_WixSourceGenerated;

        Compiler.BuildMsi(project);

        if (args.Count() == 2)
        {
            System.IO.File.Move("NuFridge.msi", args.Skip(1).First());
        }
    }

    static void Compiler_WixSourceGenerated(System.Xml.Linq.XDocument document)
    {
        var productElement = document.Root.Select("Product");

        var installDir = productElement.Elements("Property").First(p => p.Attribute("Id").Value == "WIXUI_INSTALLDIR");

        installDir.SetAttributeValue("Value", "INSTALLDIR");
    }

    private static string GetPathToSrcFolder(bool debug)
    {
        DirectoryInfo currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        if (debug)
        {
            string path = string.Empty;

            for (int i = 0; i < 3; i++)
            {
                path += @"..\";

                currentDirectory = Directory.GetParent(currentDirectory.FullName);
            }
        }

        return currentDirectory.FullName + @"\";
    }
}
public class CustomActions
{
    [CustomAction]
    public static ActionResult SaveInstallDir(Session session)
    {
        try
        {
            Registry.LocalMachine.CreateSubKey(@"Software\NuFridge")
                                 .SetValue("InstallationDirectory", session.Property("INSTALLDIR"));
        }
        catch { return ActionResult.Failure;}

        return ActionResult.Success;
    }

    [CustomAction]
    public static ActionResult ReadInstallDir(Session session)
    {
        try
        {
            session["INSTALLDIR"] = Registry.LocalMachine.OpenSubKey(@"Software\NuFridge")
                                                         .GetValue("InstallationDirectory")
                                                         .ToString();
        }
        catch { return ActionResult.Failure; }

        return ActionResult.Success;
    }
}