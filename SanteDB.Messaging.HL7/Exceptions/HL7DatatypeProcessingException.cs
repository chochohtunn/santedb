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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Messaging.HL7.Exceptions
{
    /// <summary>
    /// Assigning authority was not found
    /// </summary>
    public class HL7DatatypeProcessingException : Exception
    {
      
        /// <summary>
        /// Gets the component
        /// </summary>
        public Int32 Component { get; }
        
        /// <summary>
        /// Creates a new HL7 processing exception
        /// </summary>
        public HL7DatatypeProcessingException(String message,  Int32 component) : this(message, component, null)
        {
            this.Component = component;
        }

        /// <summary>
        /// Creates a new HL7 processing exception
        /// </summary>
        public HL7DatatypeProcessingException(String message, Int32 component, Exception cause) : base(message, cause)
        {
            this.Component = component;
        }

    }
}
