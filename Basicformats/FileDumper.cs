using Fiddler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
namespace BasicFormats
{
	[ProfferFormat("Raw Files", "Save all downloaded content to a folder.")]
	public class FileDumper : ISessionExporter, IDisposable
	{
		private string _MakeSafeFilename(string sFilename)
		{
			char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
			if (sFilename.IndexOfAny(invalidFileNameChars) < 0)
			{
				return Utilities.TrimAfter(sFilename, 255);
			}
			StringBuilder stringBuilder = new StringBuilder(sFilename);
			for (int i = 0; i < stringBuilder.Length; i++)
			{
				if (Array.IndexOf<char>(invalidFileNameChars, sFilename[i]) > -1 && sFilename[i] != Path.DirectorySeparatorChar)
				{
					stringBuilder[i] = '-';
				}
			}
			return Utilities.TrimAfter(stringBuilder.ToString(), 160);
		}
		public bool ExportSessions(string sFormat, Session[] oSessions, Dictionary<string, object> dictOptions, EventHandler<ProgressCallbackEventArgs> evtProgressNotifications)
		{
			if (sFormat != "Raw Files")
			{
				return false;
			}
			string text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			bool flag = true;
			bool flag2 = true;
			bool flag3 = true;
			bool flag4 = false;
			if (dictOptions != null)
			{
				if (dictOptions.ContainsKey("Folder"))
				{
					text = (dictOptions["Folder"] as string);
					flag4 = true;
				}
				if (dictOptions.ContainsKey("RecreateStructure"))
				{
					flag = string.Equals("True", dictOptions["RecreateStructure"] as string, StringComparison.OrdinalIgnoreCase);
					flag4 = true;
				}
				if (dictOptions.ContainsKey("OpenFolder"))
				{
					flag2 = string.Equals("True", dictOptions["OpenFolder"] as string, StringComparison.OrdinalIgnoreCase);
					flag4 = true;
				}
				if (dictOptions.ContainsKey("SkipNon200"))
				{
					flag3 = string.Equals("True", dictOptions["SkipNon200"] as string, StringComparison.OrdinalIgnoreCase);
					flag4 = true;
				}
			}
			if (!flag4)
			{
				UIFileExport uIFileExport = new UIFileExport();
				uIFileExport.txtLocation.Text = text;
				uIFileExport.cbRecreateFolderStructure.Checked = FiddlerApplication.get_Prefs().GetBoolPref("fiddler.exporters.RawFiles.RecreateStructure", true);
				uIFileExport.cbOpenFolder.Checked = FiddlerApplication.get_Prefs().GetBoolPref("fiddler.exporters.RawFiles.OpenFolder", true);
				uIFileExport.cbHTTP200Only.Checked = FiddlerApplication.get_Prefs().GetBoolPref("fiddler.exporters.RawFiles.SkipNon200", true);
				this.SetDefaultPath(uIFileExport.txtLocation, "fiddler.exporters.RawFiles.DefaultPath", text);
				DialogResult dialogResult = uIFileExport.ShowDialog();
				if (dialogResult != DialogResult.OK)
				{
					return false;
				}
				flag = uIFileExport.cbRecreateFolderStructure.Checked;
				flag2 = uIFileExport.cbOpenFolder.Checked;
				flag3 = uIFileExport.cbHTTP200Only.Checked;
				text = uIFileExport.txtLocation.Text;
				text = Utilities.EnsurePathIsAbsolute(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), text).Trim();
				FiddlerApplication.get_Prefs().SetBoolPref("fiddler.exporters.RawFiles.RecreateStructure", flag);
				FiddlerApplication.get_Prefs().SetBoolPref("fiddler.exporters.RawFiles.OpenFolder", flag2);
				FiddlerApplication.get_Prefs().SetBoolPref("fiddler.exporters.RawFiles.SkipNon200", flag3);
				FiddlerApplication.get_Prefs().SetStringPref("fiddler.exporters.RawFiles.DefaultPath", text);
				uIFileExport.Dispose();
				text = string.Concat(new object[]
				{
					text,
					Path.DirectorySeparatorChar,
					"Dump-",
					DateTime.Now.ToString("MMdd-HH-mm-ss"),
					Path.DirectorySeparatorChar
				});
			}
			try
			{
				Directory.CreateDirectory(text);
			}
			catch (Exception ex)
			{
				FiddlerApplication.ReportException(ex, "Export Failed");
				bool result = false;
				return result;
			}
			int num = 0;
			for (int i = 0; i < oSessions.Length; i++)
			{
				Session session = oSessions[i];
				try
				{
					if (!flag3 || session.get_responseCode() == 200)
					{
						if (session.HTTPMethodIs("CONNECT"))
						{
							num++;
						}
						else
						{
							if (session.responseBodyBytes != null && session.responseBodyBytes.Length > 0)
							{
								string text3;
								if (flag)
								{
									string text2 = Utilities.TrimAfter(session.get_url(), '?');
									text3 = text2.Replace('/', Path.DirectorySeparatorChar);
									if (text3.EndsWith(string.Empty + Path.DirectorySeparatorChar))
									{
										text3 += session.get_SuggestedFilename();
									}
									if (text3.Length > 0 && text3.Length < 260)
									{
										text3 = text + this._MakeSafeFilename(text3);
									}
									else
									{
										text3 = text + session.get_SuggestedFilename();
									}
								}
								else
								{
									text3 = text + session.get_SuggestedFilename();
								}
								text3 = Utilities.EnsureUniqueFilename(text3);
								byte[] array = session.responseBodyBytes;
								if (session.oResponse.get_headers().Exists("Content-Encoding") || session.oResponse.get_headers().Exists("Transfer-Encoding"))
								{
									array = (byte[])array.Clone();
									Utilities.utilDecodeHTTPBody(session.oResponse.get_headers(), ref array);
								}
								FiddlerApplication.get_Log().LogFormat("Writing #{0} to {1}", new object[]
								{
									session.get_id().ToString(),
									text3
								});
								Utilities.WriteArrayToFile(text3, array);
							}
							num++;
							if (evtProgressNotifications != null)
							{
								ProgressCallbackEventArgs progressCallbackEventArgs = new ProgressCallbackEventArgs((float)num / (float)oSessions.Length, "Dumped " + num.ToString() + " files to disk.");
								evtProgressNotifications(null, progressCallbackEventArgs);
								if (progressCallbackEventArgs.get_Cancel())
								{
									bool result = false;
									return result;
								}
							}
						}
					}
				}
				catch (Exception ex2)
				{
					FiddlerApplication.ReportException(ex2, "Failed to generate response file.");
				}
			}
			if (flag2)
			{
				try
				{
					string fileName = string.Format("\"{0}\"", text);
					using (Process.Start(new ProcessStartInfo(fileName)
					{
						Verb = "explore"
					}))
					{
					}
				}
				catch (Exception ex3)
				{
					FiddlerApplication.ReportException(ex3, "Cannot open folder");
				}
			}
			return true;
		}
		private void SetDefaultPath(TextBox txtUI, string sPrefName, string sDefaultPath)
		{
			string text = FiddlerApplication.get_Prefs().GetStringPref(sPrefName, sDefaultPath).Trim();
			try
			{
				if (!Directory.Exists(text))
				{
					text = sDefaultPath;
				}
			}
			catch
			{
				text = sDefaultPath;
			}
			txtUI.Text = text;
		}
		public void Dispose()
		{
		}
	}
}
