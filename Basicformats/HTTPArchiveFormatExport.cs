using Fiddler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace BasicFormats
{
	[ProfferFormat("HTTPArchive v1.1", "A lossy JSON-based HTTP traffic archive format. Standard is documented @ http://groups.google.com/group/http-archive-specification/"), ProfferFormat("HTTPArchive v1.2", "A lossy JSON-based HTTP traffic archive format. Standard is documented @ http://groups.google.com/group/http-archive-specification/")]
	public class HTTPArchiveFormatExport : ISessionExporter, IDisposable
	{
		public bool ExportSessions(string sFormat, Session[] oSessions, Dictionary<string, object> dictOptions, EventHandler<ProgressCallbackEventArgs> evtProgressNotifications)
		{
			if (sFormat != "HTTPArchive v1.1" && sFormat != "HTTPArchive v1.2")
			{
				return false;
			}
			bool result = false;
			string text = null;
			int iMaxBinaryBodyLength = FiddlerApplication.get_Prefs().GetInt32Pref("fiddler.importexport.HTTPArchiveJSON.MaxBinaryBodyLength", 32768);
			int iMaxTextBodyLength = FiddlerApplication.get_Prefs().GetInt32Pref("fiddler.importexport.HTTPArchiveJSON.MaxTextBodyLength", 22);
			if (dictOptions != null)
			{
				if (dictOptions.ContainsKey("Filename"))
				{
					text = (dictOptions["Filename"] as string);
				}
				if (dictOptions.ContainsKey("MaxTextBodyLength"))
				{
					iMaxTextBodyLength = (int)dictOptions["MaxTextBodyLength"];
				}
				if (dictOptions.ContainsKey("MaxBinaryBodyLength"))
				{
					iMaxBinaryBodyLength = (int)dictOptions["MaxBinaryBodyLength"];
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				text = Utilities.ObtainSaveFilename("Export As " + sFormat, "HTTPArchive JSON (*.har)|*.har");
			}
			if (!string.IsNullOrEmpty(text))
			{
				try
				{
					StreamWriter streamWriter = new StreamWriter(text, false, Encoding.UTF8);
					HTTPArchiveJSONExport.WriteStream(streamWriter, oSessions, sFormat == "HTTPArchive v1.2", evtProgressNotifications, iMaxTextBodyLength, iMaxBinaryBodyLength);
					streamWriter.Close();
					bool result2 = true;
					return result2;
				}
				catch (Exception ex)
				{
					FiddlerApplication.ReportException(ex, "Failed to save HTTPArchive");
					bool result2 = false;
					return result2;
				}
				return result;
			}
			return result;
		}
		public void Dispose()
		{
		}
	}
}
