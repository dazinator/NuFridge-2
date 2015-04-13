using System.Windows.Forms;

namespace NuFridge.ControlPanel
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.materialTabSelector1 = new MaterialSkin.Controls.MaterialTabSelector();
            this.materialTabControl1 = new MaterialSkin.Controls.MaterialTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.materialLabel4 = new MaterialSkin.Controls.MaterialLabel();
            this.lblNuFridgeServiceStatus = new MaterialSkin.Controls.MaterialLabel();
            this.btnStopService = new MaterialSkin.Controls.MaterialFlatButton();
            this.btnRestartService = new MaterialSkin.Controls.MaterialFlatButton();
            this.btnStartService = new MaterialSkin.Controls.MaterialFlatButton();
            this.materialDivider1 = new System.Windows.Forms.Panel();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.materialLabel5 = new MaterialSkin.Controls.MaterialLabel();
            this.btnRestoreDatabase = new MaterialSkin.Controls.MaterialFlatButton();
            this.materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
            this.btnBackupDatabase = new MaterialSkin.Controls.MaterialFlatButton();
            this.materialDivider2 = new System.Windows.Forms.Panel();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.materialLabel6 = new MaterialSkin.Controls.MaterialLabel();
            this.lblUpdatesAvailable = new MaterialSkin.Controls.MaterialLabel();
            this.btnUpdate = new MaterialSkin.Controls.MaterialFlatButton();
            this.materialDivider3 = new System.Windows.Forms.Panel();
            this.nufridgeServiceWorker = new System.ComponentModel.BackgroundWorker();
            this.backupSaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.backupLoadFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.materialTabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // materialTabSelector1
            // 
            this.materialTabSelector1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.materialTabSelector1.BaseTabControl = this.materialTabControl1;
            this.materialTabSelector1.Depth = 0;
            this.materialTabSelector1.Location = new System.Drawing.Point(0, 63);
            this.materialTabSelector1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialTabSelector1.Name = "materialTabSelector1";
            this.materialTabSelector1.Size = new System.Drawing.Size(600, 28);
            this.materialTabSelector1.TabIndex = 2;
            this.materialTabSelector1.Text = "materialTabSelector1";
            // 
            // materialTabControl1
            // 
            this.materialTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.materialTabControl1.Controls.Add(this.tabPage1);
            this.materialTabControl1.Controls.Add(this.tabPage2);
            this.materialTabControl1.Controls.Add(this.tabPage3);
            this.materialTabControl1.Depth = 0;
            this.materialTabControl1.Location = new System.Drawing.Point(12, 97);
            this.materialTabControl1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialTabControl1.Name = "materialTabControl1";
            this.materialTabControl1.SelectedIndex = 0;
            this.materialTabControl1.Size = new System.Drawing.Size(576, 156);
            this.materialTabControl1.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.materialLabel4);
            this.tabPage1.Controls.Add(this.lblNuFridgeServiceStatus);
            this.tabPage1.Controls.Add(this.btnStopService);
            this.tabPage1.Controls.Add(this.btnRestartService);
            this.tabPage1.Controls.Add(this.btnStartService);
            this.tabPage1.Controls.Add(this.materialDivider1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(568, 130);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Windows Service";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // materialLabel4
            // 
            this.materialLabel4.AutoSize = true;
            this.materialLabel4.Depth = 0;
            this.materialLabel4.Font = new System.Drawing.Font("Roboto", 11F);
            this.materialLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialLabel4.Location = new System.Drawing.Point(6, 49);
            this.materialLabel4.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel4.Name = "materialLabel4";
            this.materialLabel4.Size = new System.Drawing.Size(61, 19);
            this.materialLabel4.TabIndex = 10;
            this.materialLabel4.Text = "Actions";
            // 
            // lblNuFridgeServiceStatus
            // 
            this.lblNuFridgeServiceStatus.AutoSize = true;
            this.lblNuFridgeServiceStatus.Depth = 0;
            this.lblNuFridgeServiceStatus.Font = new System.Drawing.Font("Roboto", 11F);
            this.lblNuFridgeServiceStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblNuFridgeServiceStatus.Location = new System.Drawing.Point(6, 16);
            this.lblNuFridgeServiceStatus.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblNuFridgeServiceStatus.Name = "lblNuFridgeServiceStatus";
            this.lblNuFridgeServiceStatus.Size = new System.Drawing.Size(108, 19);
            this.lblNuFridgeServiceStatus.TabIndex = 4;
            this.lblNuFridgeServiceStatus.Text = "materialLabel1";
            // 
            // btnStopService
            // 
            this.btnStopService.AutoSize = true;
            this.btnStopService.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnStopService.Depth = 0;
            this.btnStopService.Enabled = false;
            this.btnStopService.Location = new System.Drawing.Point(87, 82);
            this.btnStopService.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnStopService.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnStopService.Name = "btnStopService";
            this.btnStopService.Primary = true;
            this.btnStopService.Size = new System.Drawing.Size(47, 36);
            this.btnStopService.TabIndex = 3;
            this.btnStopService.Text = "Stop";
            this.btnStopService.UseVisualStyleBackColor = true;
            this.btnStopService.Click += new System.EventHandler(this.btnStopService_Click);
            // 
            // btnRestartService
            // 
            this.btnRestartService.AutoSize = true;
            this.btnRestartService.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRestartService.Depth = 0;
            this.btnRestartService.Enabled = false;
            this.btnRestartService.Location = new System.Drawing.Point(142, 82);
            this.btnRestartService.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnRestartService.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRestartService.Name = "btnRestartService";
            this.btnRestartService.Primary = true;
            this.btnRestartService.Size = new System.Drawing.Size(71, 36);
            this.btnRestartService.TabIndex = 2;
            this.btnRestartService.Text = "Restart";
            this.btnRestartService.UseVisualStyleBackColor = true;
            this.btnRestartService.Click += new System.EventHandler(this.btnRestartService_Click);
            // 
            // btnStartService
            // 
            this.btnStartService.AutoSize = true;
            this.btnStartService.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnStartService.Depth = 0;
            this.btnStartService.Enabled = false;
            this.btnStartService.Location = new System.Drawing.Point(24, 82);
            this.btnStartService.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnStartService.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnStartService.Name = "btnStartService";
            this.btnStartService.Primary = true;
            this.btnStartService.Size = new System.Drawing.Size(55, 36);
            this.btnStartService.TabIndex = 1;
            this.btnStartService.Text = "Start";
            this.btnStartService.UseVisualStyleBackColor = true;
            this.btnStartService.Click += new System.EventHandler(this.btnStartService_Click);
            // 
            // materialDivider1
            // 
            this.materialDivider1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.materialDivider1.Location = new System.Drawing.Point(10, 71);
            this.materialDivider1.Name = "materialDivider1";
            this.materialDivider1.Size = new System.Drawing.Size(552, 59);
            this.materialDivider1.TabIndex = 9;
            this.materialDivider1.Text = "materialDivider1";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.materialLabel5);
            this.tabPage2.Controls.Add(this.btnRestoreDatabase);
            this.tabPage2.Controls.Add(this.materialLabel1);
            this.tabPage2.Controls.Add(this.btnBackupDatabase);
            this.tabPage2.Controls.Add(this.materialDivider2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(568, 130);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Database";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // materialLabel5
            // 
            this.materialLabel5.AutoSize = true;
            this.materialLabel5.Depth = 0;
            this.materialLabel5.Font = new System.Drawing.Font("Roboto", 11F);
            this.materialLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialLabel5.Location = new System.Drawing.Point(6, 49);
            this.materialLabel5.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel5.Name = "materialLabel5";
            this.materialLabel5.Size = new System.Drawing.Size(61, 19);
            this.materialLabel5.TabIndex = 14;
            this.materialLabel5.Text = "Actions";
            // 
            // btnRestoreDatabase
            // 
            this.btnRestoreDatabase.AutoSize = true;
            this.btnRestoreDatabase.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnRestoreDatabase.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRestoreDatabase.Depth = 0;
            this.btnRestoreDatabase.Location = new System.Drawing.Point(97, 82);
            this.btnRestoreDatabase.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnRestoreDatabase.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRestoreDatabase.Name = "btnRestoreDatabase";
            this.btnRestoreDatabase.Primary = true;
            this.btnRestoreDatabase.Size = new System.Drawing.Size(71, 36);
            this.btnRestoreDatabase.TabIndex = 6;
            this.btnRestoreDatabase.Text = "Restore";
            this.btnRestoreDatabase.UseVisualStyleBackColor = true;
            this.btnRestoreDatabase.Click += new System.EventHandler(this.btnRestoreDatabase_Click);
            // 
            // materialLabel1
            // 
            this.materialLabel1.AutoSize = true;
            this.materialLabel1.Depth = 0;
            this.materialLabel1.Font = new System.Drawing.Font("Roboto", 11F);
            this.materialLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialLabel1.Location = new System.Drawing.Point(6, 16);
            this.materialLabel1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel1.Name = "materialLabel1";
            this.materialLabel1.Size = new System.Drawing.Size(283, 19);
            this.materialLabel1.TabIndex = 5;
            this.materialLabel1.Text = "Backups do not include NuGet packages.";
            // 
            // btnBackupDatabase
            // 
            this.btnBackupDatabase.AutoSize = true;
            this.btnBackupDatabase.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnBackupDatabase.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBackupDatabase.Depth = 0;
            this.btnBackupDatabase.Location = new System.Drawing.Point(24, 82);
            this.btnBackupDatabase.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnBackupDatabase.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnBackupDatabase.Name = "btnBackupDatabase";
            this.btnBackupDatabase.Primary = true;
            this.btnBackupDatabase.Size = new System.Drawing.Size(65, 36);
            this.btnBackupDatabase.TabIndex = 2;
            this.btnBackupDatabase.Text = "Backup";
            this.btnBackupDatabase.UseVisualStyleBackColor = true;
            this.btnBackupDatabase.Click += new System.EventHandler(this.btnBackupDatabase_Click);
            // 
            // materialDivider2
            // 
            this.materialDivider2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.materialDivider2.Location = new System.Drawing.Point(10, 71);
            this.materialDivider2.Name = "materialDivider2";
            this.materialDivider2.Size = new System.Drawing.Size(552, 59);
            this.materialDivider2.TabIndex = 13;
            this.materialDivider2.Text = "materialDivider2";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.materialLabel6);
            this.tabPage3.Controls.Add(this.lblUpdatesAvailable);
            this.tabPage3.Controls.Add(this.btnUpdate);
            this.tabPage3.Controls.Add(this.materialDivider3);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(568, 130);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Updates";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // materialLabel6
            // 
            this.materialLabel6.AutoSize = true;
            this.materialLabel6.Depth = 0;
            this.materialLabel6.Font = new System.Drawing.Font("Roboto", 11F);
            this.materialLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialLabel6.Location = new System.Drawing.Point(6, 49);
            this.materialLabel6.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel6.Name = "materialLabel6";
            this.materialLabel6.Size = new System.Drawing.Size(61, 19);
            this.materialLabel6.TabIndex = 16;
            this.materialLabel6.Text = "Actions";
            // 
            // lblUpdatesAvailable
            // 
            this.lblUpdatesAvailable.AutoSize = true;
            this.lblUpdatesAvailable.Depth = 0;
            this.lblUpdatesAvailable.Font = new System.Drawing.Font("Roboto", 11F);
            this.lblUpdatesAvailable.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblUpdatesAvailable.Location = new System.Drawing.Point(6, 16);
            this.lblUpdatesAvailable.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblUpdatesAvailable.Name = "lblUpdatesAvailable";
            this.lblUpdatesAvailable.Size = new System.Drawing.Size(178, 19);
            this.lblUpdatesAvailable.TabIndex = 4;
            this.lblUpdatesAvailable.Text = "No updates are available.";
            // 
            // btnUpdate
            // 
            this.btnUpdate.AutoSize = true;
            this.btnUpdate.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnUpdate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUpdate.Depth = 0;
            this.btnUpdate.Location = new System.Drawing.Point(24, 82);
            this.btnUpdate.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnUpdate.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Primary = true;
            this.btnUpdate.Size = new System.Drawing.Size(64, 36);
            this.btnUpdate.TabIndex = 3;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // materialDivider3
            // 
            this.materialDivider3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.materialDivider3.Location = new System.Drawing.Point(10, 71);
            this.materialDivider3.Name = "materialDivider3";
            this.materialDivider3.Size = new System.Drawing.Size(552, 59);
            this.materialDivider3.TabIndex = 15;
            this.materialDivider3.Text = "materialDivider3";
            // 
            // nufridgeServiceWorker
            // 
            this.nufridgeServiceWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.nufridgeServiceWorker_DoWork);
            this.nufridgeServiceWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.nufridgeServiceWorker_RunWorkerCompleted);
            // 
            // backupSaveFileDialog
            // 
            this.backupSaveFileDialog.Filter = "NuFridge Database Backup|*.nfdb";
            this.backupSaveFileDialog.Title = "Backup Database";
            // 
            // backupLoadFileDialog
            // 
            this.backupLoadFileDialog.Filter = "NuFridge Database Backup|*.nfdb";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 254);
            this.Controls.Add(this.materialTabControl1);
            this.Controls.Add(this.materialTabSelector1);
            this.MaximumSize = new System.Drawing.Size(600, 254);
            this.MinimumSize = new System.Drawing.Size(600, 254);
            this.Name = "MainForm";
            this.Text = "NuFridge Control Panel";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.materialTabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private MaterialSkin.Controls.MaterialTabSelector materialTabSelector1;
        private MaterialSkin.Controls.MaterialTabControl materialTabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private MaterialSkin.Controls.MaterialFlatButton btnStartService;
        private MaterialSkin.Controls.MaterialFlatButton btnStopService;
        private MaterialSkin.Controls.MaterialFlatButton btnRestartService;
        private MaterialSkin.Controls.MaterialLabel lblNuFridgeServiceStatus;
        private System.ComponentModel.BackgroundWorker nufridgeServiceWorker;
        private System.Windows.Forms.TabPage tabPage2;
        private MaterialSkin.Controls.MaterialLabel materialLabel1;
        private MaterialSkin.Controls.MaterialFlatButton btnBackupDatabase;
        private MaterialSkin.Controls.MaterialFlatButton btnRestoreDatabase;
        private System.Windows.Forms.SaveFileDialog backupSaveFileDialog;
        private System.Windows.Forms.OpenFileDialog backupLoadFileDialog;
        private System.Windows.Forms.TabPage tabPage3;
        private MaterialSkin.Controls.MaterialFlatButton btnUpdate;
        private MaterialSkin.Controls.MaterialLabel lblUpdatesAvailable;
        private MaterialSkin.Controls.MaterialLabel materialLabel4;
        private Panel materialDivider1;
        private MaterialSkin.Controls.MaterialLabel materialLabel5;
        private Panel materialDivider2;
        private MaterialSkin.Controls.MaterialLabel materialLabel6;
        private Panel materialDivider3;
    }
}

