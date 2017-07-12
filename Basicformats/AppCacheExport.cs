using Fiddler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
namespace BasicFormats
{
	[ProfferFormat("HTML5 AppCache Manifest", "HTML5 allows creation of Application Caches based on a manifest. See http://diveintohtml5.org/offline.html")]
	public class AppCacheExport : ISessionExporter, IDisposable
	{
		public bool ExportSessions(string sFormat, Session[] oSessions, Dictionary<string, object> dictOptions, EventHandler<ProgressCallbackEventArgs> evtProgressNotifications)
		{
			if (sFormat != "HTML5 AppCache Manifest")
			{
				return false;
			}
			bool result = false;
			string text = null;
			if (string.IsNullOrEmpty(text))
			{
				AppCacheOptions appCacheOptions = new AppCacheOptions();
				List<string> list = new List<string>();
				appCacheOptions.lvItems.BeginUpdate();
				for (int i = 0; i < oSessions.Length; i++)
				{
					Session session = oSessions[i];
					if (!session.HTTPMethodIs("CONNECT") && session.get_responseCode() >= 200 && session.get_responseCode() <= 399 && !list.Contains(session.get_fullUrl()))
					{
						list.Add(session.get_fullUrl());
						string text2 = (session.oResponse.get_headers() != null) ? Utilities.TrimAfter(session.oResponse.get_headers().get_Item("Content-Type"), ";") : string.Empty;
						ListViewItem listViewItem = appCacheOptions.lvItems.Items.Add(session.get_fullUrl());
						listViewItem.SubItems.Add((session.responseBodyBytes != null) ? session.responseBodyBytes.Length.ToString() : "0");
						listViewItem.SubItems.Add(text2);
						if (session.HTTPMethodIs("POST"))
						{
							listViewItem.Checked = true;
						}
						if (text2.IndexOf("script", StringComparison.OrdinalIgnoreCase) > -1)
						{
							listViewItem.Group = appCacheOptions.lvItems.Groups["lvgScript"];
						}
						else
						{
							if (text2.IndexOf("image/", StringComparison.OrdinalIgnoreCase) > -1)
							{
								listViewItem.Group = appCacheOptions.lvItems.Groups["lvgImages"];
							}
							else
							{
								if (text2.IndexOf("html", StringComparison.OrdinalIgnoreCase) > -1)
								{
									listViewItem.Group = appCacheOptions.lvItems.Groups["lvgMarkup"];
								}
								else
								{
									if (text2.IndexOf("css", StringComparison.OrdinalIgnoreCase) > -1)
									{
										listViewItem.Group = appCacheOptions.lvItems.Groups["lvgCSS"];
									}
									else
									{
										listViewItem.Group = appCacheOptions.lvItems.Groups["lvgOther"];
									}
								}
							}
						}
						listViewItem.Tag = session;
					}
				}
				appCacheOptions.lvItems.EndUpdate();
				if (appCacheOptions.lvItems.Items.Count > 0)
				{
					appCacheOptions.lvItems.FocusedItem = appCacheOptions.lvItems.Items[0];
				}
				if (DialogResult.OK == appCacheOptions.ShowDialog(FiddlerApplication.get_UI()))
				{
					text = Utilities.ObtainSaveFilename("Export As " + sFormat, "AppCache Manifest (*.appcache)|*.appcache");
					if (!string.IsNullOrEmpty(text))
					{
						try
						{
							List<string> list2 = new List<string>();
							List<string> list3 = new List<string>();
							string text3 = appCacheOptions.txtBase.Text.Trim();
							if (text3.Length == 0)
							{
								text3 = null;
							}
							for (int j = 0; j < appCacheOptions.lvItems.Items.Count; j++)
							{
								string text4 = appCacheOptions.lvItems.Items[j].Text;
								if (text3 != null && text4.Length > text3.Length && text4.StartsWith(text3))
								{
									text4 = text4.Substring(text3.Length);
								}
								if (appCacheOptions.lvItems.Items[j].Checked)
								{
									list3.Add(text4);
								}
								else
								{
									list2.Add(text4);
								}
							}
							StringBuilder stringBuilder = new StringBuilder();
							stringBuilder.AppendFormat("CACHE MANIFEST\r\n# Generated: {0}\r\n\r\n", DateTime.Now.ToString());
							if (text3 != null)
							{
								stringBuilder.AppendFormat("# Deploy so that URLs are relative to: {0}\r\n\r\n", text3);
							}
							if (list2.Count > 0)
							{
								stringBuilder.Append("CACHE:\r\n");
								stringBuilder.Append(string.Join("\r\n", list2.ToArray()));
								stringBuilder.Append("\r\n");
							}
							if (appCacheOptions.cbNetworkFallback.Checked || list3.Count > 0)
							{
								stringBuilder.Append("\r\nNETWORK:\r\n");
								if (appCacheOptions.cbNetworkFallback.Checked)
								{
									stringBuilder.Append("*\r\n");
								}
								stringBuilder.Append(string.Join("\r\n", list3.ToArray()));
							}
							File.WriteAllText(text, stringBuilder.ToString());
							Process.Start(CONFIG.GetPath("TextEditor"), text);
							bool result2 = true;
							return result2;
						}
						catch (Exception ex)
						{
							FiddlerApplication.ReportException(ex, "Failed to save MeddlerScript");
							bool result2 = false;
							return result2;
						}
					}
					appCacheOptions.Dispose();
				}
			}
			return result;
		}
		public void Dispose()
		{
		}
	}
}
