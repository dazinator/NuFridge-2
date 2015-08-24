using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NuFridge.Shared.Installation;
using NuFridge.Shared.Server.Application;

namespace NuFridge.Shared.Commands
{
    public class ConfigureCommand : AbstractStandardCommand
    {
        private readonly string _installDirectory;

        public ConfigureCommand(IApplicationInstanceSelector selector)
            : base(selector)
        {
            selector.LoadInstance();
            _installDirectory = selector.Current.InstallDirectory;
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;

        [STAThread]
        protected override void Start()
        {
            base.Start();

            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            using (ConfigurationForm form = new ConfigurationForm(_installDirectory))
            {
                var value = form.ShowDialog();
                switch (value)
                {
                    case DialogResult.Abort:
                        throw new Exception("User aborted configuration.");
                    case DialogResult.OK:
                        return;
                    default:
                        throw new NotImplementedException("Not handled");
                }
            }
        }
    }
}