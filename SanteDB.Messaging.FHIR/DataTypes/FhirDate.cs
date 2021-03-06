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
using System.Xml;
using System.Xml.Serialization;

namespace SanteDB.Messaging.FHIR.DataTypes
{
    /// <summary>
    /// Date precisions
    /// </summary>
    public enum DatePrecision
    {
        /// <summary>
        /// Unspecified precision
        /// </summary>
        Unspecified = 0,
        /// <summary>
        /// Precise to year
        /// </summary>
        Year = 4,
        /// <summary>
        /// Preice to month
        /// </summary>
        Month = 7, 
        /// <summary>
        /// Precise to day
        /// </summary>
        Day = 10,
        /// <summary>
        /// Full date/time precision
        /// </summary>
        Full = 28
    }

    /// <summary>
    /// Date
    /// </summary>
    [XmlType("dateTime", Namespace = "http://hl7.org/fhir")]
    public class FhirDateTime : FhirString
    {

        /// <summary>
        /// Default ctor
        /// </summary>
        public FhirDateTime() { }

        /// <summary>
        /// Create a date 
        /// </summary>
        public FhirDateTime(DateTime value)
            : base()
        {
            this.DateValue = value;
        }

        /// <summary>
        /// Gets the precision of the date
        /// </summary>
        [XmlIgnore]
        public DatePrecision Precision { get; set; }

        /// <summary>
        /// Start time
        /// </summary>
        [XmlIgnore()]
        public DateTime? DateValue
        {
            get;
            set;
        }

        /// <summary>
        /// Convert this date to a date
        /// </summary>
        public static implicit operator DateTime(FhirDateTime v)
        {
            return v.DateValue.Value;
        }

        /// <summary>
        /// Convert this date to a date
        /// </summary>
        public static implicit operator FhirDateTime(DateTime v)
        {
            return new FhirDateTime(v) ;
        }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        [XmlIgnore]
        public override string Value
        {
            get
            {

                string dateFormat = @"yyyy-MM-dd\THH:mm:ss.ffffzzz";
                if (this.Precision == DatePrecision.Unspecified)
                    this.Precision = DatePrecision.Full;
                dateFormat = dateFormat.Substring(0, (int)this.Precision);
                return this.DateValue.Value.ToString(dateFormat);
            }
            set
            {
                if (value != null)
                {
                    this.Precision = (DatePrecision)value.Length;
                    if (this.Precision > DatePrecision.Full)
                        this.Precision = DatePrecision.Full;

                    // Correct parse
                    
                    if (this.Precision == DatePrecision.Year)
                        this.DateValue = XmlConvert.ToDateTime(value, "yyyy");
                    else
                        this.DateValue = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Unspecified);

                }
                else
                    this.DateValue = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Unspecified);
            }
        }

        /// <summary>
        /// Date string
        /// </summary>
        public override string ToString()
        {
            return this.XmlValue;
        }
    }

    /// <summary>
    /// Date only
    /// </summary>
    [XmlType("date", Namespace = "http://hl7.org/fhir")]
    public class FhirDate : FhirDateTime
    {
        /// <summary>
        /// reate FHIR date
        /// </summary>
        public FhirDate()
        {

        }

        /// <summary>
        /// Create FHIR date
        /// </summary>
        public FhirDate(DateTime dt) : base(dt)
        {

        }
        /// <summary>
        /// Convert this date to a date
        /// </summary>
        public static implicit operator DateTime(FhirDate v)
        {
            return v.DateValue.Value;
        }

        /// <summary>
        /// Convert this date to a date
        /// </summary>
        public static implicit operator FhirDate(DateTime v)
        {
            return new FhirDate(v);
        }

        /// <summary>
        /// Only date is permitted
        /// </summary>
        [XmlIgnore]
        public override string Value
        {
            get
            {
                if (base.Precision > DatePrecision.Day)
                    this.Precision = DatePrecision.Day;
                return base.Value;
            }
            set
            {
                base.Value = value;
                if (base.Precision > DatePrecision.Day)
                    this.Precision = DatePrecision.Day;
            }
        }
    }
}
