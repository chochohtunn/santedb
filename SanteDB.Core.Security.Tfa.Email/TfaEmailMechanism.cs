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
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security.Tfa.Email.Configuration;
using SanteDB.Core.Security.Tfa.Email.Resources;
using SanteDB.Core.Security.Tfa.Email.Template;
using SanteDB.Core.Services;
using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace SanteDB.Core.Security.Tfa.Email
{
    /// <summary>
    /// Represents a TFA mechanism which can send/receive TFA requests via e-mail
    /// </summary>
    public class TfaEmailMechanism : ITfaMechanism
	{
		// Configuration
		private TfaEmailMechanismConfigurationSection m_configuration = ApplicationServiceContext.Current.GetService<IConfigurationManager>().GetSection<TfaEmailMechanismConfigurationSection>();

		// Tracer
		private Tracer m_tracer = new Tracer("SanteDB.Core.Security.Tfa.Email");

		/// <summary>
		/// Gets the challenge text
		/// </summary>
		public string Description
		{
			get
			{
				return Strings.challenge_text;
			}
		}

		/// <summary>
		/// Get the identifier for the challenge
		/// </summary>
		public Guid Id
		{
			get
			{
				return Guid.Parse("D919457D-E015-435C-BD35-42E425E2C60C");
			}
		}

		/// <summary>
		/// Gets the name of the mechanism
		/// </summary>
		public string Name
		{
			get
			{
				return Strings.mechanism_name;
			}
		}

		/// <summary>
		/// Send the mechanism
		/// </summary>
		public void Send(SecurityUser user, string challengeResponse, string tfaSecret)
		{
			if (user == null)
				throw new ArgumentNullException(nameof(user));
			else if (challengeResponse == null)
				throw new ArgumentNullException(nameof(challengeResponse));

			// Verify
			if (!user.Email.StartsWith(challengeResponse + "@"))
			{
				this.m_tracer.TraceEvent(EventLevel.Warning, $"User {user.UserName} reset challenge failed");
			}
			else
			{
				// Get preferred language for the user
				var securityService = ApplicationServiceContext.Current.GetService<IRepositoryService<UserEntity>>();
				var userEntity = securityService?.Find(o => o.SecurityUserKey == user.Key).FirstOrDefault();

                this.m_tracer.TraceEvent(EventLevel.Informational,  "Password reset has been requested for {0}", userEntity.Key);

				// We want to send the data
				var templateConfiguration = this.m_configuration.Templates.FirstOrDefault(o => o.Language == (userEntity?.LanguageCommunication?.FirstOrDefault(l => l.IsPreferred)?.LanguageCode ?? CultureInfo.CurrentCulture.TwoLetterISOLanguageName));
				EmailTemplate template = null;
				if (templateConfiguration != null)
					template = EmailTemplate.Load(templateConfiguration.TemplateDefinitionFile);
				if (template == null)
					template = new EmailTemplate()
					{
						From = this.m_configuration.Smtp.From,
						Body = Strings.default_body,
						Subject = Strings.default_subject
					};

				// Setup message
				MailMessage resetMessage = new MailMessage(template.From, user.Email);
				resetMessage.Subject = template.Subject;
				resetMessage.Body = String.Format(template.Body, tfaSecret);

                this.m_tracer.TraceInfo("Sending password reset to {0} from {1}", user.Email, template.From);
				try
				{
					SmtpClient smtpClient = new SmtpClient(this.m_configuration.Smtp.Server.Host, this.m_configuration.Smtp.Server.Port);
					smtpClient.UseDefaultCredentials = String.IsNullOrEmpty(this.m_configuration.Smtp.Username);
					smtpClient.EnableSsl = this.m_configuration.Smtp.Ssl;
					if (!(smtpClient.UseDefaultCredentials))
						smtpClient.Credentials = new NetworkCredential(this.m_configuration.Smtp.Username, this.m_configuration.Smtp.Password);
					smtpClient.SendCompleted += (o, e) =>
					{
                        this.m_tracer.TraceInfo("Successfully sent message to {0}", resetMessage.To);
						if (e.Error != null)
							this.m_tracer.TraceEvent(EventLevel.Error,  e.Error.ToString());
						(o as IDisposable).Dispose();
					};
                    this.m_tracer.TraceInfo("Sending password reset email message to {0}", resetMessage.To);
                    smtpClient.Send(resetMessage);
				}
				catch (Exception e)
				{
					this.m_tracer.TraceEvent(EventLevel.Error,  $"Error sending TFA secret: {e.Message}\r\n{e.ToString()}");
				}
			}
		}
	}
}