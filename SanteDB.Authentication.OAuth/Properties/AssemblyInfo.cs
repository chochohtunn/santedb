﻿/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 *
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
 * User: justin
 * Date: 2018-6-22
 */
using SanteDB.Authentication.OAuth2;
using SanteDB.Core.Attributes;
using SanteDB.Core.Configuration;
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SanteDB.Authentication.OAuth2")]
[assembly: AssemblyDescription("")]
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
[assembly: Guid("a3975c6f-4fdb-43be-81ab-3357b86173f1")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.111.0.*")][assembly: AssemblyInformationalVersion("Iqaluit")]
[assembly: AssemblyVersion("1.111.0.*")]
[assembly: AssemblyInformationalVersion("Iqaluit")]
[assembly: AssemblyFileVersion("1.111.0.0")]

[assembly: Plugin(EnableByDefault = true, Environment = PluginEnvironment.Server, Group = FeatureGroup.Security)]
[assembly: PluginDependency("SanteDB.Core, Version=1.111.0.0")]
[assembly: PluginTraceSource(OAuthConstants.TraceSourceName)]