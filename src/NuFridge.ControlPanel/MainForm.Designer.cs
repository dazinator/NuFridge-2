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
            this.hypFeedBinding = new System.Windows.Forms.LinkLabel();
            this.materialLabel3 = new MaterialSkin.Controls.MaterialLabel();
            this.materialLabel2 = new MaterialSkin.Controls.MaterialLabel();
            this.hypWebsiteBinding = new System.Windows.Forms.LinkLabel();
            this.lblNuFridgeServiceStatus = new MaterialSkin.Controls.MaterialLabel();
            this.btnStopService = new MaterialSkin.Controls.MaterialRaisedButton();
            this.btnRestartService = new MaterialSkin.Controls.MaterialRaisedButton();
            this.btnStartService = new MaterialSkin.Controls.MaterialRaisedButton();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnRestoreDatabase = new MaterialSkin.Controls.MaterialRaisedButton();
            this.materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
            this.btnBackupDatabase = new MaterialSkin.Controls.MaterialRaisedButton();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.lblWhatsNew = new MaterialSkin.Controls.MaterialLabel();
            this.txtWhatsNew = new System.Windows.Forms.TextBox();
            this.lblUpdatesAvailable = new MaterialSkin.Controls.MaterialLabel();
            this.btnUpdate = new MaterialSkin.Controls.MaterialRaisedButton();
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
            this.materialTabControl1.Size = new System.Drawing.Size(576, 282);
            this.materialTabControl1.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.hypFeedBinding);
            this.tabPage1.Controls.Add(this.materialLabel3);
            this.tabPage1.Controls.Add(this.materialLabel2);
            this.tabPage1.Controls.Add(this.hypWebsiteBinding);
            this.tabPage1.Controls.Add(this.lblNuFridgeServiceStatus);
            this.tabPage1.Controls.Add(this.btnStopService);
            this.tabPage1.Controls.Add(this.btnRestartService);
            this.tabPage1.Controls.Add(this.btnStartService);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(568, 256);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Windows Service";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // hypFeedBinding
            // 
            this.hypFeedBinding.AutoSize = true;
            this.hypFeedBinding.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.hypFeedBinding.Location = new System.Drawing.Point(286, 110);
            this.hypFeedBinding.Name = "hypFeedBinding";
            this.hypFeedBinding.Size = new System.Drawing.Size(73, 18);
            this.hypFeedBinding.TabIndex = 8;
            this.hypFeedBinding.TabStop = true;
            this.hypFeedBinding.Text = "linkLabel1";
            this.hypFeedBinding.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.hypFeedBinding_LinkClicked);
            // 
            // materialLabel3
            // 
            this.materialLabel3.AutoSize = true;
            this.materialLabel3.Depth = 0;
            this.materialLabel3.Font = new System.Drawing.Font("Roboto", 11F);
            this.materialLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialLabel3.Location = new System.Drawing.Point(227, 110);
            this.materialLabel3.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel3.Name = "materialLabel3";
            this.materialLabel3.Size = new System.Drawing.Size(53, 19);
            this.materialLabel3.TabIndex = 7;
            this.materialLabel3.Text = "Feeds:";
            // 
            // materialLabel2
            // 
            this.materialLabel2.AutoSize = true;
            this.materialLabel2.Depth = 0;
            this.materialLabel2.Font = new System.Drawing.Font("Roboto", 11F);
            this.materialLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialLabel2.Location = new System.Drawing.Point(213, 71);
            this.materialLabel2.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel2.Name = "materialLabel2";
            this.materialLabel2.Size = new System.Drawing.Size(67, 19);
            this.materialLabel2.TabIndex = 6;
            this.materialLabel2.Text = "Website:";
            // 
            // hypWebsiteBinding
            // 
            this.hypWebsiteBinding.AutoSize = true;
            this.hypWebsiteBinding.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.hypWebsiteBinding.Location = new System.Drawing.Point(286, 71);
            this.hypWebsiteBinding.Name = "hypWebsiteBinding";
            this.hypWebsiteBinding.Size = new System.Drawing.Size(73, 18);
            this.hypWebsiteBinding.TabIndex = 5;
            this.hypWebsiteBinding.TabStop = true;
            this.hypWebsiteBinding.Text = "linkLabel1";
            this.hypWebsiteBinding.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.hypWebsiteBinding_LinkClicked);
            // 
            // lblNuFridgeServiceStatus
            // 
            this.lblNuFridgeServiceStatus.AutoSize = true;
            this.lblNuFridgeServiceStatus.Depth = 0;
            this.lblNuFridgeServiceStatus.Font = new System.Drawing.Font("Roboto", 11F);
            this.lblNuFridgeServiceStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblNuFridgeServiceStatus.Location = new System.Drawing.Point(213, 32);
            this.lblNuFridgeServiceStatus.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblNuFridgeServiceStatus.Name = "lblNuFridgeServiceStatus";
            this.lblNuFridgeServiceStatus.Size = new System.Drawing.Size(108, 19);
            this.lblNuFridgeServiceStatus.TabIndex = 4;
            this.lblNuFridgeServiceStatus.Text = "materialLabel1";
            // 
            // btnStopService
            // 
            this.btnStopService.Depth = 0;
            this.btnStopService.Enabled = false;
            this.btnStopService.Location = new System.Drawing.Point(3, 104);
            this.btnStopService.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnStopService.Name = "btnStopService";
            this.btnStopService.Primary = true;
            this.btnStopService.Size = new System.Drawing.Size(204, 33);
            this.btnStopService.TabIndex = 3;
            this.btnStopService.Text = "Stop";
            this.btnStopService.UseVisualStyleBackColor = true;
            this.btnStopService.Click += new System.EventHandler(this.btnStopService_Click);
            // 
            // btnRestartService
            // 
            this.btnRestartService.Depth = 0;
            this.btnRestartService.Enabled = false;
            this.btnRestartService.Location = new System.Drawing.Point(3, 65);
            this.btnRestartService.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRestartService.Name = "btnRestartService";
            this.btnRestartService.Primary = true;
            this.btnRestartService.Size = new System.Drawing.Size(204, 33);
            this.btnRestartService.TabIndex = 2;
            this.btnRestartService.Text = "Restart";
            this.btnRestartService.UseVisualStyleBackColor = true;
            this.btnRestartService.Click += new System.EventHandler(this.btnRestartService_Click);
            // 
            // btnStartService
            // 
            this.btnStartService.Depth = 0;
            this.btnStartService.Enabled = false;
            this.btnStartService.Location = new System.Drawing.Point(3, 26);
            this.btnStartService.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnStartService.Name = "btnStartService";
            this.btnStartService.Primary = true;
            this.btnStartService.Size = new System.Drawing.Size(204, 33);
            this.btnStartService.TabIndex = 1;
            this.btnStartService.Text = "Start";
            this.btnStartService.UseVisualStyleBackColor = true;
            this.btnStartService.Click += new System.EventHandler(this.btnStartService_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.btnRestoreDatabase);
            this.tabPage2.Controls.Add(this.materialLabel1);
            this.tabPage2.Controls.Add(this.btnBackupDatabase);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(568, 256);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Database";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnRestoreDatabase
            // 
            this.btnRestoreDatabase.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRestoreDatabase.Depth = 0;
            this.btnRestoreDatabase.Location = new System.Drawing.Point(3, 65);
            this.btnRestoreDatabase.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnRestoreDatabase.Name = "btnRestoreDatabase";
            this.btnRestoreDatabase.Primary = true;
            this.btnRestoreDatabase.Size = new System.Drawing.Size(204, 33);
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
            this.materialLabel1.Location = new System.Drawing.Point(213, 32);
            this.materialLabel1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel1.Name = "materialLabel1";
            this.materialLabel1.Size = new System.Drawing.Size(283, 19);
            this.materialLabel1.TabIndex = 5;
            this.materialLabel1.Text = "Backups do not include NuGet packages.";
            // 
            // btnBackupDatabase
            // 
            this.btnBackupDatabase.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBackupDatabase.Depth = 0;
            this.btnBackupDatabase.Location = new System.Drawing.Point(3, 26);
            this.btnBackupDatabase.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnBackupDatabase.Name = "btnBackupDatabase";
            this.btnBackupDatabase.Primary = true;
            this.btnBackupDatabase.Size = new System.Drawing.Size(204, 33);
            this.btnBackupDatabase.TabIndex = 2;
            this.btnBackupDatabase.Text = "Backup";
            this.btnBackupDatabase.UseVisualStyleBackColor = true;
            this.btnBackupDatabase.Click += new System.EventHandler(this.btnBackupDatabase_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.lblWhatsNew);
            this.tabPage3.Controls.Add(this.txtWhatsNew);
            this.tabPage3.Controls.Add(this.lblUpdatesAvailable);
            this.tabPage3.Controls.Add(this.btnUpdate);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(568, 256);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Updates";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // lblWhatsNew
            // 
            this.lblWhatsNew.AutoSize = true;
            this.lblWhatsNew.Depth = 0;
            this.lblWhatsNew.Font = new System.Drawing.Font("Roboto", 11F);
            this.lblWhatsNew.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblWhatsNew.Location = new System.Drawing.Point(-1, 75);
            this.lblWhatsNew.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblWhatsNew.Name = "lblWhatsNew";
            this.lblWhatsNew.Size = new System.Drawing.Size(85, 19);
            this.lblWhatsNew.TabIndex = 6;
            this.lblWhatsNew.Text = "What\'s new";
            this.lblWhatsNew.Visible = false;
            // 
            // txtWhatsNew
            // 
            this.txtWhatsNew.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtWhatsNew.Location = new System.Drawing.Point(3, 97);
            this.txtWhatsNew.Multiline = true;
            this.txtWhatsNew.Name = "txtWhatsNew";
            this.txtWhatsNew.Size = new System.Drawing.Size(562, 152);
            this.txtWhatsNew.TabIndex = 5;
            this.txtWhatsNew.Visible = false;
            // 
            // lblUpdatesAvailable
            // 
            this.lblUpdatesAvailable.AutoSize = true;
            this.lblUpdatesAvailable.Depth = 0;
            this.lblUpdatesAvailable.Font = new System.Drawing.Font("Roboto", 11F);
            this.lblUpdatesAvailable.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblUpdatesAvailable.Location = new System.Drawing.Point(213, 32);
            this.lblUpdatesAvailable.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblUpdatesAvailable.Name = "lblUpdatesAvailable";
            this.lblUpdatesAvailable.Size = new System.Drawing.Size(178, 19);
            this.lblUpdatesAvailable.TabIndex = 4;
            this.lblUpdatesAvailable.Text = "No updates are available.";
            // 
            // btnUpdate
            // 
            this.btnUpdate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUpdate.Depth = 0;
            this.btnUpdate.Location = new System.Drawing.Point(3, 26);
            this.btnUpdate.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Primary = true;
            this.btnUpdate.Size = new System.Drawing.Size(204, 33);
            this.btnUpdate.TabIndex = 3;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
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
            this.ClientSize = new System.Drawing.Size(600, 380);
            this.Controls.Add(this.materialTabControl1);
            this.Controls.Add(this.materialTabSelector1);
            this.MaximumSize = new System.Drawing.Size(600, 380);
            this.MinimumSize = new System.Drawing.Size(600, 380);
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
        private MaterialSkin.Controls.MaterialRaisedButton btnStartService;
        private MaterialSkin.Controls.MaterialRaisedButton btnStopService;
        private MaterialSkin.Controls.MaterialRaisedButton btnRestartService;
        private MaterialSkin.Controls.MaterialLabel lblNuFridgeServiceStatus;
        private System.ComponentModel.BackgroundWorker nufridgeServiceWorker;
        private System.Windows.Forms.TabPage tabPage2;
        private MaterialSkin.Controls.MaterialLabel materialLabel1;
        private MaterialSkin.Controls.MaterialRaisedButton btnBackupDatabase;
        private MaterialSkin.Controls.MaterialRaisedButton btnRestoreDatabase;
        private System.Windows.Forms.SaveFileDialog backupSaveFileDialog;
        private System.Windows.Forms.LinkLabel hypWebsiteBinding;
        private System.Windows.Forms.LinkLabel hypFeedBinding;
        private MaterialSkin.Controls.MaterialLabel materialLabel3;
        private MaterialSkin.Controls.MaterialLabel materialLabel2;
        private System.Windows.Forms.OpenFileDialog backupLoadFileDialog;
        private System.Windows.Forms.TabPage tabPage3;
        private MaterialSkin.Controls.MaterialRaisedButton btnUpdate;
        private MaterialSkin.Controls.MaterialLabel lblUpdatesAvailable;
        private MaterialSkin.Controls.MaterialLabel lblWhatsNew;
        private System.Windows.Forms.TextBox txtWhatsNew;
    }
}

