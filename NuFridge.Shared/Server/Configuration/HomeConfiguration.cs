using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using NuFridge.Shared.Server.Application;

namespace NuFridge.Shared.Server.Configuration
{
    public class HomeConfiguration : IHomeConfiguration
    {
        private readonly IApplicationInstanceSelector _instance;

        public string InstallDirectory { get; set; }
        public string SqlDataSource { get; set; }
        public string SqlInitialCatalog { get; set; }
        public string SqlUsername { get; set; }
        public string SqlPassword { get; set; }
        public string ListenPrefixes { get; set; }
        public string WindowsDebuggingToolsPath { get; set; }
        public SqlAuthentication SqlAuthenticationMode { get; set; }


        public enum SqlAuthentication
        {
            WindowsAuthentication = 1,
            UserIdPasswordAuthentication = 2
        }

        public string NuGetFrameworkNames
        {
            get { return _instance.Current.NuGetFrameworkNames; }
            set { _instance.Current.NuGetFrameworkNames = value; }
        }

        private bool TryParseEnum<T>(string str, out T value) where T : struct
        {
            var names = Enum.GetNames(typeof(T));
            value = (Enum.GetValues(typeof(T)) as T[])[0];
            foreach (var name in names)
            {
                if (String.Equals(name, str, StringComparison.OrdinalIgnoreCase))
                {
                    value = (T)Enum.Parse(typeof(T), name);
                    return true;
                }
            }
            return false;
        }

        public HomeConfiguration(IApplicationInstanceSelector instance)
        {
            _instance = instance;

            if (instance.Current == null)
            {
                instance.LoadInstance();
            }

            InstallDirectory = instance.Current.InstallDirectory;
            SqlDataSource = ConfigurationManager.AppSettings["SqlServer"];
            SqlInitialCatalog = ConfigurationManager.AppSettings["SqlDatabase"];
            SqlUsername = ConfigurationManager.AppSettings["SqlUserId"];
            SqlPassword = ConfigurationManager.AppSettings["SqlPassword"];
            ListenPrefixes = ConfigurationManager.AppSettings["WebsiteUrl"];
            WindowsDebuggingToolsPath = ConfigurationManager.AppSettings["WindowsDebuggingToolsPath"];

            SqlAuthentication value;
            if (TryParseEnum(ConfigurationManager.AppSettings["SqlAuthenticationMode"], out value))
            {
                SqlAuthenticationMode = value;

                if (value == SqlAuthentication.UserIdPasswordAuthentication)
                {
                    if (string.IsNullOrWhiteSpace(SqlUsername) || string.IsNullOrWhiteSpace(SqlPassword))
                    {
                        throw new InvalidOperationException("Please provide a SQL username and SQL password when using UserIdPasswordAuthentication in the configuration file.");
                    }
                }
            }
            else
            {
                throw new InvalidEnumArgumentException("The SqlAuthenticationMode in the configuration file must either be WindowsAuthentication or UserIdPasswordAuthentication.");
            }

        }

        public void Save()
        {
            _instance.Save();
        }
    }
}