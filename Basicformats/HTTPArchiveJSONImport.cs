using Fiddler;
using Fiddler.WebFormats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
namespace BasicFormats
{
	internal class HTTPArchiveJSONImport
	{
		public static string _getHeaderStringFromArrayList(ArrayList alHeaders)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Hashtable hashtable in alHeaders)
			{
				stringBuilder.AppendFormat("{0}: {1}\r\n", hashtable["name"], hashtable["value"]);
			}
			return stringBuilder.ToString();
		}
		private static byte[] _getRequestFromEntry(Hashtable htRequest)
		{
			string text = (string)htRequest["method"];
			string text2 = htRequest["httpVersion"] as string;
			if (string.IsNullOrEmpty(text2))
			{
				text2 = "HTTP/0.0";
			}
			string text3 = (string)htRequest["url"];
			string text4 = HTTPArchiveJSONImport._getHeaderStringFromArrayList((ArrayList)htRequest["headers"]);
			string text5 = string.Empty;
			if (htRequest.ContainsKey("postData"))
			{
				Hashtable hashtable = htRequest["postData"] as Hashtable;
				if (hashtable != null)
				{
					if (hashtable.ContainsKey("text"))
					{
						text5 = (string)hashtable["text"];
					}
					else
					{
						if (hashtable.ContainsKey("params"))
						{
							text5 = HTTPArchiveJSONImport._getStringFromParams((ArrayList)hashtable["params"]);
						}
					}
				}
			}
			if (string.Equals("CONNECT", text, StringComparison.OrdinalIgnoreCase))
			{
				text3 = Utilities.TrimBeforeLast(text3, '/');
			}
			string s = string.Format("{0} {1} {2}\r\n{3}\r\n{4}", new object[]
			{
				text,
				text3,
				text2,
				text4,
				text5
			});
			return CONFIG.oHeaderEncoding.GetBytes(s);
		}
		private static string _getStringFromParams(ArrayList alParams)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			foreach (Hashtable hashtable in alParams)
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					stringBuilder.Append("&");
				}
				stringBuilder.AppendFormat("{0}={1}", Utilities.UrlEncode((string)hashtable["name"], Encoding.UTF8), Utilities.UrlEncode((string)hashtable["value"], Encoding.UTF8));
			}
			return stringBuilder.ToString();
		}
		private static byte[] _getBodyArrayFromContent(Hashtable htContent, string sHeaders)
		{
			byte[] array = Utilities.emptyByteArray;
			if (htContent == null)
			{
				return array;
			}
			if (htContent.ContainsKey("text"))
			{
				if (htContent.ContainsKey("encoding") && "base64" == (string)htContent["encoding"])
				{
					array = Convert.FromBase64String((string)htContent["text"]);
				}
				else
				{
					Encoding encoding = Encoding.UTF8;
					if (htContent.ContainsKey("mimeType") && ((string)htContent["mimeType"]).IndexOf("charset") > -1)
					{
						Regex regex = new Regex("charset\\s?=\\s?[\"]?(?<TokenValue>[^\";]*)");
						Match match = regex.Match((string)htContent["mimeType"]);
						if (match.Success && match.Groups["TokenValue"] != null)
						{
							try
							{
								encoding = Encoding.GetEncoding(match.Groups["TokenValue"].Value);
							}
							catch (Exception)
							{
							}
						}
					}
					array = encoding.GetBytes((string)htContent["text"]);
				}
				int num = sHeaders.IndexOf("Content-Encoding", StringComparison.OrdinalIgnoreCase);
				if (num > -1)
				{
					string text = sHeaders.Substring(num);
					text = Utilities.TrimAfter(text, '\n');
					if (text.IndexOf("gzip", StringComparison.OrdinalIgnoreCase) > -1)
					{
						array = Utilities.GzipCompress(array);
					}
					else
					{
						if (text.IndexOf("deflate", StringComparison.OrdinalIgnoreCase) > -1)
						{
							array = Utilities.DeflaterCompress(array);
						}
					}
				}
				num = sHeaders.IndexOf("Transfer-Encoding", StringComparison.OrdinalIgnoreCase);
				if (num > -1)
				{
					string text2 = sHeaders.Substring(num);
					text2 = Utilities.TrimAfter(text2, '\n');
					if (text2.IndexOf("chunked", StringComparison.OrdinalIgnoreCase) > -1)
					{
						array = Utilities.doChunk(array, 2);
					}
				}
			}
			return array;
		}
		private static byte[] _getResponseFromEntry(Hashtable htResponse)
		{
			string text = ((double)htResponse["status"]).ToString();
			string text2 = (string)htResponse["statusText"];
			string text3 = htResponse["httpVersion"] as string;
			if (string.IsNullOrEmpty(text3))
			{
				text3 = "HTTP/0.0";
			}
			else
			{
				if (text3.Contains(" "))
				{
					text3 = text3.Replace(' ', '_');
				}
				else
				{
					if (text3.StartsWith("1."))
					{
						text3 = "HTTP/" + text3;
					}
				}
			}
			string text4 = HTTPArchiveJSONImport._getHeaderStringFromArrayList((ArrayList)htResponse["headers"]);
			byte[] array = HTTPArchiveJSONImport._getBodyArrayFromContent((Hashtable)htResponse["content"], text4);
			string s = string.Format("{0} {1} {2}\r\n{3}\r\n", new object[]
			{
				text3,
				text,
				text2,
				text4
			});
			byte[] bytes = CONFIG.oHeaderEncoding.GetBytes(s);
			byte[] array2 = new byte[bytes.Length + array.Length];
			Buffer.BlockCopy(bytes, 0, array2, 0, bytes.Length);
			Buffer.BlockCopy(array, 0, array2, bytes.Length, array.Length);
			return array2;
		}
		public static bool LoadStream(StreamReader oSR, List<Session> listSessions, EventHandler<ProgressCallbackEventArgs> evtProgressNotifications)
		{
			string text = oSR.ReadToEnd();
			JSON.JSONParseErrors jSONParseErrors;
			Hashtable hashtable = JSON.JsonDecode(text, ref jSONParseErrors) as Hashtable;
			if (hashtable == null)
			{
				MessageBox.Show("This file is not properly formatted HAR JSON", "Import aborted");
				return false;
			}
			Hashtable hashtable2 = hashtable["log"] as Hashtable;
			ArrayList arrayList;
			if (hashtable2 != null)
			{
				if (evtProgressNotifications != null)
				{
					evtProgressNotifications(null, new ProgressCallbackEventArgs(0f, "Found HTTPArchive v" + hashtable2["version"] + "..."));
				}
				arrayList = (ArrayList)hashtable2["entries"];
			}
			else
			{
				MessageBox.Show("This file is not properly formatted HAR JSON.\n\nNote: Chrome does not save valid HAR content when you use 'Save Entry as HAR'. It only generates a valid file if you use 'Save All as HAR'", "Warning");
				arrayList = new ArrayList();
				arrayList.Add(hashtable);
			}
			int num = 0;
			int count = arrayList.Count;
			foreach (Hashtable htEntry in arrayList)
			{
				try
				{
					Session session = HTTPArchiveJSONImport._getSessionFromEntry(htEntry);
					if (session != null)
					{
						num++;
						listSessions.Add(session);
						if (evtProgressNotifications != null)
						{
							evtProgressNotifications(null, new ProgressCallbackEventArgs((float)(num / count), "Imported " + num.ToString() + " sessions."));
						}
					}
				}
				catch (Exception ex)
				{
					if (evtProgressNotifications != null)
					{
						evtProgressNotifications(null, new ProgressCallbackEventArgs((float)(num / count), "Skipping malformed session." + ex.Message));
					}
				}
			}
			return true;
		}
		private static Session _getSessionFromEntry(Hashtable htEntry)
		{
			Hashtable htRequest = (Hashtable)htEntry["request"];
			byte[] array = HTTPArchiveJSONImport._getRequestFromEntry(htRequest);
			Hashtable hashtable = (Hashtable)htEntry["response"];
			byte[] array2 = HTTPArchiveJSONImport._getResponseFromEntry(hashtable);
			if (array == null || array2 == null)
			{
				MessageBox.Show("Failed to get session from entry");
				return null;
			}
			SessionFlags sessionFlags = 64;
			Session session = new Session(array, array2, sessionFlags);
			int totalSize = HTTPArchiveJSONImport.getTotalSize(hashtable);
			if (totalSize > 0)
			{
				session.set_Item("X-TRANSFER-SIZE", totalSize.ToString());
			}
			if (htEntry.ContainsKey("comment"))
			{
				string text = (string)htEntry["comment"];
				if (!string.IsNullOrEmpty(text))
				{
					session.set_Item("ui-comments", text);
				}
			}
			DateTime now;
			if (!DateTime.TryParse((string)htEntry["startedDateTime"], out now))
			{
				now = DateTime.Now;
			}
			if (htEntry.ContainsKey("timings"))
			{
				Hashtable htTimers = (Hashtable)htEntry["timings"];
				session.Timers.DNSTime = HTTPArchiveJSONImport.getMilliseconds(htTimers, "dns");
				session.Timers.TCPConnectTime = HTTPArchiveJSONImport.getMilliseconds(htTimers, "connect");
				session.Timers.HTTPSHandshakeTime = HTTPArchiveJSONImport.getMilliseconds(htTimers, "ssl");
				session.Timers.ClientConnected = (session.Timers.ClientBeginRequest = (session.Timers.FiddlerGotRequestHeaders = (session.Timers.ClientDoneRequest = now)));
				session.Timers.ServerConnected = (session.Timers.FiddlerBeginRequest = now.AddMilliseconds((double)(HTTPArchiveJSONImport.getMilliseconds(htTimers, "blocked") + session.Timers.DNSTime + session.Timers.TCPConnectTime + session.Timers.HTTPSHandshakeTime)));
				session.Timers.ServerGotRequest = session.Timers.FiddlerBeginRequest.AddMilliseconds((double)HTTPArchiveJSONImport.getMilliseconds(htTimers, "send"));
				session.Timers.ServerBeginResponse = (session.Timers.FiddlerGotResponseHeaders = session.Timers.ServerGotRequest.AddMilliseconds((double)HTTPArchiveJSONImport.getMilliseconds(htTimers, "wait")));
				session.Timers.ServerDoneResponse = session.Timers.ServerBeginResponse.AddMilliseconds((double)HTTPArchiveJSONImport.getMilliseconds(htTimers, "receive"));
				session.Timers.ClientBeginResponse = (session.Timers.ClientDoneResponse = session.Timers.ServerDoneResponse);
			}
			return session;
		}
		private static int getTotalSize(Hashtable htMessage)
		{
			int num = -1;
			double? num2 = htMessage["headersSize"] as double?;
			if (num2.HasValue)
			{
				num = (int)Math.Round(num2.Value);
			}
			int num3 = -1;
			num2 = (htMessage["bodySize"] as double?);
			if (num2.HasValue)
			{
				num3 = (int)Math.Round(num2.Value);
			}
			if (num < 0 || num3 < 0)
			{
				return -1;
			}
			return num + num3;
		}
		private static int getMilliseconds(Hashtable htTimers, string sMeasure)
		{
			double? num = htTimers[sMeasure] as double?;
			if (!num.HasValue)
			{
				return 0;
			}
			int val = (int)Math.Round(num.Value);
			return Math.Max(0, val);
		}
	}
}
