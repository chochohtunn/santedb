﻿/*
 * Based on OpenIZ - Based on OpenIZ, Copyright (C) 2015 - 2019 Mohawk College of Applied Arts and Technology
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
using SanteDB.Core;
using SanteDB.Core.Attributes;
using SanteDB.Core.Configuration;
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SanteDB.Core")]
[assembly: AssemblyDescription("OpenImmuinize Core")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Mohawk College of Applied Arts and Technology")]
[assembly: AssemblyProduct("SanteDB (http://SanteDB.org)")]
[assembly: AssemblyCopyright("Copyright (C) 2015-2018, Mohawk College of Applied Arts and Technology")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("db9bc05e-45f1-4f96-a161-f36bdecaf566")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("2.0.22.*")][assembly: AssemblyInformationalVersion("2.0.22")]
[assembly: AssemblyVersion("2.0.22.*")]
[assembly: AssemblyInformationalVersion("Jarvis")]
[assembly: AssemblyFileVersion("2.0.22.0")]

[assembly: Plugin(EnableByDefault = true, Environment = PluginEnvironment.Server, Group = FeatureGroup.System)]
[assembly: PluginTraceSource(SanteDBConstants.WcfTraceSourceName)]
[assembly: PluginTraceSource(SanteDBConstants.DataTraceSourceName)]
[assembly: PluginTraceSource(SanteDBConstants.MapTraceSourceName)]
[assembly: PluginTraceSource(SanteDBConstants.SecurityTraceSourceName)]
[assembly: PluginTraceSource(SanteDBConstants.ServiceTraceSourceName)]
[assembly: PluginTraceSource(SanteDBConstants.ServiceTraceSourceName + ".AppletManager")]
[assembly: PluginTraceSource(SanteDBConstants.QueueTraceSourceName)]
