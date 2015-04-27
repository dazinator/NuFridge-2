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

namespace NuFridge.ControlPanel
{
    public partial class MainForm : MaterialForm
    {
        private readonly MaterialSkinManager materialSkinManager;


        public MainForm()
        {
            InitializeComponent();

            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Blue600, Primary.Blue900, Primary.Blue100,
                Accent.Amber700, TextShade.WHITE);



        
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
    }
}
