using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.Win32;
using wyDay.Controls;

namespace NuFridge.ControlPanel
{
    public partial class MainForm : MaterialForm
    {
        private readonly MaterialSkinManager materialSkinManager;

        static AutomaticUpdaterBackend auBackend;


        public MainForm()
        {
            InitializeComponent();

            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Blue600, Primary.Blue900, Primary.Blue100,
                Accent.Amber700, TextShade.WHITE);

            auBackend = new AutomaticUpdaterBackend
            {
                //TODO: set a unique string.
                // For instance, "appname-companyname"
                GUID = "9a136b4f-ed23-41fd-a544-b18e156894a9",
                UpdateType = UpdateType.OnlyCheck


            };

        
        }

        private string websiteUrl { get; set; }
        private string feedUrl { get; set; }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ServiceController service = GetService();

            if (service == null)
            {
                MessageBox.Show("The NuFridge Windows Service could not be found.");
                Application.Exit();
                return;
            }

            UpdateButtons(service.Status);

            var exePath = GetServiceInstallPath(service.ServiceName);

            if (exePath == null || !File.Exists(exePath))
            {
                return;
            }

            var configPath = string.Format("{0}.config", exePath);
            if (!File.Exists(configPath))
            {
                return;
            }

            var config = ConfigurationManager.OpenExeConfiguration(exePath);

            //hypWebsiteBinding.Text = config.AppSettings.Settings["NuFridge.AdministrationWebsite.Binding"].Value;
            //websiteUrl = hypWebsiteBinding.Text;

            //hypFeedBinding.Text = config.AppSettings.Settings["NuFridge.Feeds.Binding"].Value;
            //feedUrl = hypFeedBinding.Text;

            auBackend.UpdateAvailable +=auBackend_UpdateAvailable;
            auBackend.DownloadingFailed += auBackend_DownloadingFailed;
            auBackend.ExtractingFailed += auBackend_ExtractingFailed;
            auBackend.CheckingFailed += auBackend_CheckingFailed;
            auBackend.UpdateFailed += auBackend_UpdateFailed;
            auBackend.BeforeChecking += auBackend_BeforeChecking;
            auBackend.UpToDate += auBackend_UpToDate;
            auBackend.CloseAppNow += auBackend_CloseAppNow;
            auBackend.UpdateSuccessful += auBackend_UpdateSuccessful;
            auBackend.Initialize();
            auBackend.AppLoaded();

