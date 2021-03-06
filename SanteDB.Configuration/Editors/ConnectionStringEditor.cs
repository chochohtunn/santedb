﻿/*
 * Based on OpenIZ - Copyright 2015-2019 Mohawk College of Applied Arts and Technology
 * Portions Copyright 2019-2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE)
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej (Justin Fyfe)
 * Date: 2019-11-27
 */
using SanteDB.Configuration.Controls;
using SanteDB.Core;
using SanteDB.Core.Configuration.Data;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace SanteDB.Configuration.Editors
{

    /// <summary>
    /// Creates a database name editor
    /// </summary>
    public class ConnectionStringEditor : UITypeEditor
    {

        /// <summary>
        /// Edit the value
        /// </summary>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                var winService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                var list = new ListBox();
                list.Click += (o, e) => winService.CloseDropDown();

                // Get the databases
                try
                {
                    list.Items.AddRange(ConfigurationContext.Current.Configuration.GetSection<DataConfigurationSection>().ConnectionString.Select(o => new ConnectionStringWrapper(o)).OfType<Object>().ToArray());
                    list.Items.Add("New...");
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Error retrieving connection strings: {e.Message}");
                    return value;
                }

                winService.DropDownControl(list);

                if (list.SelectedItem != null)
                {
                    if(list.SelectedItem is ConnectionStringWrapper)
                        return (list.SelectedItem as ConnectionStringWrapper)?.Name;
                    else
                    {
                        var newConnectionString = new frmConnectionString();
                        if(newConnectionString.ShowDialog() == DialogResult.OK)
                        {
                            ConfigurationContext.Current.Configuration.GetSection<DataConfigurationSection>().ConnectionString.Add(newConnectionString.ConnectionString);
                            return newConnectionString.ConnectionString.Name;
                        }
                    } 
                }
            }
            return value;

        }

        /// <summary>
        /// Get the edit stype
        /// </summary>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }
    }

    /// <summary>
    /// Represents a display wrapper for a connection string
    /// </summary>
    internal class ConnectionStringWrapper
    {
        // The connection string
        private ConnectionString m_connectionString;

        /// <summary>
        /// Create a new connection string wrapper
        /// </summary>
        public ConnectionStringWrapper(ConnectionString cstr)
        {
            this.m_connectionString = cstr;
        }

        /// <summary>
        /// Connection string name
        /// </summary>
        public override string ToString() => $"{this.m_connectionString.Name} ({this.m_connectionString.ToString()})";

        /// <summary>
        /// Gets the name of the connection string
        /// </summary>
        public String Name => this.m_connectionString.Name;
    }
}
