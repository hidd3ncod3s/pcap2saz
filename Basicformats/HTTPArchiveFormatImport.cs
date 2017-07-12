using Fiddler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace BasicFormats
{
	[ProfferFormat("HTTPArchive", "A lossy JSON-based HTTP traffic archive format. Learn more @ http://groups.google.com/group/http-archive-specification")]
	public class HTTPArchiveFormatImport : ISessionImporter, IDisposable
	{
		public Session[] ImportSessions(string sFormat, Dictionary<string, object> dictOptions, EventHandler<ProgressCallbackEventArgs> evtProgressNotifications)
		{
			if (sFormat != "HTTPArchive")
			{
				return null;
			}
			string text = null;
			if (dictOptions != null && dictOptions.ContainsKey("Filename"))
			{
				text = (dictOptions["Filename"] as string);
			}
			if (string.IsNullOrEmpty(text))
			{
				text = Utilities.ObtainOpenFilename("Import " + sFormat, "HTTPArchive JSON (*.har)|*.har");
			}
			if (!string.IsNullOrEmpty(text))
			{
				try
				{
					List<Session> list = new List<Session>();
					StreamReader streamReader = new StreamReader(text, Encoding.UTF8);
					HTTPArchiveJSONImport.LoadStream(streamReader, list, evtProgressNotifications);
					streamReader.Close();
					Session[] result = list.ToArray();
					return result;
				}
				catch (Exception ex)
				{
					FiddlerApplication.ReportException(ex, "Failed to import HTTPArchive");
					Session[] result = null;
					return result;
				}
			}
			return null;
		}
		public void Dispose()
		{
		}
	}
}