            if (!auBackend.ClosingForInstall)
            {
                if (auBackend.UpdateStepOn == UpdateStepOn.Nothing)
                {
                    auBackend.ForceCheckForUpdate();
                }
            }
        }

        void auBackend_UpdateSuccessful(object sender, SuccessArgs e)
        {

        }

        void auBackend_CloseAppNow(object sender, EventArgs e)
        {
         Application.Exit();
        }

        void auBackend_UpToDate(object sender, SuccessArgs e)
        {
            Invoke((MethodInvoker) delegate
            {

            btnUpdate.Enabled = false;
            lblUpdatesAvailable.Text = "No updates are available.";

            });
        }

        void auBackend_BeforeChecking(object sender, BeforeArgs e)
        {

        }

        void auBackend_UpdateFailed(object sender, FailArgs e)
        {

            if (MessageBox.Show(e.ErrorMessage, e.ErrorTitle, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) ==
             DialogResult.Retry)
            {
                auBackend.InstallNow();
            }
        }

        void auBackend_CheckingFailed(object sender, FailArgs e)
        {
            Invoke((MethodInvoker) delegate
            {

                lblUpdatesAvailable.Text = "Failed to check for available updates.";

                btnUpdate.Enabled = false;
            });
        }

        void auBackend_ExtractingFailed(object sender, FailArgs e)
        {
            if (MessageBox.Show(e.ErrorMessage, e.ErrorTitle, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) ==
                DialogResult.Retry)
            {
                auBackend.InstallNow();
            }
        }

        void auBackend_DownloadingFailed(object sender, FailArgs e)
        {
            if (MessageBox.Show(e.ErrorMessage, e.ErrorTitle, MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) ==
                DialogResult.Retry)
            {
                auBackend.InstallNow();
            }
        }

        private void auBackend_UpdateAvailable(object sender, EventArgs e)
        {
            Invoke((MethodInvoker) delegate
            {

                btnUpdate.Enabled = true;
                lblUpdatesAvailable.Text = "An update is available.";
                //txtWhatsNew.Text = auBackend.Changes;
                //lblWhatsNew.Text = "What's new in v" + auBackend.Version;
                //lblWhatsNew.Visible = true;
                //txtWhatsNew.Visible = true;
            });
        }

        private ServiceController GetService()
        {
            return ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "NuFridge Server Service");
        }

        private void UpdateButtons(ServiceControllerStatus currentStatus)
        {
            switch (currentStatus)
            {
                case ServiceControllerStatus.Stopped:
                    UpdateButton(false, true, false);
                    lblNuFridgeServiceStatus.Text = "The NuFridge Windows Service is not running.";
                    break;
                case ServiceControllerStatus.Running:
                    UpdateButton(true, false, true);
                    lblNuFridgeServiceStatus.Text = "The NuFridge Windows Service is running.";
                    break;
                default:
                    lblNuFridgeServiceStatus.Text = "The NuFridge Windows Service is not running.";
                    break;
            }
        }

        private void UpdateButton(bool restartEnabled, bool startEnabled, bool stopEnabled)
        {
            btnRestartService.Enabled = restartEnabled;
            btnStartService.Enabled = startEnabled;
            btnStopService.Enabled = stopEnabled;

            DisableButton(btnRestartService, restartEnabled);
            DisableButton(btnStartService, startEnabled);
            DisableButton(btnStopService, stopEnabled);
        }

        private void btnStartService_Click(object sender, EventArgs e)
        {
            if (nufridgeServiceWorker.IsBusy)
            {
                return;
            }

            nufridgeServiceWorker.RunWorkerAsync(ServiceAction.Start);
        }

        private void btnRestartService_Click(object sender, EventArgs e)
        {
            if (nufridgeServiceWorker.IsBusy)
            {
                return;
            }

            nufridgeServiceWorker.RunWorkerAsync(ServiceAction.Restart);
        }

        public enum ServiceAction
        {
            Start,
            Stop,
            Restart
        }

        private void btnStopService_Click(object sender, EventArgs e)
        {
            if (nufridgeServiceWorker.IsBusy)
            {
                return;
            }

            nufridgeServiceWorker.RunWorkerAsync(ServiceAction.Stop);
        }

        private void nufridgeServiceWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var service = GetService();

            if (service == null)
            {
                return;
            }

            if (e.Argument == null)
            {
                return;
            }

            Invoke((MethodInvoker)delegate
            {
                Cursor = Cursors.AppStarting;

                DisableButton(btnRestartService, false);
                DisableButton(btnStartService, false);
                DisableButton(btnStopService, false);
            });

            var action = (ServiceAction)e.Argument;

            if (action == ServiceAction.Stop || action == ServiceAction.Restart)
            {
                Invoke((MethodInvoker)delegate
                {
                    lblNuFridgeServiceStatus.Text = "The NuFridge Windows Service is stopping.";
                });

                service.Stop();

                try
                {

                    service.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 2, 0));
                }
                catch (System.ServiceProcess.TimeoutException)
                {
                    if (service.Status == ServiceControllerStatus.Stopped)
                    {
                        MessageBox.Show("There was a problem stopping the NuFridge Server Service. Please check the log files.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    MessageBox.Show("There was a problem stopping the NuFridge Server Service. The state of the service is currently '" + service.Status + "'.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

            }

            if (action == ServiceAction.Restart)
            {
                Invoke((MethodInvoker)delegate
                {
                    UpdateButtons(service.Status);
                });
            }

            if (action == ServiceAction.Start || action == ServiceAction.Restart)
            {
                Invoke((MethodInvoker)delegate
                {
                    lblNuFridgeServiceStatus.Text = "The NuFridge Windows Service is starting.";
                });

                service.Start();

                try
                {
                    service.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 2, 0));
                }
                catch (System.ServiceProcess.TimeoutException)
                {
                    if (service.Status == ServiceControllerStatus.Stopped)
                    {
                        MessageBox.Show("There was a problem starting the NuFridge Server Service. Please check the log files.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    MessageBox.Show("There was a problem starting the NuFridge Server Service. The state of the service is currently '" + service.Status + "'.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DisableButton(MaterialFlatButton btn, bool enabled)
        {
            btn.Enabled = enabled;

            if (btn.Enabled)
            {
                btn.Cursor = Cursors.Hand;
            }
            else
            {
                btn.Cursor = Cursors.Default;
            }
        }

        private void nufridgeServiceWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Invoke((MethodInvoker)delegate
            {
                Cursor = Cursors.Default;
            });

            var service = GetService();

            Invoke((MethodInvoker)delegate
            {
                UpdateButtons(service.Status);
            });
        }

        private static string GetServiceInstallPath(string serviceName)
        {
            var regkey = Registry.LocalMachine.OpenSubKey(string.Format(@"SYSTEM\CurrentControlSet\services\{0}", serviceName));

            if (regkey.GetValue("ImagePath") == null)
                return null;
            return regkey.GetValue("ImagePath").ToString();
        }

        private void btnBackupDatabase_Click(object sender, EventArgs e)
        {
            var service = GetService();

            var exePath = GetServiceInstallPath(service.ServiceName);

            if (exePath == null || !File.Exists(exePath))
            {
                MessageBox.Show("The NuFridge Server Service does not have a valid physical path.");
                return;
            }

            var directoryPath = Directory.GetParent(exePath);

            var backupPath = Path.Combine(directoryPath.FullName, "Database");

            if (!Directory.Exists(backupPath))
            {
                MessageBox.Show("The NuFridge Server Service does not have a valid database physical path.");
                return;
            }

            backupSaveFileDialog.FileName = string.Format("{0}{1}NuFridgeDatabaseBackup-{2:yyyy-MM-dd_hh-mm-ss-tt}.nfdb",
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        Path.DirectorySeparatorChar,
        DateTime.Now);

            if (backupSaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                ZipFile.CreateFromDirectory(backupPath, backupSaveFileDialog.FileName, CompressionLevel.Optimal, false);
            }
        }

        private void btnRestoreDatabase_Click(object sender, EventArgs e)
        {
            var service = GetService();

            var exePath = GetServiceInstallPath(service.ServiceName);

            if (exePath == null || !File.Exists(exePath))
            {
                MessageBox.Show("The NuFridge Server Service does not have a valid physical path.");
                return;
            }

            var directoryPath = Directory.GetParent(exePath);

            var backupPath = Path.Combine(directoryPath.FullName, "Database");

            if (!Directory.Exists(backupPath))
            {
                MessageBox.Show("The NuFridge Server Service does not have a valid database physical path.");
                return;
            }

            string[] backupPathFiles = Directory.GetFiles(backupPath);

            if (backupLoadFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (!backupPathFiles.Any() || MessageBox.Show("An existing database was found. Are you sure you want to overwrite it?", "Restore Database", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    foreach (string file in backupPathFiles)
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        if (fileInfo.Exists)
                        {
                            fileInfo.Delete();
                        }
                    }

                    ZipFile.ExtractToDirectory(backupLoadFileDialog.FileName, backupPath);
                }
            }
        }

        private void hypWebsiteBinding_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(websiteUrl.Replace("*", "localhost"));
        }

        private void hypFeedBinding_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(feedUrl.Replace("*", "localhost"));
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            auBackend.InstallNow();
        }
    }
}
