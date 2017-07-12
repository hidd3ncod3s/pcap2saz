using Fiddler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BasicFormats
{
	[ProfferFormat("Packet Capture", "The PCAP and CAP formats are used by WireShark, Microsoft Network Monitor, Message Analyzer, and other low-level capture tools. Only (unencrypted) HTTP traffic can be imported.")]
	public class PacketCaptureImport : ISessionImporter, IDisposable
	{
		internal enum FileType : byte
		{
			Unknown,
			PCAP,
			PCAPNG,
			CAP
		}
		public struct PacketCounts
		{
			public uint Total;
			public uint IPv4;
			public uint IPv6;
			public uint IPFragments;
			public uint UDP;
			public uint TCP;
			public override string ToString()
			{
				return string.Format("\tTotal:\t{0}\nipv4:\t{1}\tFragments: {2}\nipv6:\t{3}\nudp:\t{4}\ntcp:\t{5}\n", new object[]
				{
					this.Total,
					this.IPv4,
					this.IPFragments,
					this.IPv6,
					this.UDP,
					this.TCP
				});
			}
		}
		internal static bool bDebug;
		internal static bool bVerboseDebug;
		internal static bool bSilent;
		internal static EventHandler<ProgressCallbackEventArgs> evtOnProgress;
		internal PacketCaptureImport.FileType loadedFileType;
		public Session[] ImportSessions(string sFormat, Dictionary<string, object> dictOptions, EventHandler<ProgressCallbackEventArgs> evtPN)
		{
			if (sFormat != "Packet Capture")
			{
				return null;
			}
			PacketCaptureImport.evtOnProgress = evtPN;
			Session[] result;
			try
			{
                PacketCaptureImport.bDebug = true;
                PacketCaptureImport.bVerboseDebug = true;
				string text = null;
				if (dictOptions != null)
				{
					if (dictOptions.ContainsKey("Filename"))
					{
						text = (dictOptions["Filename"] as string);
					}
					if (dictOptions.ContainsKey("Quiet"))
					{
						PacketCaptureImport.bSilent = StringExtensions.OICEquals("True", dictOptions["Quiet"] as string);
                        PacketCaptureImport.bDebug = StringExtensions.OICEquals("True", dictOptions["Quiet"] as string);
                        PacketCaptureImport.bVerboseDebug = StringExtensions.OICEquals("True", dictOptions["Quiet"] as string);
					}
				}
				if (string.IsNullOrEmpty(text))
				{
					text = Utilities.ObtainOpenFilename("Import from " + sFormat, "Packet Capture (*.cap;*.pcap;*.pcapng)|*.cap;*.pcap;*.pcapng");
				}

				if (string.IsNullOrEmpty(text))
				{
					result = null;
				}
				else
				{
					try
					{
						using (FileStream fileStream = new FileStream(text, FileMode.Open, FileAccess.Read))
						{
							BinaryReader binaryReader = new BinaryReader(fileStream);
							uint num = binaryReader.ReadUInt32();
							uint num2 = num;
							if (num2 <= 1397970002u)
							{
								if (num2 == 168627466u)
								{
									this.loadedFileType = PacketCaptureImport.FileType.PCAPNG;
									result = this.GetSessionsFromPCAPNG(binaryReader, num);
									return result;
								}
								if (num2 != 1295823521u)
								{
									if (num2 != 1397970002u)
									{
										goto IL_192;
									}
									this.loadedFileType = PacketCaptureImport.FileType.CAP;
									if (!PacketCaptureImport.bSilent)
									{
										Console.WriteLine(String.Format("This Importer currently requires NetMon v2.x format. v1.x files like this one cannot be parsed.\nPlease use Help > Send Feedback for more information.", "Unsupported File Format"));
									}
									result = null;
									return result;
								}
							}
							else
							{
								if (num2 <= 2712812621u)
								{
									if (num2 == 1430408519u)
									{
										this.loadedFileType = PacketCaptureImport.FileType.CAP;
										result = this.GetSessionsFromNetMonCAP(binaryReader, num);
										return result;
									}
									if (num2 != 2712812621u)
									{
										goto IL_192;
									}
								}
								else
								{
									if (num2 != 2712847316u && num2 != 3569595041u)
									{
										goto IL_192;
									}
								}
							}
							this.loadedFileType = PacketCaptureImport.FileType.PCAP;
							result = this.GetSessionsFromPCAP(binaryReader, num);
							return result;
							IL_192:
							if (!PacketCaptureImport.bSilent)
							{
								Console.WriteLine(String.Format(string.Format("Unable to parse capture file; Magic Bytes: 0x{0:x}", num), "Unknown File Format"));
							}
							result = null;
						}
					}
					catch (Exception ex)
					{
						//FiddlerApplication.ReportException(ex, "Unable to Import", "The Packet Capture could not be imported.");
                        Console.WriteLine(String.Format(string.Format("Unable to Import: The Packet Capture could not be imported.")));
						result = null;
					}
				}
			}
			finally
			{
				PacketCaptureImport.evtOnProgress = null;
			}
			return result;
		}
		public void Dispose()
		{
		}
		private Session[] GetSessionsFromPackets(ref PacketCaptureImport.PacketCounts pcCounts, Dictionary<string, TCPStream> dictTCPConns)
		{
            //FiddlerApplication.get_Log().LogFormat("PACKET COUNTER\n\n{0}\n", new object[]
            //{
            //    pcCounts
            //});
			if (pcCounts.IPv4 + pcCounts.IPv6 < 1u)
			{
				string text = "No IPv4 or IPv6 traffic was found in this capture.";
				if (this.loadedFileType == PacketCaptureImport.FileType.CAP)
				{
					text += "\n\nIf this capture was collected using Message Analyzer, please instead use its 'Link Local' capture mode instead of 'Firewall' mode.";
				}
				if (!PacketCaptureImport.bSilent)
				{
					Console.WriteLine(String.Format(text, "No traffic found"));
				}
				return null;
			}
			if (PacketCaptureImport.evtOnProgress != null)
			{
				ProgressCallbackEventArgs progressCallbackEventArgs = new ProgressCallbackEventArgs(0.4f, "Finished processing raw packets...");
				PacketCaptureImport.evtOnProgress(null, progressCallbackEventArgs);
				if (progressCallbackEventArgs.Cancel == true)
				{
					return null;
				}
			}
			PacketCaptureImport._PairTCPStreams(dictTCPConns);
			if (PacketCaptureImport.evtOnProgress != null)
			{
				ProgressCallbackEventArgs progressCallbackEventArgs2 = new ProgressCallbackEventArgs(0.6f, "Finished assembling streams...");
				PacketCaptureImport.evtOnProgress(null, progressCallbackEventArgs2);
                if (progressCallbackEventArgs2.Cancel == true)
				{
					return null;
				}
			}
			return PacketCaptureImport._DumpTCPTraffic(dictTCPConns);
		}
		private Session[] GetSessionsFromPCAP(BinaryReader rdr, uint uiMagic)
		{
			PCAPFileHeader pCAPFileHeader = PCAPFileHeader.CreateFromReader(rdr, uiMagic);
			if (pCAPFileHeader.IsByteOrderSwapped && !PacketCaptureImport.bSilent)
			{
				Console.WriteLine(String.Format("Warning: Byte order swapped. Import will fail!", "FILE FORMAT WARNING"));
			}
			if (pCAPFileHeader.LinkType != 1u && !PacketCaptureImport.bSilent)
			{
				Console.WriteLine(String.Format("Warning: Link Type is not Ethernet. Import likely to fail!", "FILE FORMAT WARNING"));
			}

            //Console.WriteLine(String.Format("Importing PCAP:\n{0}", pCAPFileHeader));

			PacketCaptureImport.PacketCounts packetCounts = default(PacketCaptureImport.PacketCounts);
			Dictionary<uint, DNSTransaction> dictionary = new Dictionary<uint, DNSTransaction>();
			Dictionary<string, TCPStream> dictionary2 = new Dictionary<string, TCPStream>();
			long length = rdr.BaseStream.Length;
			bool flag = false;
            //Console.WriteLine(String.Format("length #{0}\n", length));
			while (!flag && rdr.BaseStream.Position + 16L <= length)
			{
				packetCounts.Total += 1u;
                //Console.WriteLine(String.Format("packetCounts.Total : {0}", packetCounts.Total));
				PCAPPacketHeader pCAPPacketHeader = PCAPPacketHeader.CreateFromReader(rdr);
				if (14u > pCAPPacketHeader.PacketSavedSize)
				{
                    //Console.WriteLine("14u > pCAPPacketHeader.PacketSavedSize");
					rdr.BaseStream.Position += (long)pCAPPacketHeader.PacketSavedSize;
				}
				else
				{
					if (pCAPPacketHeader.PacketOriginalSize != pCAPPacketHeader.PacketSavedSize)
					{
                        Console.WriteLine(String.Format("! WARNING: Packet{0} was not stored completely. Stored only {1}/{2} bytes", packetCounts.Total, pCAPPacketHeader.PacketSavedSize,
                            pCAPPacketHeader.PacketOriginalSize));
					}
					byte[] array = rdr.ReadBytes((int)pCAPPacketHeader.PacketSavedSize);
                    
                    //hidd3ncodes: Trying to skip this packet.
                    if (pCAPPacketHeader.PacketOriginalSize != pCAPPacketHeader.PacketSavedSize)
                    {
                        continue;
                    }

					if ((long)array.Length != (long)((ulong)pCAPPacketHeader.PacketSavedSize))
					{
                        Console.WriteLine(String.Format("! WARNING: File was incomplete. Last frame stored only {0}/{1} bytes", array.Length,pCAPPacketHeader.PacketSavedSize));
						flag = true;
					}
					else
					{
						IPFrame iPFrame = IPFrame.ParseAsIPFrame(packetCounts.Total, array, pCAPPacketHeader.dtPacket);
						if (iPFrame != null)
						{
							if (iPFrame.IPVersion == 4)
							{
								packetCounts.IPv4 += 1u;
							}
							else
							{
								if (iPFrame.IPVersion == 6)
								{
									packetCounts.IPv6 += 1u;
								}
							}
                            
							IPSubProtocols nextProtocol = iPFrame.NextProtocol;
							if (nextProtocol != IPSubProtocols.TCP)
							{
								if (nextProtocol != IPSubProtocols.UDP)
								{
									if (nextProtocol == IPSubProtocols.ESP)
									{
										//if (PacketCaptureImport.bVerboseDebug)
										//{
                                            Console.WriteLine(String.Format("ESP Frame #{0} skipped; parsing NYI", iPFrame.iFrameNumber));
										//}
									}
								}
								else
								{
									UDPMessage uDPMessage = UDPMessage.Parse(iPFrame, array);
									packetCounts.UDP += 1u;
									if (WellKnownPorts.DNS == uDPMessage.DstPort)
									{
										DNSQuery dNSQuery = DNSQuery.Parse(iPFrame, array);
										if (dNSQuery.QueryType == DNSQueryType.AddressQuery)
										{
											DNSTransaction dNSTransaction;
											if (!dictionary.TryGetValue(dNSQuery.uiTransactionID, out dNSTransaction))
											{
												dNSTransaction = new DNSTransaction();
												dictionary.Add(dNSQuery.uiTransactionID, dNSTransaction);
											}
											dNSTransaction.uiTransactionID = dNSQuery.uiTransactionID;
											dNSTransaction.sQueryForHostname = dNSQuery.sHostname;
											dNSTransaction.bAAAAQuery = (dNSQuery.QueryType == DNSQueryType.AAAA);
											dNSTransaction.dtQuerySent = pCAPPacketHeader.dtPacket;
										}
									}
									else
									{
										if (WellKnownPorts.DNS == uDPMessage.SrcPort)
										{
											DNSResponse dNSResponse = DNSResponse.Parse(iPFrame, array);
											DNSTransaction dNSTransaction2;
											if (dictionary.TryGetValue(dNSResponse.uiTransactionID, out dNSTransaction2))
											{
												dNSTransaction2.dtResponseReceived = pCAPPacketHeader.dtPacket;
											}
										}
									}
								}
							}
							else
							{
								TCPFrame tCPFrame = TCPFrame.Parse(iPFrame, array);
                                //Console.WriteLine(String.Format("I am here", length));
								if (tCPFrame != null)
								{
									packetCounts.TCP += 1u;
									TCPEndpoints tCPEndpoints = new TCPEndpoints(iPFrame.ipSrc, iPFrame.ipDest, tCPFrame.SrcPort, tCPFrame.DstPort);
									string key = tCPEndpoints.ToString();
									TCPStream tCPStream;
									if (!dictionary2.TryGetValue(key, out tCPStream))
									{
										tCPStream = new TCPStream(tCPEndpoints);
										dictionary2.Add(key, tCPStream);
									}
									tCPStream.AddFrame(tCPFrame);
								}
							}
						}
					}
				}
                //Console.WriteLine(String.Format("Flag #{0}", flag));
                //Console.WriteLine(String.Format("rdr.BaseStream.Position #{0}", rdr.BaseStream.Position));
			}
			return this.GetSessionsFromPackets(ref packetCounts, dictionary2);
		}
		private Session[] GetSessionsFromPCAPNG(BinaryReader rdr, uint uiMagic)
		{
			PCAPNGFileHeader pCAPNGFileHeader = PCAPNGFileHeader.CreateFromReader(rdr, uiMagic);
			if (pCAPNGFileHeader.ByteOrderMagic != 439041101u)
			{
				Console.WriteLine(String.Format("Sorry, this format is not yet supported. Please contact support.", "Unsupported Byte-Order"));
				return null;
			}
			if (pCAPNGFileHeader.SectionHeaderLength != pCAPNGFileHeader.RepeatedHeaderLength)
			{
				Console.WriteLine(String.Format("Sorry, this file appears to be corrupt. HeaderLength != TrailingHeaderLength. Please contact support.", "File Corrupt"));
				return null;
			}
            //Console.WriteLine(String.Format("Importing PCAPNG:\n{0}", pCAPNGFileHeader);

			PacketCaptureImport.PacketCounts packetCounts = default(PacketCaptureImport.PacketCounts);
			Dictionary<uint, DNSTransaction> dictionary = new Dictionary<uint, DNSTransaction>();
			Dictionary<string, TCPStream> dictionary2 = new Dictionary<string, TCPStream>();
			long length = rdr.BaseStream.Length;
			bool flag = false;
			while (!flag && rdr.BaseStream.Position + 8L <= length)
			{
				PCAPNGBlockHeader pCAPNGBlockHeader = PCAPNGBlockHeader.CreateFromReader(rdr);
				PCAPNGBlockType uiBlockType = pCAPNGBlockHeader.uiBlockType;
				if (uiBlockType != PCAPNGBlockType.InterfaceDescriptionBlock)
				{
					if (uiBlockType != PCAPNGBlockType.EnhancedPacketBlock)
					{
						//FiddlerApplication.get_Log().LogString(pCAPNGBlockHeader.ToString());
						rdr.ReadBytes((int)(pCAPNGBlockHeader.uiBlockLength - 8u));
					}
					else
					{
						packetCounts.Total += 1u;
						PCAPNGEnhancedPacketHeader pCAPNGEnhancedPacketHeader = PCAPNGEnhancedPacketHeader.CreateFromReader(rdr);
						if (pCAPNGEnhancedPacketHeader.PacketOriginalSize != pCAPNGEnhancedPacketHeader.PacketSavedSize)
						{
                            //FiddlerApplication.get_Log().LogFormat("! WARNING: Packet was not stored completely. Stored only {0}/{1} bytes", new object[]
                            //{
                            //    pCAPNGEnhancedPacketHeader.PacketSavedSize,
                            //    pCAPNGEnhancedPacketHeader.PacketOriginalSize
                            //});
						}
						byte[] array = rdr.ReadBytes((int)pCAPNGEnhancedPacketHeader.PacketSavedSize);
						if ((long)array.Length != (long)((ulong)pCAPNGEnhancedPacketHeader.PacketSavedSize))
						{
                            //FiddlerApplication.get_Log().LogFormat("! WARNING: File was incomplete. Last frame stored only {0}/{1} bytes", new object[]
                            //{
                            //    array.Length,
                            //    pCAPNGEnhancedPacketHeader.PacketSavedSize
                            //});
							flag = true;
						}
						else
						{
							IPFrame iPFrame = IPFrame.ParseAsIPFrame(packetCounts.Total, array, pCAPNGEnhancedPacketHeader.dtPacket);
							if (iPFrame == null)
							{
								rdr.ReadBytes((int)((ulong)(pCAPNGBlockHeader.uiBlockLength - 28u) - (ulong)((long)pCAPNGEnhancedPacketHeader.PacketSavedSize)));
							}
							else
							{
								if (iPFrame.IPVersion == 4)
								{
									packetCounts.IPv4 += 1u;
								}
								else
								{
									if (iPFrame.IPVersion == 6)
									{
										packetCounts.IPv6 += 1u;
									}
								}
								IPSubProtocols nextProtocol = iPFrame.NextProtocol;
								if (nextProtocol != IPSubProtocols.TCP)
								{
									if (nextProtocol != IPSubProtocols.UDP)
									{
										if (nextProtocol == IPSubProtocols.ESP)
										{
											if (PacketCaptureImport.bVerboseDebug)
											{
                                                //FiddlerApplication.get_Log().LogFormat("ESP Frame #{0} skipped; parsing NYI", new object[]
                                                //{
                                                //    iPFrame.iFrameNumber
                                                //});
											}
										}
									}
									else
									{
										UDPMessage uDPMessage = UDPMessage.Parse(iPFrame, array);
										packetCounts.UDP += 1u;
										if (WellKnownPorts.DNS == uDPMessage.DstPort)
										{
											DNSQuery dNSQuery = DNSQuery.Parse(iPFrame, array);
											if (dNSQuery.QueryType == DNSQueryType.AddressQuery)
											{
												DNSTransaction dNSTransaction;
												if (!dictionary.TryGetValue(dNSQuery.uiTransactionID, out dNSTransaction))
												{
													dNSTransaction = new DNSTransaction();
													dictionary.Add(dNSQuery.uiTransactionID, dNSTransaction);
												}
												dNSTransaction.uiTransactionID = dNSQuery.uiTransactionID;
												dNSTransaction.sQueryForHostname = dNSQuery.sHostname;
												dNSTransaction.bAAAAQuery = (dNSQuery.QueryType == DNSQueryType.AAAA);
												dNSTransaction.dtQuerySent = pCAPNGEnhancedPacketHeader.dtPacket;
											}
										}
										else
										{
											if (WellKnownPorts.DNS == uDPMessage.SrcPort)
											{
												DNSResponse dNSResponse = DNSResponse.Parse(iPFrame, array);
												DNSTransaction dNSTransaction2;
												if (dictionary.TryGetValue(dNSResponse.uiTransactionID, out dNSTransaction2))
												{
													dNSTransaction2.dtResponseReceived = pCAPNGEnhancedPacketHeader.dtPacket;
												}
											}
										}
									}
								}
								else
								{
									TCPFrame tCPFrame = TCPFrame.Parse(iPFrame, array);
									if (tCPFrame == null)
									{
										continue;
									}
									packetCounts.TCP += 1u;
									TCPEndpoints tCPEndpoints = new TCPEndpoints(iPFrame.ipSrc, iPFrame.ipDest, tCPFrame.SrcPort, tCPFrame.DstPort);
									string key = tCPEndpoints.ToString();
									TCPStream tCPStream;
									if (!dictionary2.TryGetValue(key, out tCPStream))
									{
										tCPStream = new TCPStream(tCPEndpoints);
										dictionary2.Add(key, tCPStream);
									}
									tCPStream.AddFrame(tCPFrame);
								}
								int count = (int)((ulong)(pCAPNGBlockHeader.uiBlockLength - 28u) - (ulong)((long)pCAPNGEnhancedPacketHeader.PacketSavedSize));
								rdr.ReadBytes(count);
							}
						}
					}
				}
				else
				{
					//FiddlerApplication.get_Log().LogString(pCAPNGBlockHeader.ToString());
					rdr.ReadBytes((int)(pCAPNGBlockHeader.uiBlockLength - 8u));
				}
			}
			return this.GetSessionsFromPackets(ref packetCounts, dictionary2);
		}
		private Session[] GetSessionsFromNetMonCAP(BinaryReader rdr, uint uiMagic)
		{
			NetMonFileHeader netMonFileHeader = NetMonFileHeader.CreateFromReader(rdr, uiMagic);
			uint[] frameOffsets = netMonFileHeader.GetFrameOffsets(rdr);
			PacketCaptureImport.PacketCounts packetCounts = default(PacketCaptureImport.PacketCounts);
			Dictionary<uint, DNSTransaction> dictionary = new Dictionary<uint, DNSTransaction>();
			Dictionary<string, TCPStream> dictionary2 = new Dictionary<string, TCPStream>();
			uint num = 0u;
			while ((ulong)num < (ulong)((long)frameOffsets.Length))
			{
				packetCounts.Total += 1u;
				rdr.BaseStream.Position = (long)((ulong)frameOffsets[(int)((UIntPtr)num)]);
				NetmonPacketHeader netmonPacketHeader = NetmonPacketHeader.CreateFromReader(rdr, netMonFileHeader.dtCapture);
				if (netmonPacketHeader.MediaType != MediaTypes.Ethernet && netmonPacketHeader.MediaType != MediaTypes.WFPCapture_Message2V4)
				{
					if (PacketCaptureImport.bVerboseDebug)
					{
                        //FiddlerApplication.get_Log().LogFormat("Skipping frame {0} with MediaType: 0x{1:x}", new object[]
                        //{
                        //    num,
                        //    netmonPacketHeader.MediaType
                        //});
					}
				}
				else
				{
					byte[] array = new byte[netmonPacketHeader.PacketSavedSize];
					rdr.BaseStream.Position = (long)((ulong)(frameOffsets[(int)((UIntPtr)num)] + 16u));
					rdr.BaseStream.Read(array, 0, array.Length);
					IPFrame iPFrame;
					if (netmonPacketHeader.MediaType == MediaTypes.WFPCapture_Message2V4)
					{
						iPFrame = IPFrame.FakeAsIPFrame(num, array, netmonPacketHeader.dtPacket);
					}
					else
					{
						iPFrame = IPFrame.ParseAsIPFrame(num, array, netmonPacketHeader.dtPacket);
					}
					if (iPFrame != null)
					{
						if (iPFrame.IPVersion == 4)
						{
							packetCounts.IPv4 += 1u;
						}
						else
						{
							if (iPFrame.IPVersion == 6)
							{
								packetCounts.IPv6 += 1u;
							}
						}
						if (PacketCaptureImport.bVerboseDebug)
						{
                            //FiddlerApplication.get_Log().LogFormat("Adding frame {0} - {1}", new object[]
                            //{
                            //    num,
                            //    iPFrame.ToString()
                            //});
						}
						IPSubProtocols nextProtocol = iPFrame.NextProtocol;
						if (nextProtocol != IPSubProtocols.TCP)
						{
							if (nextProtocol != IPSubProtocols.UDP)
							{
								if (nextProtocol == IPSubProtocols.ESP)
								{
									if (PacketCaptureImport.bVerboseDebug)
									{
                                        //FiddlerApplication.get_Log().LogFormat("ESP Frame #{0} skipped; parsing NYI", new object[]
                                        //{
                                        //    iPFrame.iFrameNumber
                                        //});
									}
								}
							}
							else
							{
								UDPMessage uDPMessage = UDPMessage.Parse(iPFrame, array);
								packetCounts.UDP += 1u;
								if (WellKnownPorts.DNS == uDPMessage.DstPort)
								{
									DNSQuery dNSQuery = DNSQuery.Parse(iPFrame, array);
									if (dNSQuery.QueryType == DNSQueryType.AddressQuery)
									{
										DNSTransaction dNSTransaction;
										if (!dictionary.TryGetValue(dNSQuery.uiTransactionID, out dNSTransaction))
										{
											dNSTransaction = new DNSTransaction();
											dictionary.Add(dNSQuery.uiTransactionID, dNSTransaction);
										}
										dNSTransaction.uiTransactionID = dNSQuery.uiTransactionID;
										dNSTransaction.sQueryForHostname = dNSQuery.sHostname;
										dNSTransaction.bAAAAQuery = (dNSQuery.QueryType == DNSQueryType.AAAA);
										dNSTransaction.dtQuerySent = netmonPacketHeader.dtPacket;
									}
								}
								else
								{
									if (WellKnownPorts.DNS == uDPMessage.SrcPort)
									{
										DNSResponse dNSResponse = DNSResponse.Parse(iPFrame, array);
										DNSTransaction dNSTransaction2;
										if (dictionary.TryGetValue(dNSResponse.uiTransactionID, out dNSTransaction2))
										{
											dNSTransaction2.dtResponseReceived = netmonPacketHeader.dtPacket;
										}
									}
								}
							}
						}
						else
						{
							TCPFrame tCPFrame = TCPFrame.Parse(iPFrame, array);
							if (tCPFrame != null)
							{
								packetCounts.TCP += 1u;
								TCPEndpoints tCPEndpoints = new TCPEndpoints(iPFrame.ipSrc, iPFrame.ipDest, tCPFrame.SrcPort, tCPFrame.DstPort);
								string key = tCPEndpoints.ToString();
								TCPStream tCPStream;
								if (!dictionary2.TryGetValue(key, out tCPStream))
								{
									tCPStream = new TCPStream(tCPEndpoints);
									uint processTableIndex = netmonPacketHeader.ProcessTableIndex;
									if ((ulong)processTableIndex < (ulong)((long)netMonFileHeader.arrProcesses.Length))
									{
										tCPStream.sProcessInfo = netMonFileHeader.arrProcesses[(int)((UIntPtr)processTableIndex)];
									}
									dictionary2.Add(key, tCPStream);
								}
								tCPStream.AddFrame(tCPFrame);
							}
						}
					}
				}
				num += 1u;
			}
			return this.GetSessionsFromPackets(ref packetCounts, dictionary2);
		}
		private static void _PairTCPStreams(Dictionary<string, TCPStream> dictTCP)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (KeyValuePair<string, TCPStream> current in dictTCP)
			{
				current.Value.SortMessages();
				if (!current.Value.IsComplete)
				{
					if (PacketCaptureImport.bDebug)
					{
                        //FiddlerApplication.get_Log().LogFormat("!Incomplete Stream {0}\n", new object[]
                        //{
                        //    current.Key
                        //});
					}
					num2++;
				}
				if (current.Value.tcpsPeer == null)
				{
					TCPStream tCPStream;
					if (dictTCP.TryGetValue(current.Value.tcpEndpoints.ComputePeerId(), out tCPStream))
					{
						current.Value.tcpsPeer = tCPStream;
						tCPStream.tcpsPeer = current.Value;
						num3++;
						current.Value.SniffStream();
						tCPStream.SniffStream();
						if (PacketCaptureImport.bDebug)
						{
                            //FiddlerApplication.get_Log().LogFormat("Sniffed {0} in {1}", new object[]
                            //{
                            //    current.Value.ctContentType,
                            //    current.Value.sStreamID
                            //});
						}
					}
					else
					{
						if (PacketCaptureImport.bDebug)
						{
                            //FiddlerApplication.get_Log().LogFormat("!Unable to find peer for {0}\n", new object[]
                            //{
                            //    current.Key
                            //});
						}
						current.Value.SniffStream();
						num++;
					}
				}
			}
			if (PacketCaptureImport.bDebug)
			{
                //FiddlerApplication.get_Log().LogFormat("Found {0} bidirectional TCP Streams\n", new object[]
                //{
                //    num3
                //});
			}
			if (num > 0)
			{
                //FiddlerApplication.get_Log().LogFormat("! Found {0} TCP Streams without peer!\n", new object[]
                //{
                //    num
                //});
			}
			if (num2 > 0)
			{
                //FiddlerApplication.get_Log().LogFormat("! Found {0} Incomplete TCP Streams!\n", new object[]
                //{
                //    num2
                //});
			}
		}
		private static Session[] _DumpTCPTraffic(Dictionary<string, TCPStream> dictTCP)
		{
			List<Session> list = new List<Session>();
			foreach (KeyValuePair<string, TCPStream> current in dictTCP)
			{
				TCPStream value = current.Value;
				TCPStream tcpsPeer = current.Value.tcpsPeer;
				if (tcpsPeer == null || tcpsPeer.IsServer)
				{
					TCPStreamContent tCPStreamContent = current.Value.ctContentType & ~(TCPStreamContent.Client | TCPStreamContent.Server);
					if (tCPStreamContent != TCPStreamContent.HTTP)
					{
						if (tCPStreamContent == TCPStreamContent.SSLTLS)
						{
							list.AddRange(PacketCaptureImport.GetHTTPSHandshakeFromStreams(value, tcpsPeer));
						}
					}
					else
					{
						list.AddRange(PacketCaptureImport.GetSessionsFromStreams(value, tcpsPeer));
					}
				}
			}
			list.Sort((Session a, Session b) => a.Timers.ClientBeginRequest.CompareTo(b.Timers.ClientBeginRequest));
			foreach (Session current2 in list)
			{
				current2.state = (SessionStates)11;
			}
			return list.ToArray();
		}
		public static Session[] GetHTTPSHandshakeFromStreams(TCPStream tcpsClient, TCPStream tcpsServer)
		{
			Session[] result;
			try
			{
				if (tcpsClient == null)
				{
					result = new Session[0];
				}
				else
				{
					MemoryStream payloadStream = tcpsClient.GetPayloadStream(null, 1024);
					payloadStream.Position = 0L;
					MemoryStream memoryStream = null;
					if (tcpsServer != null)
					{
						memoryStream = tcpsServer.GetPayloadStream(null, 1024);
						memoryStream.Position = 0L;
					}
					string text = Utilities.UNSTABLE_DescribeClientHello(payloadStream);
					string text2 = "No server response was found";
					if (memoryStream != null && memoryStream.Length > 0L)
					{
						text2 = Utilities.UNSTABLE_DescribeServerHello(memoryStream);
					}
					if (string.IsNullOrEmpty(text) && (text2 == null || text2.Length < 48))
					{
						result = new Session[0];
					}
					else
					{
						HTTPRequestHeaders hTTPRequestHeaders = new HTTPRequestHeaders();
						hTTPRequestHeaders.HTTPMethod = "CONNECT";
						hTTPRequestHeaders.RequestPath= string.Format("{0}:{1}", tcpsClient.tcpEndpoints.addrDst, tcpsClient.tcpEndpoints.iDstPort);
						hTTPRequestHeaders.Add("Host", hTTPRequestHeaders.RequestPath);
						hTTPRequestHeaders.Add("Fiddler-Import", "Packet capture contained HTTPS traffic. Parsing HTTPS Handshake to show this mock Session.");
						HTTPResponseHeaders hTTPResponseHeaders = new HTTPResponseHeaders();
						hTTPResponseHeaders.SetStatus(200, "Emulated CONNECT Tunnel");
						Session session = new Session(hTTPRequestHeaders, Encoding.UTF8.GetBytes(text));
                        session.oResponse.headers = hTTPResponseHeaders;
						session.responseBodyBytes = Encoding.UTF8.GetBytes(text2);
						session.oFlags["X-EgressPort"] = (session.oFlags["X-ClientPort"] = tcpsClient.tcpEndpoints.iSrcPort.ToString());
						session.oFlags["X-ClientIP"] = tcpsClient.tcpEndpoints.addrSrc.ToString();
						session.oFlags["X-HostIP"] = tcpsClient.tcpEndpoints.addrDst.ToString();
						session.oFlags["X-HostPort"] = tcpsClient.tcpEndpoints.iDstPort.ToString();
						session.Timers.ClientConnected = tcpsClient.dtConnectStart;
						session.Timers.ClientBeginRequest = tcpsClient.dtFirstPayload;
						session.Timers.FiddlerGotRequestHeaders = (session.Timers.FiddlerGotResponseHeaders = new DateTime(0L));
						session.Timers.ServerConnected = tcpsServer.dtConnectStart;
						session.Timers.ServerBeginResponse = tcpsServer.dtFirstPayload;
						string sProcessInfo = tcpsClient.sProcessInfo;
						if (!string.IsNullOrEmpty(sProcessInfo))
						{
							session.oFlags["X-ProcessInfo"] = sProcessInfo;
						}
                        session.UNSTABLE_SetBitFlag((SessionFlags)1, true);
						result = new Session[]
						{
							session
						};
					}
				}
			}
			catch (Exception)
			{
				result = new Session[0];
			}
			return result;
		}
		private static List<Session> GetSessionsFromStreams(TCPStream tcpsClient, TCPStream tcpsServer)
		{
			StringBuilder stringBuilder = null;
			if (PacketCaptureImport.bVerboseDebug)
			{
				stringBuilder = new StringBuilder();
			}
			if (stringBuilder != null)
			{
				stringBuilder.AppendFormat("Connection {0}\nREQUEST\n", tcpsClient.sStreamID);
			}
			MemoryStream payloadStream = tcpsClient.GetPayloadStream(stringBuilder, 0);
			if (stringBuilder != null)
			{
				stringBuilder.AppendFormat("\nRESPONSE\n", new object[0]);
			}
			MemoryStream memoryStream;
			if (tcpsServer != null)
			{
				memoryStream = tcpsServer.GetPayloadStream(stringBuilder, 0);
			}
			else
			{
				memoryStream = new MemoryStream();
			}
			payloadStream.Position = 0L;
			memoryStream.Position = 0L;
			List<Session> list = new List<Session>();
			int num = 0;
			bool flag;
			do
			{
				string text = null;
				SessionTimers sessionTimers = new SessionTimers();
				sessionTimers.ClientConnected = tcpsClient.dtConnectStart;
				if (tcpsServer != null)
				{
					sessionTimers.ServerConnected = tcpsServer.dtConnectStart;
				}
				sessionTimers.ClientBeginRequest = tcpsClient.GetTimestampAtByte((int)payloadStream.Position);
				HTTPRequestHeaders hTTPRequestHeaders;
				byte[] array;
				flag = Parser.TakeRequest(payloadStream, out hTTPRequestHeaders, out array);
				sessionTimers.ClientDoneRequest = tcpsClient.GetTimestampAtByte((int)payloadStream.Position - 1);
				if (flag)
				{
					num++;
					HTTPResponseHeaders hTTPResponseHeaders = null;
					byte[] emptyByteArray = Utilities.emptyByteArray;
					bool flag2 = false;
					if (tcpsServer != null)
					{
						sessionTimers.ServerBeginResponse = tcpsServer.GetTimestampAtByte((int)memoryStream.Position);
						flag2 = Parser.TakeResponse(memoryStream, hTTPRequestHeaders.HTTPMethod, out hTTPResponseHeaders, out emptyByteArray);
					}
					if (flag2)
					{
						while (flag2 && hTTPResponseHeaders.HTTPResponseCode > 99 && hTTPResponseHeaders.HTTPResponseCode < 200)
						{
							text = text + "Eating a HTTP/" + hTTPResponseHeaders.HTTPResponseCode.ToString() + " message from the stream.";
							flag2 = Parser.TakeResponse(memoryStream, hTTPRequestHeaders.HTTPMethod, out hTTPResponseHeaders, out emptyByteArray);
						}
						sessionTimers.ServerDoneResponse = tcpsServer.GetTimestampAtByte((int)memoryStream.Position - 1);
					}
					else
					{
						hTTPResponseHeaders = Parser.ParseResponse("HTTP/1.1 0 FIDDLER GENERATED - RESPONSE DATA WAS MISSING\r\n\r\n");
					}
					Session session = new Session(hTTPRequestHeaders, array);
					session.oResponse.headers= hTTPResponseHeaders;
					session.responseBodyBytes = emptyByteArray;
					session.oFlags["X-EgressPort"] = (session.oFlags["X-ClientPort"] = tcpsClient.tcpEndpoints.iSrcPort.ToString());
					session.oFlags["X-ClientIP"] = tcpsClient.tcpEndpoints.addrSrc.ToString();
					session.oFlags["X-HostIP"] = tcpsClient.tcpEndpoints.addrDst.ToString();
					session.oFlags["X-HostPort"] = tcpsClient.tcpEndpoints.iDstPort.ToString();
					if (!string.IsNullOrEmpty(text))
					{
						session.oFlags["x-fiddler-streaming"] = text;
					}
					session.Timers = sessionTimers;
					if (num > 1)
					{
                        session.UNSTABLE_SetBitFlag((SessionFlags)24, true);
					}
					string sProcessInfo = tcpsClient.sProcessInfo;
					if (!string.IsNullOrEmpty(sProcessInfo))
					{
						session.oFlags["X-ProcessInfo"] = sProcessInfo;
					}
					if (stringBuilder != null)
					{
						session.oFlags["X-ConnectionDebugInfo"] = stringBuilder.ToString();
					}
					list.Add(session);
				}
			}
			while (flag);
			return list;
		}
		private static void _DumpDNSTraffic(Dictionary<uint, DNSTransaction> dictDNSQueries)
		{
			foreach (DNSTransaction current in dictDNSQueries.Values)
			{
				string text = "INCOMPLETE";
				int iMSTaken = current.iMSTaken;
				if (iMSTaken >= 0)
				{
					text = string.Format("Took {0}ms.", iMSTaken);
				}
                //FiddlerApplication.get_Log().LogFormat("{0}>\tDNS Query [0x{2:x}] for IPv{1} address of:\t'{3}'\t\t... {4}\n", new object[]
                //{
                //    current.dtQuerySent.ToString("MM/dd/yy HH:mm:ss:ffff"),
                //    current.bAAAAQuery ? 6 : 4,
                //    current.uiTransactionID,
                //    current.sQueryForHostname,
                //    text
                //});
			}
		}
	}
}
