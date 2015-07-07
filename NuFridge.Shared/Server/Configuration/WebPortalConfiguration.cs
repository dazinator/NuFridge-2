namespace NuFridge.Shared.Server.Configuration
{
    public class WebPortalConfiguration : IWebPortalConfiguration
    {

        public string ListenPrefixes{ get; set; }

        public WebPortalConfiguration(IHomeConfiguration config)
        {
            ListenPrefixes = config.ListenPrefixes;
        }
    }
}