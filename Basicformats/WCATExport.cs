using Fiddler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace BasicFormats
{
	[ProfferFormat("WCAT Script", "WCAT request-replay scripts are loaded by Microsoft's Web Capacity Analysis Tool, available from http://www.iis.net/community/Performance")]
	public class WCATExport : ISessionExporter, IDisposable
	{
		private string WCATEscape(string sInput)
		{
			return sInput.Replace("\\", "\\\\").Replace("\"", "\\\"");
		}
		public bool ExportSessions(string sFormat, Session[] oSessions, Dictionary<string, object> dictOptions, EventHandler<ProgressCallbackEventArgs> evtProgressNotifications)
		{
			if (sFormat != "WCAT Script")
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
				text = Utilities.ObtainSaveFilename("Export As " + sFormat, "WCAT Script (*.wcat)|*.wcat");
			}
			if (!string.IsNullOrEmpty(text))
			{
				try
				{
					StringBuilder stringBuilder = new StringBuilder();
					this.EmitScenarioHeader(stringBuilder);
					int num = 0;
					bool result2;
					for (int i = 0; i < oSessions.Length; i++)
					{
						Session session = oSessions[i];
						if (session.HTTPMethodIs("GET") || session.HTTPMethodIs("POST"))
						{
							stringBuilder.AppendLine("    request");
							stringBuilder.AppendLine("    {");
							stringBuilder.AppendFormat("      id = \"{0}\";\r\n", session.id);
							stringBuilder.AppendFormat("      url     = \"{0}\";\r\n", this.WCATEscape(session.PathAndQuery));
							if (session.isHTTPS)
							{
								stringBuilder.AppendLine("      secure = true");
								if (session.port != 443)
								{
                                    stringBuilder.AppendFormat("      port = {0};\r\n", session.port);
								}
							}
							else
							{
                                if (session.port != 80)
								{
                                    stringBuilder.AppendFormat("      port = {0};\r\n", session.port);
								}
							}
							if (session.oRequest.headers.HTTPVersion == "HTTP/1.0")
							{
								stringBuilder.AppendLine("      version = HTTP10;");
							}
							if (session.HTTPMethodIs("POST"))
							{
								stringBuilder.AppendLine("      verb = POST;");
								stringBuilder.AppendFormat("      postdata = \"{0}\";\r\n", this.WCATEscape(Encoding.UTF8.GetString(session.requestBodyBytes)));
							}
                            if (session.responseCode > 0 && 200 != session.responseCode)
							{
                                stringBuilder.AppendFormat("      statuscode = {0};\r\n", session.responseCode);
							}
                            HTTPRequestHeaders headers = session.oRequest.headers;
							foreach (HTTPHeaderItem current in headers)
							{
								this.EmitRequestHeaderEntry(stringBuilder, current.Name, current.Value);
							}
							stringBuilder.AppendLine("    }");
							num++;
							if (evtProgressNotifications != null)
							{
								ProgressCallbackEventArgs progressCallbackEventArgs = new ProgressCallbackEventArgs((float)num / (float)oSessions.Length, "Added " + num.ToString() + " sessions to WCAT Script.");
								evtProgressNotifications(null, progressCallbackEventArgs);
								if (progressCallbackEventArgs.Cancel)
								{
									result2 = false;
									return result2;
								}
							}
						}
					}
					stringBuilder.AppendLine("  }\r\n}");
					File.WriteAllText(text, stringBuilder.ToString());
					result2 = true;
					return result2;
				}
				catch (Exception ex)
				{
					FiddlerApplication.ReportException(ex, "Failed to save WCAT Script");
					bool result2 = false;
					return result2;
				}
				return result;
			}
			return result;
		}
		private void EmitScenarioHeader(StringBuilder sb)
		{
			sb.Append("scenario\r\n{\r\n  name    = \"Fiddler-Generated WCAT Script\";\r\n  warmup      = 30;\r\n  duration    = 120;\r\n  cooldown    = 10;\r\n\r\n  default\r\n  {\r\n    version = HTTP11;\r\n  }\r\n");
			sb.AppendLine("");
			sb.AppendLine("  transaction                        ");
			sb.AppendLine("  {                                  ");
			sb.AppendLine("    id = \"FiddlerScenario\";     ");
			sb.AppendLine("    weight = 1;");
		}
		private void EmitRequestHeaderEntry(StringBuilder sb, string headername, string headervalue)
		{
			sb.AppendLine("      addheader");
			sb.AppendLine("      {");
			sb.AppendLine("        name=\"" + headername + "\";");
			sb.AppendLine("        value=\"" + headervalue.Replace("\"", "\\\"") + "\";");
			sb.AppendLine("      }");
		}
		public void Dispose()
		{
		}
	}
}
