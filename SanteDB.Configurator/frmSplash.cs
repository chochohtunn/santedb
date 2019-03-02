﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SanteDB.Configurator
{
    public partial class frmSplash : Form
    {
        public frmSplash()
        {
            InitializeComponent();
            lblCopyright.Text = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
            lblVersion.Text = $"v.{Assembly.GetEntryAssembly().GetName().Version} ({Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion})";
        }

        /// <summary>
        /// Notify status
        /// </summary>
        /// <param name="statusText"></param>
        /// <param name="status"></param>
        public void NotifyStatus(String statusText, float status)
        {
            if (status < 0)
            {
                this.pbStatus.Style = ProgressBarStyle.Marquee;
                this.pbStatus.Value = 100;
            }
            else
            {
                this.pbStatus.Style = ProgressBarStyle.Blocks;
                this.pbStatus.Value = (int)(status * this.pbStatus.Maximum);
            }
            this.lblStatus.Text = statusText;
            Application.DoEvents();
        }
    }
}