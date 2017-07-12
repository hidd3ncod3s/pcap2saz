using Fiddler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
namespace BasicFormats
{
	[ProfferFormat("TestStudio LoadTest", "Read the HTTP/HTTPS steps from a Telerik TestStudio LoadTest")]
	public class TSTestImporter : ISessionImporter, IDisposable
	{
		public Session[] ImportSessions(string sFormat, Dictionary<string, object> dictOptions, EventHandler<ProgressCallbackEventArgs> evtProgressNotifications)
		{
			if (sFormat != "TestStudio LoadTest")
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
				text = Utilities.ObtainOpenFilename("Import from " + sFormat, "Telerik TestStudio (*.tstest)|*.tstest");
			}
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			List<Session> list = null;
			try
			{
				FileStream fileStream = new FileStream(text, FileMode.Open, FileAccess.Read);
				int num = fileStream.ReadByte();
				fileStream.Seek(0L, SeekOrigin.Begin);
				if (num == 123)
				{
					list = TSTestJSON.LoadStream(fileStream, evtProgressNotifications);
				}
				else
				{
					list = TSTestXML.LoadStream(fileStream, evtProgressNotifications);
				}
				fileStream.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Failed to import...");
				return null;
			}
			return list.ToArray();
		}
		public void Dispose()
		{
		}
	}
}
