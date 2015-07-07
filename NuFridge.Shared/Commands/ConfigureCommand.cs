using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NuFridge.Shared.Installation;
using NuFridge.Shared.Server.Application;
using NuFridge.Shared.Server.Configuration;

namespace NuFridge.Shared.Commands
{
    public class ConfigureCommand : AbstractStandardCommand
    {
        private readonly IHomeConfiguration _config;

        public ConfigureCommand(IHomeConfiguration config, IApplicationInstanceSelector selector)
            : base(selector)
        {
            _config = config;
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        [STAThread]
        protected override void Start()
        {
            base.Start();

                var handle = GetConsoleWindow();
                ShowWindow(handle, SW_HIDE);

            using (ConfigurationForm form = new ConfigurationForm(_config))
            {
                var value = form.ShowDialog();
                switch (value)
                {
                    case DialogResult.Abort:
                        throw new Exception("User aborted configuration.");
                        break;
                    case DialogResult.OK:
                        return;
                    default:
                        throw new NotImplementedException("Not handled");
                        break;
                }
            }
        }
    }
}