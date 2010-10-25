﻿using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using ICSharpCode.SharpZipLib.Zip;
using System.Text.RegularExpressions;
using System.Text;
using NearForums.Validation;
using NearForums.ServiceClient;
using NearForums.Web.Controllers.Filters;

using System.Configuration;
using System.Web.Configuration;
using System.Net.Configuration;
using NearForums.Web.Controllers.Helpers;
using DotNetOpenAuth.OAuth.ChannelElements;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.Messages;
using NearForums.Web.Controllers.Helpers.OAuth;
using System.Net;
using NearForums.Web.Extensions;

namespace NearForums.Web.Controllers
{
	public class AdminController : BaseController
	{
		#region Templates
		#region Add template
		[RequireAuthorization(UserGroup.Admin)]
		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult AddTemplate()
		{
			return View();
		}


		[RequireAuthorization(UserGroup.Admin)]
		[AcceptVerbs(HttpVerbs.Post)]
		[ValidateInput(true)]
		public ActionResult AddTemplate([Bind(Prefix = "")] Template template, HttpPostedFileBase postedFile)
		{
			bool fileValid = true;
			string baseDirectory = null;

			try
			{
				if (postedFile == null)
				{
					throw new ValidationException(new ValidationError("postedFile", ValidationErrorType.NullOrEmpty));
				}

				TemplatesServiceClient.Add(template);

				if (SafeIO.Path_GetExtension(postedFile.FileName) == ".zip")
				{
					//Validate max length
					if (postedFile.ContentLength > 1024 * 1024 * 3)
					{
						fileValid = false;
					}
				}
				else
				{
					fileValid = false;
				}

				if (fileValid)
				{
					baseDirectory = Server.MapPath(Config.Template.Path + template.Key);
					#region Create directories
					try
					{
						SafeIO.Directory_CreateDirectory(baseDirectory);
						SafeIO.Directory_CreateDirectory(baseDirectory + "\\contents");
					}
					catch (UnauthorizedAccessException)
					{
						throw new ValidationException(new ValidationError("postedFile", ValidationErrorType.AccessRights));
					}
					#endregion
					//Open the zip file.
					#region Save the files in the zip file
					using (ZipInputStream zipStream = new ZipInputStream(postedFile.InputStream))
					{
						ZipEntry entry;
						while (((entry = zipStream.GetNextEntry()) != null) && fileValid)
						{
							fileValid = ValidateFileName(entry.Name);

							if (fileValid && entry.IsFile)
							{
								string fileName = baseDirectory;

								if (SafeIO.Path_GetDirectoryName(entry.Name).ToUpper() == "TEMPLATE-CONTENTS")
								{
									fileName += "\\contents";
								}
								fileName += "\\" + SafeIO.Path_GetFileName(entry.Name);

								#region Save file
								using (System.IO.FileStream streamWriter = SafeIO.File_Create(fileName))
								{
									int size = 2048;
									byte[] data = new byte[2048];
									while (true)
									{
										size = zipStream.Read(data, 0, data.Length);
										if (size > 0)
										{
											streamWriter.Write(data, 0, size);
										}
										else
										{
											break;
										}
									}
									streamWriter.Close();
								}
								#endregion
							}
						}
						zipStream.Close();
					}
					#endregion
				}

				if (fileValid)
				{
					//All worked file
					ReplaceFilePaths(baseDirectory + "\\template.html", Config.Template.Path + template.Key + "/");

					ChopTemplateFile(baseDirectory + "\\template.html");

					return RedirectToAction("ListTemplates", "Admin");
				}
				else
				{
					throw new ValidationException(new ValidationError("postedFile", ValidationErrorType.FileFormat));
				}
			}
			catch (ValidationException ex)
			{
				this.AddErrors(this.ModelState, ex);

				//Delete the folder
				if (baseDirectory != null)
				{
					try
					{
						SafeIO.Directory_Delete(baseDirectory, true);
					}
					catch (UnauthorizedAccessException)
					{

					}
				}
				if (template.Id > 0)
				{
					TemplatesServiceClient.Delete(template.Id);
				}
			}
			return View();
		}

		#region Validate filename
		private bool ValidateFileName(string fileName)
		{
			bool fileValid = true;
			if (!Regex.IsMatch(fileName, @"^((template\.html)|(template-contents/((.*\.gif)|(.*\.png)|(.*\.jpg)|(.*\.css))?))$", RegexOptions.IgnoreCase))
			{
				fileValid = false;
			}
			return fileValid;
		}
		#endregion

