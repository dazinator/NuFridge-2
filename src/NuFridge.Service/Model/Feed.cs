namespace NuFridge.Service.Model
{
    //When adding new properties please remember to add them to the javascript view model so values are not lost on database updates
    public class Feed : IEntityBase
    {

        public string Id { get; set; }

        public string Name { get; set; }
        public string ApiKey { get; set; }

        public string Host { get; set; }
        public int Port { get; set; }
        public string VirtualDirectory { get; set; }

        public bool SynchronizeOnStart { get; set; }
        public bool EnablePackageFileWatcher { get; set; }
        public bool GroupPackageFilesById { get; set; }
        public bool AllowPackageOverwrite { get; set; }

        public string GetUrl()
        {

            string port;
            string virtualDirectory;

            //No https support at the moment
            if (Port == 80)
            {
                port = string.Empty;
            }
            else
            {
                port = ":" + Port;
            }

            if (VirtualDirectory != null && VirtualDirectory != "/")
            {
                virtualDirectory = VirtualDirectory;

                if (!virtualDirectory.StartsWith("/"))
                {
                    virtualDirectory = "/" + virtualDirectory;
                }

                if (!virtualDirectory.EndsWith("/"))
                {
                    virtualDirectory = virtualDirectory + "/";
                }

            }
            else
            {
                virtualDirectory = "/";
            }


            return string.Format("http://{0}{1}{2}", Host, port, virtualDirectory);
        }
    }
}