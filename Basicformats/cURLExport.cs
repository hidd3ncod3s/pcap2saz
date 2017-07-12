using Fiddler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace BasicFormats
{
	[ProfferFormat("cURL Script", "CURL Scripts are played back by cURL; see http://curl.haxx.se/")]
	public class cURLExport : ISessionExporter, IDisposable
	{
		private static string _EscapeString(string sIn)
		{
			if (string.IsNullOrEmpty(sIn))
			{
				return string.Empty;
			}
			return sIn.Replace("\"", "\\\"").Replace("%", "%%");
		}
		public bool ExportSessions(string sFormat, Session[] oSessions, Dictionary<string, object> dictOptions, EventHandler<ProgressCallbackEventArgs> evtProgressNotifications)
		{
			if (sFormat != "cURL Script")
			{
				return false;
			}
			bool result = false;
			string text = null;
			if (dictOptions != null && dictOptions.ContainsKey("Filename"))
			{
				text = (dictOptions["Filename"] as string);
			}
			if (string.IsNullOrEmpty(text))
			{
				text = Utilities.ObtainSaveFilename("Export As " + sFormat, "Batch Script (*.bat)|*.bat");
			}
			if (!string.IsNullOrEmpty(text))
			{
				try
				{
					StringBuilder stringBuilder = new StringBuilder();
					string stringPref = FiddlerApplication.get_Prefs().GetStringPref("fiddler.exporters.cURL.DefaultOptions", "-k -i --raw");
					int num = 0;
					int num2 = 0;
					bool result2;
					for (int i = 0; i < oSessions.Length; i++)
					{
						Session session = oSessions[i];
						string text2 = string.Empty;
						if (session.HTTPMethodIs("CONNECT"))
						{
							num++;
						}
						else
						{
							if (!session.HTTPMethodIs("GET"))
							{
								if (session.HTTPMethodIs("HEAD"))
								{
									text2 = "-I ";
								}
								else
								{
									text2 = string.Format("-X {0} ", session.get_RequestMethod());
								}
							}
							string text3;
							if (stringPref.Contains("-i"))
							{
								text3 = string.Format("{0}.dat", num2);
							}
							else
							{
								text3 = session.get_SuggestedFilename();
							}
							string text4 = string.Empty;
							if (session.oRequest.get_Item("Content-Type").StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
							{
								text4 = string.Format("-d \"{0}\" ", cURLExport._EscapeString(session.GetRequestBodyAsString()));
							}
							string text5 = cURLExport._EscapeString(session.get_fullUrl());
							stringBuilder.AppendFormat("curl {0} -o {1} {2}{3}\"{4}\"", new object[]
							{
								stringPref,
								text3,
								text2,
								text4,
								text5
							});
							foreach (HTTPHeaderItem current in session.oRequest.get_headers())
							{
								if (!(current.Name == "Content-Length"))
								{
									stringBuilder.AppendFormat(" -H \"{0}\"", cURLExport._EscapeString(current.ToString()));
								}
							}
							stringBuilder.AppendLine();
							num++;
							num2++;
							if (evtProgressNotifications != null)
							{
								ProgressCallbackEventArgs progressCallbackEventArgs = new ProgressCallbackEventArgs((float)num / (float)oSessions.Length, "Added " + num.ToString() + " sessions to cURL Script.");
								evtProgressNotifications(null, progressCallbackEventArgs);
								if (progressCallbackEventArgs.get_Cancel())
								{
									result2 = false;
									return result2;
								}
							}
						}
					}
					this.PrependScriptHeader(stringBuilder);
					this.EmitScriptFooter(stringBuilder);
					File.WriteAllText(text, stringBuilder.ToString());
					result2 = true;
					return result2;
				}
				catch (Exception ex)
				{
					FiddlerApplication.ReportException(ex, "Failed to save cURLScript");
					bool result2 = false;
					return result2;
				}
				return result;
			}
			return result;
		}
		private void PrependScriptHeader(StringBuilder sb)
		{
		}
		private void EmitScriptFooter(StringBuilder sb)
		{
		}
		public void Dispose()
		{
		}
	}
}
