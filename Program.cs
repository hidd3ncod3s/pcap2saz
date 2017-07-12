using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Fiddler;
using System.Text;
//using System.Windows.Forms.TextBox;
//using BasicFormats;

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
        /*if (!flag4)
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
        }*/
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
                //int responseCode= session.responseCode;
                if (!flag3 || session.responseCode == 200)
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
                                string text2 = Utilities.TrimAfter(session.url, '?');
                                text3 = text2.Replace('/', Path.DirectorySeparatorChar);
                                if (text3.EndsWith(string.Empty + Path.DirectorySeparatorChar))
                                {
                                    text3 += session.SuggestedFilename;
                                }
                                if (text3.Length > 0 && text3.Length < 260)
                                {
                                    text3 = text + this._MakeSafeFilename(text3);
                                }
                                else
                                {
                                    text3 = text + session.SuggestedFilename;
                                }
                            }
                            else
                            {
                                text3 = text + session.SuggestedFilename;
                            }
                            text3 = text3 + "_";
                            text3 = Utilities.EnsureUniqueFilename(text3);
                            
                            byte[] array = session.responseBodyBytes;
                            if (session.oResponse.headers.Exists("Content-Encoding") || session.oResponse.headers.Exists("Transfer-Encoding"))
                            {
                                array = (byte[])array.Clone();
                                Utilities.utilDecodeHTTPBody(session.oResponse.headers, ref array);
                            }

                            Console.Write(String.Format("Writing #{0} to {1}\n", session.id.ToString(), text3));

                            Utilities.WriteArrayToFile(text3, array);
                        }
                        num++;
                        /*if (evtProgressNotifications != null)
                        {
                            ProgressCallbackEventArgs progressCallbackEventArgs = new ProgressCallbackEventArgs((float)num / (float)oSessions.Length, "Dumped " + num.ToString() + " files to disk.");
                            evtProgressNotifications(null, progressCallbackEventArgs);
                            if (progressCallbackEventArgs.get_Cancel())
                            {
                                bool result = false;
                                return result;
                            }
                        }*/
                    }
                }
            }
            catch (Exception ex2)
            {
                FiddlerApplication.ReportException(ex2, "Failed to generate response file.");
            }
        }
        /*if (flag2)
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
        }*/
        return true;
    }
    /*private void SetDefaultPath(TextBox txtUI, string sPrefName, string sDefaultPath)
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
    }*/
    public void Dispose()
    {
    }
}

namespace AnalyzePcapFile
{
    class Program
    {
        public static void WriteCommandResponse(string s)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ForegroundColor = oldColor;
        }

        public static void DoQuit()
        {
            WriteCommandResponse("Shutting down...");
            Fiddler.FiddlerApplication.Shutdown();
            Thread.Sleep(500);
        }
        private static string Ellipsize(string s, int iLen)
        {
            if (s.Length <= iLen) return s;
            return s.Substring(0, iLen - 3) + "...";
        }

        private static void ReadSessions(List<Fiddler.Session> oAllSessions)
        {
            Session[] oLoaded = Utilities.ReadSessionArchive(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) 
                                                           + Path.DirectorySeparatorChar + "ToLoad.saz", false);