		#region Replace files paths
		/// <summary>
		/// Replaces the paths from template-contents/ to /Content/Templates/{NameOfTheTemplate}/contents/ in the files .html and css
		/// </summary>
		private void ReplaceFilePaths(string filePath, string newPath)
		{
			System.IO.File.WriteAllText(filePath, Regex.Replace(System.IO.File.ReadAllText(filePath), "template-contents/", newPath + "contents/"));
		}
		#endregion

		#region Chop template
		/// <summary>
		/// Chop the file template.html to template.part.1.html ... template.part.n.html
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns>Amount of parts</returns>
		public int ChopTemplateFile(string fileName)
		{
			List<StringBuilder> partsList = new List<StringBuilder>();

			string currentLine = null;
			using (System.IO.FileStream inputStream = SafeIO.File_OpenRead(fileName))
			{
				using (System.IO.StreamReader inputReader = new System.IO.StreamReader(inputStream))
				{
					StringBuilder part = new StringBuilder();
					partsList.Add(part);
					while ((currentLine = inputReader.ReadLine()) != null)
					{
						//Search for 
						int matches = Regex.Matches(currentLine, @"{-\w+?-}").Count;
						if (matches > 0)
						{
							bool endReadingLine = false;
							int i = 0;
							while (i < currentLine.Length && !endReadingLine)
							{
								if (currentLine[i] == '{' && currentLine[i + 1] == '-')
								{
									//New part
									if (part.Length > 0)
									{
										part = new StringBuilder();
										partsList.Add(part);
									}
									part.Append(currentLine[i]);
								}
								else if (currentLine[i] == '}' && currentLine[i - 1] == '-')
								{
									part.Append(currentLine[i]);
									part = new StringBuilder();
									partsList.Add(part);
								}
								else
								{
									part.Append(currentLine[i]);
								}
								i++;
							}

						}
						else
						{
							part.AppendLine(currentLine);
						}

					}
				}
			}

			//Save the files based on the partsList.
			int partNumber = 0;
			foreach (StringBuilder part in partsList)
			{
				if (part.Length > 0)
				{
					//template.part.1.html
					string directoryName = SafeIO.Path_GetDirectoryName(fileName);
					SafeIO.File_WriteAllText(directoryName + "\\template.part." + partNumber + ".html", part.ToString(), Encoding.UTF8);
					partNumber++;
				}
			}

			return partsList.Count;
		}
		#endregion
		#endregion

		#region List templates
		[RequireAuthorization(UserGroup.Admin)]
		public ActionResult ListTemplates(TemplateActionError? error)
		{
			List<Template> list = TemplatesServiceClient.GetAll();
			if (error == TemplateActionError.DeleteCurrent)
			{
				ViewData["DeleteCurrent"] = true;
			}
			else if (error == TemplateActionError.UnauthorizedAccess)
			{
				ViewData["Access"] = true;
			}
			return View(list);
		}

		public enum TemplateActionError
		{
			DeleteCurrent=0,
			UnauthorizedAccess=1
		}
		#endregion 

		#region Set current
		[RequireAuthorization(UserGroup.Admin)]
		public ActionResult TemplateSetCurrent(int id)
		{
			TemplatesServiceClient.SetCurrent(id);

			this.Cache.Template = null;

			return RedirectToAction("ListTemplates", "Admin");
		}
		#endregion

		#region Delete Template
		[RequireAuthorization(UserGroup.Admin)]
		public ActionResult DeleteTemplate(int id)
		{
			TemplateActionError? error = null;

			Template t = TemplatesServiceClient.Get(id);
			if (t != null)
			{
				if (t.IsCurrent)
				{
					error = TemplateActionError.DeleteCurrent;
				}
				else
				{

					string baseDirectory = Server.MapPath(Config.Template.Path + t.Key);
					try
					{
						SafeIO.Directory_Delete(baseDirectory, true);
						TemplatesServiceClient.Delete(id);
					}
					catch (UnauthorizedAccessException)
					{
						error = TemplateActionError.UnauthorizedAccess;
					}
				}
				
			}
			return RedirectToAction("ListTemplates", new{error=error});
		}
		#endregion
		#endregion

		#region Dashboard
		[RequireAuthorization(UserGroup.Moderator)]
		public ActionResult Dashboard()
		{
			return View();
		}
		#endregion

		#region Status
		/// <summary>
		/// Shows the status of the website/webserver/db/configuration
		/// </summary>
		/// <returns></returns>
		[RequireAuthorization(UserGroup.Admin)]
		public ActionResult Status()
		{
			this.StatusFill();
			return View();
		}

		/// <summary>
		/// If there is no user created on the db= >gets the status of the website. 
		/// </summary>
		public ActionResult StatusFirst()
		{
			try
			{
				if (UsersServiceClient.GetTestUser() != null)
				{
					return RedirectToAction("Status");
				}
			}
			catch
			{
			}
			this.StatusFill();
			return View("Status");
		}

