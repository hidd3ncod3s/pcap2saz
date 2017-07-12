using Fiddler;
using Fiddler.WebFormats;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
namespace BasicFormats
{
	internal class HTTPArchiveJSONExport
	{
		private static int _iMaxTextBodyLength;
		private static int _iMaxBinaryBodyLength;
		internal static bool WriteStream(StreamWriter swOutput, Session[] oSessions, bool bUseV1dot2Format, EventHandler<ProgressCallbackEventArgs> evtProgressNotifications, int iMaxTextBodyLength, int iMaxBinaryBodyLength)
		{
			HTTPArchiveJSONExport._iMaxTextBodyLength = iMaxTextBodyLength;
			HTTPArchiveJSONExport._iMaxBinaryBodyLength = iMaxBinaryBodyLength;
			Hashtable hashtable = new Hashtable();
			hashtable.Add("version", bUseV1dot2Format ? "1.2" : "1.1");
			hashtable.Add("pages", new ArrayList(0));
			if (bUseV1dot2Format)
			{
				hashtable.Add("comment", "exported @ " + DateTime.Now.ToString());
			}
			Hashtable hashtable2 = new Hashtable();
			hashtable2.Add("name", "Fiddler");
			hashtable2.Add("version", Application.ProductVersion);
			if (bUseV1dot2Format)
			{
				hashtable2.Add("comment", "http://www.fiddler2.com");
			}
			hashtable.Add("creator", hashtable2);
			ArrayList arrayList = new ArrayList();
			int num = 0;
			int i = 0;
			while (i < oSessions.Length)
			{
				Session session = oSessions[i];
				try
				{
					if (session.get_state() < 11)
					{
						goto IL_24D;
					}
					Hashtable hashtable3 = new Hashtable();
					hashtable3.Add("startedDateTime", session.Timers.ClientBeginRequest.ToString("o"));
					hashtable3.Add("request", HTTPArchiveJSONExport.getRequest(session));
					hashtable3.Add("response", HTTPArchiveJSONExport.getResponse(session, bUseV1dot2Format));
					hashtable3.Add("cache", new Hashtable());
					Hashtable timings = HTTPArchiveJSONExport.getTimings(session.Timers, bUseV1dot2Format);
					hashtable3.Add("time", HTTPArchiveJSONExport.getTotalTime(timings));
					hashtable3.Add("timings", timings);
					if (bUseV1dot2Format)
					{
						string value = session.get_Item("ui-comments");
						if (!string.IsNullOrEmpty(value))
						{
							hashtable3.Add("comment", session.get_Item("ui-comments"));
						}
						string arg_1A9_0 = session.m_hostIP;
						if (!string.IsNullOrEmpty(value) && !session.isFlagSet(2048))
						{
							hashtable3.Add("serverIPAddress", session.m_hostIP);
						}
						hashtable3.Add("connection", session.get_clientPort().ToString());
					}
					arrayList.Add(hashtable3);
				}
				catch (Exception ex)
				{
					FiddlerApplication.ReportException(ex, "Failed to Export Session");
				}
				goto IL_20B;
				IL_24D:
				i++;
				continue;
				IL_20B:
				num++;
				if (evtProgressNotifications == null)
				{
					goto IL_24D;
				}
				ProgressCallbackEventArgs progressCallbackEventArgs = new ProgressCallbackEventArgs((float)num / (float)oSessions.Length, "Wrote " + num.ToString() + " sessions to HTTPArchive.");
				evtProgressNotifications(null, progressCallbackEventArgs);
				if (progressCallbackEventArgs.get_Cancel())
				{
					return false;
				}
				goto IL_24D;
			}
			hashtable.Add("entries", arrayList);
			swOutput.WriteLine(JSON.JsonEncode(new Hashtable
			{

				{
					"log",
					hashtable
				}
			}));
			return true;
		}
		private static Hashtable getRequest(Session oS)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("method", oS.oRequest.get_headers().HTTPMethod);
			hashtable.Add("url", oS.get_fullUrl());
			hashtable.Add("httpVersion", oS.oRequest.get_headers().HTTPVersion);
			hashtable.Add("headersSize", oS.oRequest.get_headers().ByteCount() + 2);
			hashtable.Add("bodySize", oS.requestBodyBytes.Length);
			hashtable.Add("headers", HTTPArchiveJSONExport.getHeadersAsArrayList(oS.oRequest.get_headers()));
			hashtable.Add("cookies", HTTPArchiveJSONExport.getCookies(oS.oRequest.get_headers()));
			hashtable.Add("queryString", HTTPArchiveJSONExport.getQueryString(oS.get_fullUrl()));
			if (oS.requestBodyBytes != null && oS.requestBodyBytes.Length > 0)
			{
				hashtable.Add("postData", HTTPArchiveJSONExport.getPostData(oS));
			}
			return hashtable;
		}
		private static Hashtable getPostData(Session oS)
		{
			Hashtable hashtable = new Hashtable();
			string text = oS.oRequest.get_Item("Content-Type");
			hashtable.Add("mimeType", Utilities.TrimAfter(text, ';'));
			if (text.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
			{
				hashtable.Add("params", HTTPArchiveJSONExport.getQueryString("http://fake/path?" + oS.GetRequestBodyAsString()));
			}
			else
			{
				hashtable.Add("text", oS.GetRequestBodyAsString());
			}
			return hashtable;
		}
		private static ArrayList getCookies(HTTPHeaders oHeaders)
		{
			ArrayList arrayList = new ArrayList();
			if (oHeaders is HTTPRequestHeaders)
			{
				string text = oHeaders.get_Item("Cookie");
				if (string.IsNullOrEmpty(text))
				{
					return arrayList;
				}
				string[] array = text.Split(new char[]
				{
					';'
				}, StringSplitOptions.RemoveEmptyEntries);
				string[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					string text2 = array2[i];
					string text3 = text2.Trim();
					if (text3.Length >= 1)
					{
						arrayList.Add(new Hashtable
						{

							{
								"name",
								Utilities.TrimAfter(text3, '=')
							},

							{
								"value",
								Utilities.TrimBefore(text3, '=')
							}
						});
					}
				}
			}
			else
			{
				foreach (HTTPHeaderItem hTTPHeaderItem in oHeaders)
				{
					if (hTTPHeaderItem.Name == "Set-Cookie")
					{
						Hashtable hashtable = new Hashtable();
						string value = hTTPHeaderItem.Value;
						string text4 = Utilities.TrimAfter(value, ';');
						hashtable.Add("name", Utilities.TrimAfter(text4, '='));
						hashtable.Add("value", Utilities.TrimBefore(text4, '='));
						string text5 = Utilities.TrimBefore(value, ';');
						if (!string.IsNullOrEmpty(text5))
						{
							if (text5.IndexOf("httpOnly", StringComparison.OrdinalIgnoreCase) > -1)
							{
								hashtable.Add("httpOnly", "true");
							}
							if (text5.IndexOf("secure", StringComparison.OrdinalIgnoreCase) > -1)
							{
								hashtable.Add("_secure", "true");
							}
							Regex regex = new Regex("expires\\s?=\\s?[\"]?(?<TokenValue>[^\";]*)");
							Match match = regex.Match(text5);
							DateTime dateTime;
							if (match.Success && match.Groups["TokenValue"] != null && DateTime.TryParse(match.Groups["TokenValue"].Value, out dateTime))
							{
								hashtable.Add("expires", dateTime.ToString("o"));
							}
							regex = new Regex("domain\\s?=\\s?[\"]?(?<TokenValue>[^\";]*)");
							match = regex.Match(text5);
							if (match.Success && match.Groups["TokenValue"] != null)
							{
								hashtable.Add("domain", match.Groups["TokenValue"].Value);
							}
							regex = new Regex("path\\s?=\\s?[\"]?(?<TokenValue>[^\";]*)");
							match = regex.Match(text5);
							if (match.Success && match.Groups["TokenValue"] != null)
							{
								hashtable.Add("path", match.Groups["TokenValue"].Value);
							}
						}
						arrayList.Add(hashtable);
					}
				}
			}
			return arrayList;
		}
		private static ArrayList getQueryString(string sURI)
		{
			ArrayList arrayList = new ArrayList();
			try
			{
				Uri uri = new Uri(sURI);
				NameValueCollection nameValueCollection = Utilities.ParseQueryString(uri.Query);
				string[] allKeys = nameValueCollection.AllKeys;
				for (int i = 0; i < allKeys.Length; i++)
				{
					string text = allKeys[i];
					string[] values = nameValueCollection.GetValues(text);
					for (int j = 0; j < values.Length; j++)
					{
						string value = values[j];
						arrayList.Add(new Hashtable
						{

							{
								"name",
								text
							},

							{
								"value",
								value
							}
						});
					}
				}
			}
			catch
			{
				return new ArrayList();
			}
			return arrayList;
		}
		private static Hashtable getResponse(Session oS, bool bUseV1dot2Format)
		{
			return new Hashtable
			{

				{
					"status",
					oS.get_responseCode()
				},

				{
					"statusText",
					Utilities.TrimBefore(oS.oResponse.get_headers().HTTPResponseStatus, ' ')
				},

				{
					"httpVersion",
					oS.oResponse.get_headers().HTTPVersion
				},

				{
					"headersSize",
					oS.oResponse.get_headers().ByteCount() + 2
				},

				{
					"redirectURL",
					oS.oResponse.get_headers().get_Item("Location")
				},

				{
					"bodySize",
					(oS.responseBodyBytes == null) ? 0 : oS.responseBodyBytes.Length
				},

				{
					"headers",
					HTTPArchiveJSONExport.getHeadersAsArrayList(oS.oResponse.get_headers())
				},

				{
					"cookies",
					HTTPArchiveJSONExport.getCookies(oS.oResponse.get_headers())
				},

				{
					"content",
					HTTPArchiveJSONExport.getBodyInfo(oS, bUseV1dot2Format)
				}
			};
		}
		private static Hashtable getBodyInfo(Session oS, bool bUseV1dot2Format)
		{
			Hashtable hashtable = new Hashtable();
			int num;
			int num2;
			HTTPArchiveJSONExport.getDecompressedSize(oS, out num, out num2);
			hashtable.Add("size", num);
			hashtable.Add("compression", num2);
			hashtable.Add("mimeType", oS.oResponse.get_Item("Content-Type"));
			if (oS.responseBodyBytes == null)
			{
				return hashtable;
			}
			string mIMEType = oS.oResponse.get_MIMEType();
			bool flag = HTTPArchiveJSONExport.IsMIMETypeTextEquivalent(mIMEType);
			if (flag && "text/plain" == mIMEType && oS.responseBodyBytes.Length > 3 && ((oS.responseBodyBytes[0] == 67 && oS.responseBodyBytes[1] == 87 && oS.responseBodyBytes[2] == 83) || (oS.responseBodyBytes[0] == 70 && oS.responseBodyBytes[1] == 76 && oS.responseBodyBytes[2] == 86)))
			{
				flag = false;
			}
			if (flag)
			{
				if (oS.responseBodyBytes.Length < HTTPArchiveJSONExport._iMaxTextBodyLength)
				{
					hashtable.Add("text", oS.GetResponseBodyAsString());
				}
				else
				{
					hashtable.Add("comment", "Body length exceeded fiddler.importexport.HTTPArchiveJSON.MaxTextBodyLength, so body was omitted.");
				}
			}
			else
			{
				if (bUseV1dot2Format)
				{
					if (oS.responseBodyBytes.Length < HTTPArchiveJSONExport._iMaxBinaryBodyLength)
					{
						hashtable.Add("encoding", "base64");
						hashtable.Add("text", Convert.ToBase64String(oS.responseBodyBytes));
					}
					else
					{
						hashtable.Add("comment", "Body length exceeded fiddler.importexport.HTTPArchiveJSON.MaxBinaryBodyLength, so body was omitted.");
					}
				}
			}
			return hashtable;
		}
		private static bool IsMIMETypeTextEquivalent(string sMIME)
		{
			if (sMIME.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if (sMIME.StartsWith("application/"))
			{
				if (sMIME.StartsWith("application/javascript", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				if (sMIME.StartsWith("application/x-javascript", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				if (sMIME.StartsWith("application/ecmascript", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				if (sMIME.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				if (sMIME.StartsWith("application/xhtml+xml", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				if (sMIME.StartsWith("application/xml", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return sMIME.StartsWith("image/svg+xml", StringComparison.OrdinalIgnoreCase);
		}
		private static Hashtable getTimings(SessionTimers oTimers, bool bUseV1dot2Format)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("blocked", -1);
			hashtable.Add("dns", oTimers.DNSTime);
			hashtable.Add("connect", oTimers.TCPConnectTime + oTimers.HTTPSHandshakeTime);
			if (bUseV1dot2Format)
			{
				hashtable.Add("ssl", oTimers.HTTPSHandshakeTime);
			}
			hashtable.Add("send", Math.Max(0.0, Math.Round((oTimers.ServerGotRequest - oTimers.FiddlerBeginRequest).TotalMilliseconds)));
			hashtable.Add("wait", Math.Max(0.0, Math.Round((oTimers.ServerBeginResponse - oTimers.ServerGotRequest).TotalMilliseconds)));
			hashtable.Add("receive", Math.Max(0.0, Math.Round((oTimers.ServerDoneResponse - oTimers.ServerBeginResponse).TotalMilliseconds)));
			return hashtable;
		}
		private static int getMilliseconds(Hashtable htTimers, string sMeasure)
		{
			object obj = htTimers[sMeasure];
			if (obj == null)
			{
				return 0;
			}
			int val;
			if (obj is int)
			{
				val = (int)obj;
			}
			else
			{
				double a = (double)obj;
				val = (int)Math.Round(a);
			}
			return Math.Max(0, val);
		}
		private static int getTotalTime(Hashtable htTimers)
		{
			int num = 0;
			num += HTTPArchiveJSONExport.getMilliseconds(htTimers, "blocked");
			num += HTTPArchiveJSONExport.getMilliseconds(htTimers, "dns");
			num += HTTPArchiveJSONExport.getMilliseconds(htTimers, "connect");
			num += HTTPArchiveJSONExport.getMilliseconds(htTimers, "send");
			num += HTTPArchiveJSONExport.getMilliseconds(htTimers, "wait");
			return num + HTTPArchiveJSONExport.getMilliseconds(htTimers, "receive");
		}
		private static void getDecompressedSize(Session oSession, out int iExpandedSize, out int iCompressionSavings)
		{
			if (oSession.responseBodyBytes == null)
			{
				iExpandedSize = (iCompressionSavings = 0);
				return;
			}
			int num = oSession.responseBodyBytes.Length;
			byte[] array = (byte[])oSession.responseBodyBytes.Clone();
			try
			{
				Utilities.utilDecodeHTTPBody(oSession.oResponse.get_headers(), ref array);
			}
			catch (Exception)
			{
			}
			iExpandedSize = array.Length;
			iCompressionSavings = iExpandedSize - num;
		}
		private static ArrayList getHeadersAsArrayList(HTTPHeaders oHeaders)
		{
			ArrayList arrayList = new ArrayList();
			foreach (HTTPHeaderItem hTTPHeaderItem in oHeaders)
			{
				arrayList.Add(new Hashtable(2)
				{

					{
						"name",
						hTTPHeaderItem.Name
					},

					{
						"value",
						hTTPHeaderItem.Value
					}
				});
			}
			return arrayList;
		}
	}
}