            if ((oLoaded != null) && (oLoaded.Length > 0))
            {
                oAllSessions.AddRange(oLoaded);
                WriteCommandResponse("Loaded: " + oLoaded.Length + " sessions.");
            }
        }

        private static void SaveSessionsToFile(List<Fiddler.Session> oAllSessions)
        {
            bool bSuccess = false;
            string sFilename = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
                             + Path.DirectorySeparatorChar + DateTime.Now.ToString("hh-mm-ss") + ".saz";
            try
            {
                try
                {
                    Monitor.Enter(oAllSessions);

                    string sPassword = null;
                    Console.WriteLine("Password Protect this Archive (Y/N)?");
                    ConsoleKeyInfo oCKI = Console.ReadKey();
                    if ((oCKI.KeyChar == 'y') || (oCKI.KeyChar == 'Y'))
                    {
                        Console.WriteLine("\nEnter the password:");
                        sPassword = Console.ReadLine();
                        Console.WriteLine(String.Format("\nEncrypting with Password: '{0}'", sPassword));
                    }
                    Console.WriteLine();

                    bSuccess = Utilities.WriteSessionArchive(sFilename, oAllSessions.ToArray(), sPassword, false);
                }
                finally
                {
                    Monitor.Exit(oAllSessions);
                }

                WriteCommandResponse( bSuccess ? ("Wrote: " + sFilename) : ("Failed to save: " + sFilename) );
            }
            catch (Exception eX)
            {
                Console.WriteLine("Save failed: " + eX.Message);
            }
        }

        private static void WriteSessionList(List<Fiddler.Session> oAllSessions)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Session list contains...");
            try
            {
                Monitor.Enter(oAllSessions);
                foreach (Session oS in oAllSessions)
                {
                    Console.Write(String.Format("{0} {1} {2}\n{3} {4}\n\n", oS.id, oS.oRequest.headers.HTTPMethod, Ellipsize(oS.fullUrl, 60), oS.responseCode, oS.oResponse.MIMEType));
                }
            }
            finally
            {
                Monitor.Exit(oAllSessions);
            }
            Console.WriteLine();
            Console.ForegroundColor = oldColor;
        }

        private static void WriteSessionList(Fiddler.Session[] oAllSessions)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Full Session list details...");
            //string referer = ""; 

            try
            {
                Monitor.Enter(oAllSessions);
                foreach (Session oS in oAllSessions)
                {
                    //Console.Write(String.Format("{0} {1} {2}\n{3} {4}\n\n", oS.id, oS.oRequest.headers.HTTPMethod, Ellipsize(oS.fullUrl, 256), oS.responseCode, oS.oResponse.MIMEType));

                    if (oS.oRequest.headers == null)
                        continue;

                    IEnumerator<HTTPHeaderItem> hdr1 = oS.oRequest.headers.GetEnumerator();
                    hdr1.Reset();
                    //referer = "";

#if false
                    while (hdr1.MoveNext())
                    {
                        if(hdr1.Current.Name.Equals("Referer", StringComparison.OrdinalIgnoreCase)){
                            //Console.WriteLine("Session list contains...");
                            referer = hdr1.Current.Value;
                            break;
                        }
                    }
                    if(referer != "")
                        Console.Write(String.Format("{0}\n\t{1}\n\n", oS.fullUrl, referer));
                    else
                        Console.Write(String.Format("{0}\n", oS.fullUrl));
#endif
                    Console.Write(String.Format("{0}\r\n", oS.fullUrl));
                    /*while (hdr1.MoveNext())
                    {
                        Console.Write(String.Format("{0}\n", hdr1.ToString()));
                    }*/
                    Console.Write(String.Format("{0}", oS.oRequest.headers.ToString()));
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
            finally
            {
                Monitor.Exit(oAllSessions);
            }
            Console.WriteLine();
            Console.ForegroundColor = oldColor;
        }

        public static bool DoExport(string sExportFormat, Session[] oSessions, Dictionary<string, object> dictOptions, EventHandler<ProgressCallbackEventArgs> ehPCEA)
        {
            bool result = false;
            try
            {
                ISessionExporter sessionExporter = new FileDumper();
                result = sessionExporter.ExportSessions(sExportFormat, oSessions, dictOptions, ehPCEA);
                sessionExporter.Dispose();
            }
            catch (Exception eX)
            {
                //FiddlerApplication.LogAddonException(eX, "Exporter for " + sExportFormat + " failed.");
                Console.Write(String.Format("Exporter for {0} failed.", sExportFormat));
                result = false;
            }
            //FiddlerApplication.UI.UseWaitCursor = false;
            return result;
        }
        
        public static bool WriteSessionArchive(string sFilename, Session[] arrSessions, string sPassword, bool bVerboseDialogs)
		{
			if (arrSessions == null || arrSessions.Length < 1)
			{
				if (bVerboseDialogs)
				{
					FiddlerApplication.DoNotifyUser("No sessions were provided to save to the archive.", "WriteSessionArchive - No Input");
				}
				return false;
			}
			if (FiddlerApplication.oSAZProvider == null)
			{
				throw new NotSupportedException("This application was compiled without .SAZ support.");
			}
			//FiddlerApplication.DoBeforeSaveSAZ(sFilename, arrSessions);
			bool result;
			try
			{
				if (File.Exists(sFilename))
				{
					File.Delete(sFilename);
				}
				ISAZWriter iSAZWriter = FiddlerApplication.oSAZProvider.CreateSAZ(sFilename);
				if (!string.IsNullOrEmpty(sPassword))
				{
					iSAZWriter.SetPassword(sPassword);
				}
				iSAZWriter.Comment = "Fiddler Session Archive. See http://fiddler2.com";
				int num = 1;
				string sFileNumberFormat = "D" + arrSessions.Length.ToString().Length;
				for (int i = 0; i < arrSessions.Length; i++)
				{
					Session oSession = arrSessions[i];
					WriteSessionToSAZ(oSession, iSAZWriter, num, sFileNumberFormat, null, bVerboseDialogs);
					num++;
				}
				iSAZWriter.CompleteArchive();
				result = true;
			}
			catch (Exception ex)
			{
				if (bVerboseDialogs)
				{
					FiddlerApplication.DoNotifyUser("Failed to save Session Archive.\n\n" + ex.Message, "Save Failed");
				}
				result = false;
			}
			return result;
		}
        internal static void WriteSessionToSAZ(Session oSession, ISAZWriter oISW, int iFileNumber, string sFileNumberFormat, StringBuilder sbHTML, bool bVerboseDialogs)
        {
            string text = "raw\\" + iFileNumber.ToString(sFileNumberFormat);
            string text2 = text + "_c.txt";
            string text3 = text + "_s.txt";
            string text4 = text + "_m.xml";
            bool shouldicreatexmlfile = true;
            try
            {
                oISW.AddFile(text2, delegate(Stream oS)
                {
                    oSession.WriteRequestToStream(false, true, oS);
                });
            }
            catch (Exception eX)
            {
                if (bVerboseDialogs)
                {
                    //FiddlerApplication.DoNotifyUser( + Utilities.DescribeExceptionWithStack(eX), "Archive Failure");
                    Console.Write(String.Format("Unable to add " + text2 + "\n\n"));
                }
            }
            try
            {
                oISW.AddFile(text3, delegate(Stream oS)
                {
                    oSession.WriteResponseToStream(oS, false);
                });
            }
            catch (Exception eX2)
            {
                if (bVerboseDialogs)
                {
                    //FiddlerApplication.DoNotifyUser("Unable to add " + text3 + "\n\n" + Utilities.DescribeExceptionWithStack(eX2), "Archive Failure");
                    Console.Write(String.Format("Unable to add " + text3 + "\n\n"));
                }
            }
            if (shouldicreatexmlfile)
            {
                try
                {
                    oISW.AddFile(text4, delegate(Stream oS)
                    {
                        oSession.WriteMetadataToStream(oS);
                    });
                }
                catch (Exception eX3)
                {
                    if (bVerboseDialogs)
                    {
                        //FiddlerApplication.DoNotifyUser("Unable to add " + text4 + "\n\n" + Utilities.DescribeExceptionWithStack(eX3), "Archive Failure");
                        Console.Write(String.Format("Unable to add " + text4 + "\n\n"));
                    }
                }
            }
            if (oSession.bHasWebSocketMessages)
            {
                Console.Write(String.Format("[INTERESTING]Skipping websocket sessions.\n\n"));
                /*string text5 = text + "_w.txt";
                try
                {
                    oISW.AddFile(text5, delegate(Stream oS)
                    {
                        oSession.WriteWebSocketMessagesToStream(oS);
                    });
                }
                catch (Exception eX4)
                {
                    if (bVerboseDialogs)
                    {
                        FiddlerApplication.DoNotifyUser("Unable to add " + text5 + "\n\n" + Utilities.DescribeExceptionWithStack(eX4), "Archive Failure");
                    }
                }*/
            }
        }

        private static void addittothetree(ref List<TreeNode> rootNodeList, string parenturl, string childurl)
        {
            bool added = false;

            // childurl is always NOT NULL

            if (parenturl != "")
            {
                //there is a referer URL
                foreach (TreeNode RootNode in rootNodeList)
                {
                    added = RootNode.AddtoTree(parenturl, childurl);
                    if (added)
                        break;
                }

                if (!added)
                {
                    //add it to root
                    rootNodeList.Add(new TreeNode(parenturl));
                    addittothetree(ref rootNodeList, parenturl, childurl);
                    return;
                }
            } else {
                foreach (TreeNode RootNode in rootNodeList)
                {
                    if (RootNode.url.ToLower() == childurl.ToLower())
                        return;
                }
                //add it to root
                rootNodeList.Add(new TreeNode(childurl));
            }
        }

        private static void CreateAndOutputURLTraversalTree(Fiddler.Session[] oAllSessions)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Session list contains...");
            string referer = "";
            var rootNodesList = new List<TreeNode>();

            try
            {
                Monitor.Enter(oAllSessions);
                foreach (Session oS in oAllSessions)
                {
                    //Console.Write(String.Format("{0} {1} {2}\n{3} {4}\n\n", oS.id, oS.oRequest.headers.HTTPMethod, Ellipsize(oS.fullUrl, 256), oS.responseCode, oS.oResponse.MIMEType));

                    if (oS.oRequest.headers == null)
                        continue;

                    IEnumerator<HTTPHeaderItem> hdr1 = oS.oRequest.headers.GetEnumerator();
                    hdr1.Reset();
                    referer = "";

                    while (hdr1.MoveNext())
                    {
                        if (hdr1.Current.Name.Equals("Referer", StringComparison.OrdinalIgnoreCase))
                        {
                            referer = hdr1.Current.Value;
                            break;
                        }
                    }

                    //Console.Write(String.Format("{0}\n\t{1}\n\n", oS.fullUrl, referer));
                    addittothetree(ref rootNodesList, referer, oS.fullUrl);
                }
            }
            finally
            {
                Monitor.Exit(oAllSessions);
            }

            foreach (TreeNode RootNode in rootNodesList)
            {
                RootNode.PrintTree();
            }
            Console.WriteLine();
            Console.ForegroundColor = oldColor;
        }

        private static void fileteroutInterestingStreamSessions(Fiddler.Session[] oAllSessions, out Fiddler.Session[] octetStreamSessions, out Fiddler.Session[] NoUserAgentSessions)
        {
            List<Session> octetStreams = new List<Session>();
            List<Session> NoUAStreams = new List<Session>();
            bool foundBinary = false;
            bool seenanuseragent = false;

            try
            {
                Monitor.Enter(oAllSessions);
                foreach (Session oS in oAllSessions)
                {
                    //Console.Write(String.Format("{0} {1} {2}\n{3} {4}\n\n", oS.id, oS.oRequest.headers.HTTPMethod, Ellipsize(oS.fullUrl, 256), oS.responseCode, oS.oResponse.MIMEType));
                    
                    if (oS.oResponse.headers == null)
                        continue;

                    IEnumerator<HTTPHeaderItem> hdr1 = oS.oResponse.headers.GetEnumerator();
                    hdr1.Reset();
                    foundBinary = false;

                    while (hdr1.MoveNext())
                    {
                        
                        if (hdr1.Current.Name.IndexOf("content-type", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            if (hdr1.Current.Value.IndexOf("octet-stream", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                hdr1.Current.Value.IndexOf("msdownload", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                Console.Write(String.Format("Exe file= {0}\n", hdr1.Current.Value));
                                foundBinary = true;
                                break;
                            }
                        }

                    }

                    if (foundBinary == true)
                        octetStreams.Add(oS);
                }
            }
            finally
            {
                Monitor.Exit(oAllSessions);
            }


            try
            {
                Monitor.Enter(oAllSessions);
                foreach (Session oS in oAllSessions)
                {
                    //Console.Write(String.Format("{0} {1} {2}\n{3} {4}\n\n", oS.id, oS.oRequest.headers.HTTPMethod, Ellipsize(oS.fullUrl, 256), oS.responseCode, oS.oResponse.MIMEType));

                    if (oS.oRequest.headers == null)
                        continue;

                    IEnumerator<HTTPHeaderItem> hdr1 = oS.oRequest.headers.GetEnumerator();
                    hdr1.Reset();
                    seenanuseragent = false;

                    while (hdr1.MoveNext())
                    {
                        //Console.Write(String.Format("{0}\n", hdr1.Current.Name));
                        if (hdr1.Current.Name.IndexOf("user-agent", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            //Console.Write(String.Format("Matched {0}\n", hdr1.Current.Name));
                            seenanuseragent = true;
                            break;
                        }
                    }

                    if (seenanuseragent == false)
                    {
                        NoUAStreams.Add(oS);
                        //Console.Write("[hidd3ncod3s]Added a session\n");
                    }
                }
            }
            finally
            {
                Monitor.Exit(oAllSessions);
            }

            octetStreamSessions= octetStreams.ToArray();
            NoUserAgentSessions = NoUAStreams.ToArray();
        }
        static void Main(string[] args)
        {
            ArgumentParser.InputArguments arguments = new ArgumentParser.InputArguments(args);

            if (args.Length < 1 )
            {
                //"%s <fullpathofpcap>\n\n"
                Console.Write(String.Format("{0} <fullpathofpcap>\n", System.AppDomain.CurrentDomain.FriendlyName));
                return;
            }
            int iProcCount = Environment.ProcessorCount;
            int iMinWorkerThreads = Math.Max(16, 6 * iProcCount);
            int iMinIOThreads = iProcCount;
            ThreadPool.SetMinThreads(iMinWorkerThreads, iMinIOThreads);

            List<Fiddler.Session> oAllSessions = new List<Fiddler.Session>();

            // <-- Personalize for your Application, 64 chars or fewer
            //Fiddler.FiddlerApplication.SetAppDisplayName("AnalyzePcapFile");

            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);

            DNZSAZProvider.fnObtainPwd = () =>
            {
                Console.WriteLine("Enter the password (or just hit Enter to cancel):");
                string sResult = Console.ReadLine();
                Console.WriteLine();
                return sResult;
            };

            FiddlerApplication.oSAZProvider = new DNZSAZProvider();

            FiddlerApplication.Prefs.SetBoolPref("fiddler.network.streaming.abortifclientaborts", true);

            Dictionary<string, object> dictOptions = new Dictionary<string, object>();
            dictOptions.Add("Filename", args[0]);
            dictOptions.Add("Quiet", "True");

            BasicFormats.PacketCaptureImport pcapreader = new BasicFormats.PacketCaptureImport();
            Session[] pcapSessions= pcapreader.ImportSessions("Packet Capture", dictOptions, null);
            Session[] octetStreamSessions;
            Session[] NoUserAgentSessions;

            if (pcapSessions != null && pcapSessions.Length > 0)
            {
                Console.WriteLine("Successfully parsed PCAP File  " + args[0]);
                CreateAndOutputURLTraversalTree(pcapSessions);
                Console.WriteLine("=============================================================");
                //WriteSessionList(pcapSessions);
                WriteSessionArchive(args[0] + ".saz", pcapSessions, "", true);
            }
            else
            {
                Console.WriteLine("ERROR parsing PCAP file  " + args[0]);
            }
        }

        /// <summary>
        /// When the user hits CTRL+C, this event fires.  We use this to shut down and unregister our FiddlerCore.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            DoQuit();
        }
    }
}