		/// <summary>
		/// Adds current status to the viewdata
		/// </summary>
		protected virtual void StatusFill()
		{
			try
			{
				Exception lastException = null;
				#region Server and Web.config
				var systemWeb = (SystemWebSectionGroup)ConfigurationManager.GetSection("system.web");
				var compilation = (CompilationSection)ConfigurationManager.GetSection("system.web/compilation");
				var customErrors = (CustomErrorsSection)ConfigurationManager.GetSection("system.web/customErrors");
				var smtp = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
				var defaultProxy = (DefaultProxySection)ConfigurationManager.GetSection("system.net/defaultProxy");

				ViewData["Debug"] = compilation.Debug;
				ViewData["CustomErrors"] = customErrors.Mode;
				ViewData["MachineName"] = Server.MachineName;
				ViewData["Mail"] = (smtp != null && smtp.From != null) ? "Set" : "Not properly set";
				ViewData["Proxy"] = defaultProxy != null && defaultProxy.Enabled;
				if (defaultProxy != null && defaultProxy.Enabled)
				{
					try
					{
						ViewData["Proxy-Address"] = defaultProxy.Proxy.ProxyAddress;
					}
					catch
					{
					}
				}
				ViewData["Connectivity"] = "Failure";
				try
				{
					new WebClient().OpenRead("http://google.com");
					ViewData["Connectivity"] = "Success";
				}
				catch (Exception ex)
				{
					//No need to do nothing with the exception
					lastException = ex;
				}
				#endregion

				#region Database
				ConnectionStringSettings connString = ConfigurationManager.ConnectionStrings["Forums"];
				ViewData["ConnectionString"] = connString == null ? "Not set" : "Set";
				ViewData["ConnectionStringProvider"] = connString != null ? connString.ProviderName : "";
				try
				{
					UsersServiceClient.GetTestUser();
					ViewData["DatabaseTest"] = "Success";
				}
				catch
				{
					ViewData["DatabaseTest"] = "Failure. Could not connect to database.";
					ViewData["WillNotRun"] = true;
				}
				#endregion

				#region Logging
				ViewData["LoggingEnabled"] = LoggerServiceClient.IsEnabled;
				#endregion

				#region Project
				ViewData["Version"] = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
				#endregion

				#region Notifications
				ViewData["Subscriptions"] = Config.Notifications.Subscription.IsDefined;
				#endregion

				#region Authorization providers

				SecurityHelper.TryLoginFromFacebook(this.Session, this.Request, this.Response);

				ViewData["Facebook"] = Config.AuthorizationProviders.Facebook.IsDefined;
				ViewData["Twitter"] = Config.AuthorizationProviders.Twitter.IsDefined;
				#region Test Twitter
				if (Config.AuthorizationProviders.Twitter.IsDefined)
				{
					try
					{
						ViewData["Twitter-Test"] = "Failure";
						var twitter = new WebConsumer(TwitterConsumer.ServiceDescription, new InMemoryTokenManager(Config.AuthorizationProviders.Twitter.ApiKey, Config.AuthorizationProviders.Twitter.SecretKey));
						UserAuthorizationRequest usr = twitter.PrepareRequestUserAuthorization();
						ViewData["Twitter-Test"] = "Success";
					}
					catch (Exception ex)
					{
						//There is no need to do nothing with the exception.
						//Is just to show a success or failure message.
						lastException = ex;
					}
				}
				#endregion

				#region Test SSOOpenId
				ViewData["SSOOpenId"] = Config.AuthorizationProviders.SSOOpenId.IsDefined;
				if (Config.AuthorizationProviders.SSOOpenId.IsDefined)
				{
					try
					{
						ViewData["SSOOpenId-Test"] = "Failure";
						OpenIdRelyingParty openid = new OpenIdRelyingParty();
						var authenticationRequest = openid.CreateRequest(Identifier.Parse(Config.AuthorizationProviders.SSOOpenId.Identifier), new Uri("http://dummyurl.com/"), new Uri("http://dummyurl.com/login"));
						ViewData["SSOOpenId-Test"] = "Success";
					}
					catch (Exception ex)
					{
						//There is no need to do nothing with the exception.
						//Is just to show a success or failure message.
						lastException = ex;
					}
				}
				#endregion
				#endregion
			}
			catch (Exception ex)
			{
				//There were errors while getting the webserver/database/config/website status
				LoggerServiceClient.LogError(ex);
				ViewData["StatusError"] = ex.Message;
			}
		}
		#endregion
	}
}
