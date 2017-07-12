using Fiddler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
namespace BasicFormats
{
	[ProfferFormat("IE's F12 NetXML", "Internet Explorer 9 Developer Tools Export Format.")]
	public class F12NetXML : ISessionImporter, IDisposable
	{
		public Session[] ImportSessions(string sFormat, Dictionary<string, object> dictOptions, EventHandler<ProgressCallbackEventArgs> evtProgressNotifications)
		{
			if (sFormat != "IE's F12 NetXML")
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
				text = Utilities.ObtainOpenFilename("Import from " + sFormat, "IE's F12 NetXML (*.xml)|*.xml");
			}
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			List<Session> list = null;
			try
			{
				FileStream fileStream = new FileStream(text, FileMode.Open, FileAccess.Read);
				list = HTTPArchiveXML.LoadStream(fileStream, evtProgressNotifications);
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
