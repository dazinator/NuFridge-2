using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NuFridge.Shared.Logging;
using NuFridge.Shared.Server.Configuration;

namespace NuFridge.Shared.Installation
{
    public partial class ConfigurationForm : Form
    {
        private ILog _log = LogProvider.For<ConfigurationForm>();

        public ConfigurationForm()
        {
            InitializeComponent();
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private void pbClose_Click(object sender, EventArgs e)
        {

            if (MessageBox.Show("Are you sure you wish to cancel the installation of NuFridge?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
        }

        private void EnableAllFields(bool enable)
        {
            txtSqlServer.Enabled = enable;
            txtDatabase.Enabled = enable;
            txtUserId.Enabled = enable;
            txtPassword.Enabled = enable;
            txtSiteUrl.Enabled = enable;
        }

        private void EnableAllButtons(bool enable)
        {
            pbContinue.Enabled = enable;
            pbClose.Enabled = enable;
            pbClose.Visible = enable;
        }

        private void pbContinue_Click(object sender, EventArgs e)
        {
            EnableAllButtons(false);
            EnableAllFields(false);

            if (!CheckFieldsFilledIn())
            {
                EnableAllButtons(true);
                EnableAllFields(true);
                return;
            }

            _log.Info("Testing configuation.");

            lblLoadingMessage.Text = @"Please wait. Connecting to the SQL server.";
            lblLoadingMessage.Visible = true;

            if (!IsSqlServerValid())
            {
                lblLoadingMessage.Visible = false;
                EnableAllButtons(true);
                EnableAllFields(true);
                return;
            }

            lblLoadingMessage.Text = "Please wait. Testing site URL.";

            if (!IsWebUrlValid())
            {
                lblLoadingMessage.Visible = false;
                EnableAllButtons(true);
                EnableAllFields(true);
                return;
            }

            _log.Info("Saving app.config settings.");

            var folder = Directory.GetParent(Assembly.GetEntryAssembly().Location);
            var path = Path.Combine(folder.FullName, "NuFridge.Service.exe.config");

            ExeConfigurationFileMap map = new ExeConfigurationFileMap { ExeConfigFilename = path };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);

            config.AppSettings.Settings["SqlServer"].Value = txtSqlServer.Text;
            config.AppSettings.Settings["SqlDatabase"].Value = txtDatabase.Text;
            config.AppSettings.Settings["SqlUserId"].Value = txtUserId.Text;
            config.AppSettings.Settings["SqlPassword"].Value = txtPassword.Text;
            config.AppSettings.Settings["WebsiteUrl"].Value = txtSiteUrl.Text;

            config.Save(ConfigurationSaveMode.Full);

            ConfigurationManager.RefreshSection("appSettings");

            _log.Info("Closing form.");

            DialogResult = DialogResult.OK;
            Close();
        }

        private bool IsWebUrlValid()
        {
            Uri uri;

            if (!Uri.TryCreate(txtSiteUrl.Text, UriKind.Absolute, out uri))
            {
                _log.Error("The specified site URL is not valid.");
                MessageBox.Show("The specified site URL is not valid.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (uri.Scheme != Uri.UriSchemeHttp)
            {
                _log.Error("Only HTTP is supported for the site URL in this version.");
                MessageBox.Show("Only HTTP is supported for the site URL in this version.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private bool IsSqlServerValid()
        {
            string connectionString = string.Format("Data Source={0};Initial Catalog={1};User ID={2};Password={3};", txtSqlServer.Text, txtDatabase.Text, txtUserId.Text, txtPassword.Text);
            SqlConnection connection = new SqlConnection(connectionString);

            try
            {
                connection.Open();
                return true;
            }
            catch (Exception ex)
            {
                _log.ErrorException("Error connecting to sql.", ex);
                MessageBox.Show("There was a problem connecting to the SQL Server. \r\nError: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                connection.Dispose();
            }
        }

        private bool CheckFieldsFilledIn()
        {
            if (string.IsNullOrWhiteSpace(txtSqlServer.Text))
            {
                MessageBox.Show(@"The SQL server field must be filled in.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDatabase.Text))
            {
                MessageBox.Show(@"The database field must be filled in.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUserId.Text))
            {
                MessageBox.Show(@"The user id field must be filled in.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show(@"The password field must be filled in.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtSiteUrl.Text))
            {
                MessageBox.Show(@"The website url field must be filled in.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        private void ConfigurationForm_Load(object sender, EventArgs e)
        {
            Text = @"NuFridge Installation";
        }

        private void topPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void ConfigurationForm_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}