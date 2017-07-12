using Fiddler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace BasicFormats
{
	[ProfferFormat("MeddlerScript", "Meddler Scripts are played back from Meddler; see http://bayden.com/meddler/")]
	public class MeddlerExport : ISessionExporter, IDisposable
	{
		public bool ExportSessions(string sFormat, Session[] oSessions, Dictionary<string, object> dictOptions, EventHandler<ProgressCallbackEventArgs> evtProgressNotifications)
		{
			if (sFormat != "MeddlerScript")
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
				text = Utilities.ObtainSaveFilename("Export As " + sFormat, "MeddlerScript (*.ms)|*.ms");
			}
			if (!string.IsNullOrEmpty(text))
			{
				try
				{
					StringBuilder stringBuilder = new StringBuilder();
					int num = 0;
					string text2 = null;
					bool result2;
					for (int i = 0; i < oSessions.Length; i++)
					{
						Session session = oSessions[i];
						if (text2 == null)
						{
							text2 = "http://localhost:{$PORT}" + session.get_PathAndQuery();
						}
						stringBuilder.AppendFormat("\r\n\t\t\tif (oSession.requestHeaders.Path == '{0}')\r\n\t\t\t{{\r\n", session.get_PathAndQuery().Replace("'", "\\'"));
						stringBuilder.AppendFormat("\r\n\t\t\t\toHeaders.Version='{0}';", session.oResponse.get_headers().HTTPVersion.Replace("'", "\\'"));
						stringBuilder.AppendFormat("\r\n\t\t\t\toHeaders.Status='{0}';\r\n", session.oResponse.get_headers().HTTPResponseStatus.Replace("'", "\\'"));
						foreach (HTTPHeaderItem current in session.oResponse.get_headers())
						{
							stringBuilder.AppendFormat("\r\n\t\t\t\toHeaders.Add('{0}', '{1}');", current.Name.Replace("'", "\\'"), current.Value.Replace("'", "\\'"));
						}
						stringBuilder.AppendFormat("\r\n\t\t\t\toSession.WriteString(oHeaders);\r\n", new object[0]);
						stringBuilder.AppendFormat("\r\n\t\t\t\toSession.WriteBytes(Convert.FromBase64String('", new object[0]);
						stringBuilder.AppendFormat(Convert.ToBase64String(session.responseBodyBytes), new object[0]);
						stringBuilder.AppendFormat("'));\r\n", new object[0]);
						stringBuilder.AppendFormat("\t\t\t\toSession.CloseSocket(); return;\r\n\t\t\t}}\r\n", new object[0]);
						num++;
						if (evtProgressNotifications != null)
						{
							ProgressCallbackEventArgs progressCallbackEventArgs = new ProgressCallbackEventArgs((float)num / (float)oSessions.Length, "Added " + num.ToString() + " sessions to MeddlerScript.");
							evtProgressNotifications(null, progressCallbackEventArgs);
							if (progressCallbackEventArgs.get_Cancel())
							{
								result2 = false;
								return result2;
							}
						}
					}
					this.PrependMeddlerHeader(stringBuilder, text2 ?? "http://localhost:{$PORT}/");
					this.EmitMeddlerFooter(stringBuilder);
					File.WriteAllText(text, stringBuilder.ToString());
					result2 = true;
					return result2;
				}
				catch (Exception ex)
				{
					FiddlerApplication.ReportException(ex, "Failed to save MeddlerScript");
					bool result2 = false;
					return result2;
				}
				return result;
			}
			return result;
		}
		private void PrependMeddlerHeader(StringBuilder sb, string sLaunchURI)
		{
			sb.Insert(0, "import Meddler;\r\nimport System;\r\nimport System.Net.Sockets;\r\nimport System.Windows.Forms;\r\n\r\n// Script generated by Fiddler2 export.\r\n//\r\n// You can set options for this script using the format:\r\n//     ScriptOptions(\"StartURL\" (where {$PORT} is autoreplaced by the Meddler port number), \"Optional HTTPS Certificate Thumbprint\", \"Random # Seed\")\r\npublic ScriptOptions(\"" + sLaunchURI + "\")\r\nclass Handlers\r\n{\r\n\tstatic function OnConnection(oSession: Session)\r\n\t{\r\n\ttry {\r\n\t\tif (oSession.ReadRequest())\r\n\t\t{\r\n\t\t\tvar oHeaders: ResponseHeaders = new ResponseHeaders();");
		}
		private void EmitMeddlerFooter(StringBuilder sb)
		{
			sb.Append("\t\t}\r\n\t\toSession.CloseSocket();\r\n\t\t}\r\n\t\tcatch(e) {MeddlerObject.Log.LogString(\"Script threw exception\\n\"+e);}\r\n\t}\r\n}");
		}
		public void Dispose()
		{
		}
	}
}
