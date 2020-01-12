﻿namespace SanteDB.Configurator
{
    partial class frmMain
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.tbMain = new System.Windows.Forms.TabControl();
            this.tpSettings = new System.Windows.Forms.TabPage();
            this.spMainControl = new System.Windows.Forms.SplitContainer();
            this.trvFeatures = new System.Windows.Forms.TreeView();
            this.imlMain = new System.Windows.Forms.ImageList(this.components);
            this.pnlInfo = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rtbLicense = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblInstanceName = new System.Windows.Forms.Label();
            this.lblSupport = new System.Windows.Forms.LinkLabel();
            this.lblCopyright = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblApplication = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pnlFeature = new System.Windows.Forms.Panel();
            this.btnDisable = new System.Windows.Forms.LinkLabel();
            this.btnEnable = new System.Windows.Forms.LinkLabel();
            this.tcSettings = new System.Windows.Forms.TabControl();
            this.tpSetting = new System.Windows.Forms.TabPage();
            this.pgConfiguration = new SanteDB.Configuration.Controls.PropertyGridEx();
            this.lblDescription = new System.Windows.Forms.Label();
            this.lblDisabled = new System.Windows.Forms.Label();
            this.lblEnabled = new System.Windows.Forms.Label();
            this.lblSelectedOption = new System.Windows.Forms.Label();
            this.tpAdvanced = new System.Windows.Forms.TabPage();
            this.spEditor = new System.Windows.Forms.SplitContainer();
            this.lsvConfigSections = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pbEditor = new SanteDB.Configuration.Controls.PropertyGridEx();
            this.label2 = new System.Windows.Forms.Label();
            this.tspMain = new System.Windows.Forms.ToolStrip();
            this.btnApply = new System.Windows.Forms.ToolStripButton();
            this.btnRestart = new System.Windows.Forms.ToolStripButton();
            this.tbMain.SuspendLayout();
            this.tpSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spMainControl)).BeginInit();
            this.spMainControl.Panel1.SuspendLayout();
            this.spMainControl.Panel2.SuspendLayout();
            this.spMainControl.SuspendLayout();
            this.pnlInfo.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.pnlFeature.SuspendLayout();
            this.tcSettings.SuspendLayout();
            this.tpSetting.SuspendLayout();
            this.tpAdvanced.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spEditor)).BeginInit();
            this.spEditor.Panel1.SuspendLayout();
            this.spEditor.Panel2.SuspendLayout();
            this.spEditor.SuspendLayout();
            this.tspMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbMain
            // 
            this.tbMain.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tbMain.Controls.Add(this.tpSettings);
            this.tbMain.Controls.Add(this.tpAdvanced);
            this.tbMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbMain.ImageList = this.imlMain;
            this.tbMain.Location = new System.Drawing.Point(0, 38);
            this.tbMain.Multiline = true;
            this.tbMain.Name = "tbMain";
            this.tbMain.SelectedIndex = 0;
            this.tbMain.Size = new System.Drawing.Size(818, 419);
            this.tbMain.TabIndex = 0;
            // 
            // tpSettings
            // 
            this.tpSettings.Controls.Add(this.spMainControl);
            this.tpSettings.ImageIndex = 0;
            this.tpSettings.Location = new System.Drawing.Point(4, 4);
            this.tpSettings.Name = "tpSettings";
            this.tpSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tpSettings.Size = new System.Drawing.Size(810, 392);
            this.tpSettings.TabIndex = 0;
            this.tpSettings.Text = "Settings";
            this.tpSettings.UseVisualStyleBackColor = true;
            // 
            // spMainControl
            // 
            this.spMainControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spMainControl.Location = new System.Drawing.Point(3, 3);
            this.spMainControl.Name = "spMainControl";
            // 
            // spMainControl.Panel1
            // 
            this.spMainControl.Panel1.Controls.Add(this.trvFeatures);
            // 
            // spMainControl.Panel2
            // 
            this.spMainControl.Panel2.Controls.Add(this.pnlInfo);
            this.spMainControl.Panel2.Controls.Add(this.pnlFeature);
            this.spMainControl.Size = new System.Drawing.Size(804, 386);
            this.spMainControl.SplitterDistance = 210;
            this.spMainControl.TabIndex = 0;
            // 
            // trvFeatures
            // 
            this.trvFeatures.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trvFeatures.FullRowSelect = true;
            this.trvFeatures.HideSelection = false;
            this.trvFeatures.ImageIndex = 0;
            this.trvFeatures.ImageList = this.imlMain;
            this.trvFeatures.Location = new System.Drawing.Point(0, 0);
            this.trvFeatures.Name = "trvFeatures";
            this.trvFeatures.SelectedImageIndex = 0;
            this.trvFeatures.Size = new System.Drawing.Size(210, 386);
            this.trvFeatures.TabIndex = 0;
            this.trvFeatures.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.trvFeatures_AfterSelect);
            // 
            // imlMain
            // 
            this.imlMain.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imlMain.ImageStream")));
            this.imlMain.TransparentColor = System.Drawing.Color.Magenta;
            this.imlMain.Images.SetKeyName(0, "ServerSettings_16x_24.bmp");
            this.imlMain.Images.SetKeyName(1, "SettingsGroup_16x_24.bmp");
            this.imlMain.Images.SetKeyName(2, "SettingsPanel_16x_24.bmp");
            this.imlMain.Images.SetKeyName(3, "XMLIntellisenseElement_16x_24.bmp");
            this.imlMain.Images.SetKeyName(4, "StatusWarning_cyan_12x_16x_24.bmp");
            this.imlMain.Images.SetKeyName(5, "StatusInfoTip_16x_24.bmp");
            this.imlMain.Images.SetKeyName(6, "GroupByType_16x_24.bmp");
            this.imlMain.Images.SetKeyName(7, "Edit_16x_24.bmp");
            this.imlMain.Images.SetKeyName(8, "Cancel_grey_16x_24.bmp");
            this.imlMain.Images.SetKeyName(9, "Checkmark_16xMD_24.bmp");
            this.imlMain.Images.SetKeyName(10, "PartiallyComplete_16x_24.bmp");
            this.imlMain.Images.SetKeyName(11, "PropertyShortcut_16x_24.bmp");
            // 
            // pnlInfo
            // 
            this.pnlInfo.Controls.Add(this.panel1);
            this.pnlInfo.Controls.Add(this.lblInstanceName);
            this.pnlInfo.Controls.Add(this.lblSupport);
            this.pnlInfo.Controls.Add(this.lblCopyright);
            this.pnlInfo.Controls.Add(this.lblVersion);
            this.pnlInfo.Controls.Add(this.lblApplication);
            this.pnlInfo.Controls.Add(this.pictureBox1);
            this.pnlInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlInfo.Location = new System.Drawing.Point(0, 0);
            this.pnlInfo.Name = "pnlInfo";
            this.pnlInfo.Size = new System.Drawing.Size(590, 386);
            this.pnlInfo.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.rtbLicense);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(4, 97);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(586, 286);
            this.panel1.TabIndex = 9;
            // 
            // rtbLicense
            // 
            this.rtbLicense.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtbLicense.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbLicense.Location = new System.Drawing.Point(0, 22);
            this.rtbLicense.Name = "rtbLicense";
            this.rtbLicense.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.rtbLicense.Size = new System.Drawing.Size(586, 264);
            this.rtbLicense.TabIndex = 10;
            this.rtbLicense.Text = "";
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(3);
            this.label1.Size = new System.Drawing.Size(586, 22);
            this.label1.TabIndex = 9;
            this.label1.Text = "License Notice(s)";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInstanceName
            // 
            this.lblInstanceName.AutoSize = true;
            this.lblInstanceName.Location = new System.Drawing.Point(100, 59);
            this.lblInstanceName.Name = "lblInstanceName";
            this.lblInstanceName.Size = new System.Drawing.Size(79, 13);
            this.lblInstanceName.TabIndex = 6;
            this.lblInstanceName.Text = "Instance Name";
            // 
            // lblSupport
            // 
            this.lblSupport.AutoSize = true;
            this.lblSupport.Location = new System.Drawing.Point(100, 75);
            this.lblSupport.Name = "lblSupport";
            this.lblSupport.Size = new System.Drawing.Size(178, 13);
            this.lblSupport.TabIndex = 5;
            this.lblSupport.TabStop = true;
            this.lblSupport.Text = "Support (https://help.santesuite.org)";
            this.lblSupport.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblSupport_LinkClicked);
            // 
            // lblCopyright
            // 
            this.lblCopyright.AutoSize = true;
            this.lblCopyright.Location = new System.Drawing.Point(100, 43);
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Size = new System.Drawing.Size(51, 13);
            this.lblCopyright.TabIndex = 4;
            this.lblCopyright.Text = "Copyright";
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(100, 27);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(42, 13);
            this.lblVersion.TabIndex = 3;
            this.lblVersion.Text = "Version";
            // 
            // lblApplication
            // 
            this.lblApplication.AutoSize = true;
            this.lblApplication.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblApplication.Location = new System.Drawing.Point(99, 4);
            this.lblApplication.Name = "lblApplication";
            this.lblApplication.Size = new System.Drawing.Size(177, 20);
            this.lblApplication.TabIndex = 2;
            this.lblApplication.Text = "SanteSuite SanteDB";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::SanteDB.Configurator.Properties.Resources.icon;
            this.pictureBox1.Location = new System.Drawing.Point(4, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(89, 87);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // pnlFeature
            // 
            this.pnlFeature.Controls.Add(this.btnDisable);
            this.pnlFeature.Controls.Add(this.btnEnable);
            this.pnlFeature.Controls.Add(this.tcSettings);
            this.pnlFeature.Controls.Add(this.lblDescription);
            this.pnlFeature.Controls.Add(this.lblDisabled);
            this.pnlFeature.Controls.Add(this.lblEnabled);
            this.pnlFeature.Controls.Add(this.lblSelectedOption);
            this.pnlFeature.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFeature.Location = new System.Drawing.Point(0, 0);
            this.pnlFeature.Name = "pnlFeature";
            this.pnlFeature.Size = new System.Drawing.Size(590, 386);
            this.pnlFeature.TabIndex = 0;
            // 
            // btnDisable
            // 
            this.btnDisable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDisable.AutoSize = true;
            this.btnDisable.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.btnDisable.Location = new System.Drawing.Point(540, 27);
            this.btnDisable.Name = "btnDisable";
            this.btnDisable.Size = new System.Drawing.Size(42, 13);
            this.btnDisable.TabIndex = 13;
            this.btnDisable.TabStop = true;
            this.btnDisable.Text = "Disable";
            this.btnDisable.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.btnDisable_LinkClicked);
            // 
            // btnEnable
            // 
            this.btnEnable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEnable.AutoSize = true;
            this.btnEnable.BackColor = System.Drawing.SystemColors.Info;
            this.btnEnable.Location = new System.Drawing.Point(542, 27);
            this.btnEnable.Name = "btnEnable";
            this.btnEnable.Size = new System.Drawing.Size(40, 13);
            this.btnEnable.TabIndex = 11;
            this.btnEnable.TabStop = true;
            this.btnEnable.Text = "Enable";
            this.btnEnable.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.btnEnable_LinkClicked);
            // 
            // tcSettings
            // 
            this.tcSettings.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tcSettings.Controls.Add(this.tpSetting);
            this.tcSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcSettings.ImageList = this.imlMain;
            this.tcSettings.Location = new System.Drawing.Point(0, 100);
            this.tcSettings.Name = "tcSettings";
            this.tcSettings.SelectedIndex = 0;
            this.tcSettings.Size = new System.Drawing.Size(590, 286);
            this.tcSettings.TabIndex = 8;
            // 
            // tpSetting
            // 
            this.tpSetting.Controls.Add(this.pgConfiguration);
            this.tpSetting.ImageIndex = 11;
            this.tpSetting.Location = new System.Drawing.Point(4, 4);
            this.tpSetting.Name = "tpSetting";
            this.tpSetting.Padding = new System.Windows.Forms.Padding(3);
            this.tpSetting.Size = new System.Drawing.Size(582, 259);
            this.tpSetting.TabIndex = 0;
            this.tpSetting.Text = "Configuration";
            this.tpSetting.UseVisualStyleBackColor = true;
            // 
            // pgConfiguration
            // 
            this.pgConfiguration.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgConfiguration.Location = new System.Drawing.Point(3, 3);
            this.pgConfiguration.Name = "pgConfiguration";
            this.pgConfiguration.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.pgConfiguration.Size = new System.Drawing.Size(576, 253);
            this.pgConfiguration.TabIndex = 1;
            this.pgConfiguration.ToolbarVisible = false;
            // 
            // lblDescription
            // 
            this.lblDescription.BackColor = System.Drawing.SystemColors.Control;
            this.lblDescription.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDescription.ForeColor = System.Drawing.SystemColors.InfoText;
            this.lblDescription.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.lblDescription.ImageIndex = 5;
            this.lblDescription.ImageList = this.imlMain;
            this.lblDescription.Location = new System.Drawing.Point(0, 74);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Padding = new System.Windows.Forms.Padding(3);
            this.lblDescription.Size = new System.Drawing.Size(590, 26);
            this.lblDescription.TabIndex = 14;
            // 
            // lblDisabled
            // 
            this.lblDisabled.BackColor = System.Drawing.SystemColors.Info;
            this.lblDisabled.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDisabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDisabled.ForeColor = System.Drawing.SystemColors.InfoText;
            this.lblDisabled.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.lblDisabled.ImageIndex = 4;
            this.lblDisabled.ImageList = this.imlMain;
            this.lblDisabled.Location = new System.Drawing.Point(0, 48);
            this.lblDisabled.Name = "lblDisabled";
            this.lblDisabled.Padding = new System.Windows.Forms.Padding(3);
            this.lblDisabled.Size = new System.Drawing.Size(590, 26);
            this.lblDisabled.TabIndex = 10;
            this.lblDisabled.Text = "      This feature is disabled.";
            // 
            // lblEnabled
            // 
            this.lblEnabled.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.lblEnabled.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblEnabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEnabled.ForeColor = System.Drawing.SystemColors.InfoText;
            this.lblEnabled.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.lblEnabled.ImageIndex = 9;
            this.lblEnabled.ImageList = this.imlMain;
            this.lblEnabled.Location = new System.Drawing.Point(0, 22);
            this.lblEnabled.Name = "lblEnabled";
            this.lblEnabled.Padding = new System.Windows.Forms.Padding(3);
            this.lblEnabled.Size = new System.Drawing.Size(590, 26);
            this.lblEnabled.TabIndex = 12;
            this.lblEnabled.Text = "       This feature is enabled";
            // 
            // lblSelectedOption
            // 
            this.lblSelectedOption.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.lblSelectedOption.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSelectedOption.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSelectedOption.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblSelectedOption.Location = new System.Drawing.Point(0, 0);
            this.lblSelectedOption.Name = "lblSelectedOption";
            this.lblSelectedOption.Padding = new System.Windows.Forms.Padding(3);
            this.lblSelectedOption.Size = new System.Drawing.Size(590, 22);
            this.lblSelectedOption.TabIndex = 9;
            this.lblSelectedOption.Text = "Configuration";
            // 
            // tpAdvanced
            // 
            this.tpAdvanced.Controls.Add(this.spEditor);
            this.tpAdvanced.Controls.Add(this.label2);
            this.tpAdvanced.ImageIndex = 1;
            this.tpAdvanced.Location = new System.Drawing.Point(4, 4);
            this.tpAdvanced.Name = "tpAdvanced";
            this.tpAdvanced.Padding = new System.Windows.Forms.Padding(3);
            this.tpAdvanced.Size = new System.Drawing.Size(810, 392);
            this.tpAdvanced.TabIndex = 1;
            this.tpAdvanced.Text = "Setting Editor";
            this.tpAdvanced.UseVisualStyleBackColor = true;
            // 
            // spEditor
            // 
            this.spEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.spEditor.Location = new System.Drawing.Point(3, 29);
            this.spEditor.Name = "spEditor";
            // 
            // spEditor.Panel1
            // 
            this.spEditor.Panel1.Controls.Add(this.lsvConfigSections);
            // 
            // spEditor.Panel2
            // 
            this.spEditor.Panel2.Controls.Add(this.pbEditor);
            this.spEditor.Size = new System.Drawing.Size(804, 360);
            this.spEditor.SplitterDistance = 267;
            this.spEditor.TabIndex = 0;
            // 
            // lsvConfigSections
            // 
            this.lsvConfigSections.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lsvConfigSections.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lsvConfigSections.FullRowSelect = true;
            this.lsvConfigSections.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lsvConfigSections.HideSelection = false;
            this.lsvConfigSections.Location = new System.Drawing.Point(0, 0);
            this.lsvConfigSections.MultiSelect = false;
            this.lsvConfigSections.Name = "lsvConfigSections";
            this.lsvConfigSections.ShowGroups = false;
            this.lsvConfigSections.Size = new System.Drawing.Size(267, 360);
            this.lsvConfigSections.SmallImageList = this.imlMain;
            this.lsvConfigSections.TabIndex = 0;
            this.lsvConfigSections.UseCompatibleStateImageBehavior = false;
            this.lsvConfigSections.View = System.Windows.Forms.View.Details;
            this.lsvConfigSections.SelectedIndexChanged += new System.EventHandler(this.lsvConfigSections_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Section";
            this.columnHeader1.Width = 239;
            // 
            // pbEditor
            // 
            this.pbEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbEditor.Location = new System.Drawing.Point(0, 0);
            this.pbEditor.Name = "pbEditor";
            this.pbEditor.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.pbEditor.Size = new System.Drawing.Size(533, 360);
            this.pbEditor.TabIndex = 0;
            this.pbEditor.ToolbarVisible = false;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.SystemColors.Info;
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.InfoText;
            this.label2.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label2.ImageIndex = 5;
            this.label2.ImageList = this.imlMain;
            this.label2.Location = new System.Drawing.Point(3, 3);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(3);
            this.label2.Size = new System.Drawing.Size(804, 26);
            this.label2.TabIndex = 1;
            this.label2.Text = "      Editing the values in this panel can damage your installation of SanteDB";
            // 
            // tspMain
            // 
            this.tspMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tspMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnApply,
            this.btnRestart});
            this.tspMain.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.tspMain.Location = new System.Drawing.Point(0, 0);
            this.tspMain.Name = "tspMain";
            this.tspMain.Size = new System.Drawing.Size(818, 38);
            this.tspMain.TabIndex = 1;
            this.tspMain.Text = "toolStrip1";
            // 
            // btnApply
            // 
            this.btnApply.Image = ((System.Drawing.Image)(resources.GetObject("btnApply.Image")));
            this.btnApply.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(91, 35);
            this.btnApply.Text = "Apply Changes";
            this.btnApply.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnRestart
            // 
            this.btnRestart.Image = ((System.Drawing.Image)(resources.GetObject("btnRestart.Image")));
            this.btnRestart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(87, 35);
            this.btnRestart.Text = "Restart Service";
            this.btnRestart.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(818, 457);
            this.Controls.Add(this.tbMain);
            this.Controls.Add(this.tspMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SanteDB Configuration Editor";
            this.tbMain.ResumeLayout(false);
            this.tpSettings.ResumeLayout(false);
            this.spMainControl.Panel1.ResumeLayout(false);
            this.spMainControl.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spMainControl)).EndInit();
            this.spMainControl.ResumeLayout(false);
            this.pnlInfo.ResumeLayout(false);
            this.pnlInfo.PerformLayout();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.pnlFeature.ResumeLayout(false);
            this.pnlFeature.PerformLayout();
            this.tcSettings.ResumeLayout(false);
            this.tpSetting.ResumeLayout(false);
            this.tpAdvanced.ResumeLayout(false);
            this.spEditor.Panel1.ResumeLayout(false);
            this.spEditor.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.spEditor)).EndInit();
            this.spEditor.ResumeLayout(false);
            this.tspMain.ResumeLayout(false);
            this.tspMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tbMain;
        private System.Windows.Forms.TabPage tpSettings;
        private System.Windows.Forms.TabPage tpAdvanced;
        private System.Windows.Forms.ImageList imlMain;
        private System.Windows.Forms.ToolStrip tspMain;
        private System.Windows.Forms.ToolStripButton btnApply;
        private System.Windows.Forms.ToolStripButton btnRestart;
        private System.Windows.Forms.SplitContainer spEditor;
        private System.Windows.Forms.ListView lsvConfigSections;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private Configuration.Controls.PropertyGridEx pbEditor;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.SplitContainer spMainControl;
        private System.Windows.Forms.TreeView trvFeatures;
        private System.Windows.Forms.Panel pnlFeature;
        private System.Windows.Forms.LinkLabel btnDisable;
        private System.Windows.Forms.LinkLabel btnEnable;
        private System.Windows.Forms.TabControl tcSettings;
        private System.Windows.Forms.TabPage tpSetting;
        private Configuration.Controls.PropertyGridEx pgConfiguration;
        private System.Windows.Forms.Label lblEnabled;
        private System.Windows.Forms.Label lblDisabled;
        private System.Windows.Forms.Label lblSelectedOption;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Panel pnlInfo;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.RichTextBox rtbLicense;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblInstanceName;
        private System.Windows.Forms.LinkLabel lblSupport;
        private System.Windows.Forms.Label lblCopyright;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblApplication;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

