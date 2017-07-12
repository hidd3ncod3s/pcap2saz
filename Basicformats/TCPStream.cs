using Fiddler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace BasicFormats
{
	public class TCPStream
	{
		private struct PayloadPacketTime
		{
			public int iOffset;
			public DateTime dtTime;
		}
		public TCPEndpoints tcpEndpoints;
		public TCPStream tcpsPeer;
		public TCPStreamContent ctContentType;
		public string sProcessInfo;
		private List<TCPFrame> listFrames = new List<TCPFrame>();
		private ulong InitialSequenceNumber;
		private bool _seenFIN;
		private bool _seenSYN;
		private bool _seenSYNACK;
		public DateTime dtFirstPayload;
		public DateTime dtConnectStart;
		private List<TCPStream.PayloadPacketTime> listPayloadPacketTimes;
		internal bool IsServer
		{
			get
			{
				return this._seenSYNACK || (!this._seenSYN && (this.ctContentType & TCPStreamContent.Server) == TCPStreamContent.Server);
			}
		}
		public bool IsComplete
		{
			get
			{
				if (!this._seenFIN)
				{
					return false;
				}
				if (!this._seenSYN && !this._seenSYNACK)
				{
					return false;
				}
				ulong num = this.InitialSequenceNumber;
				foreach (TCPFrame current in this.listFrames)
				{
					if (current.uiSeqNum > num)
					{
						if (PacketCaptureImport.bDebug)
						{
                            //FiddlerApplication.get_Log().LogFormat("!Missing traffic from: {0}\n", new object[]
                            //{
                            //    this.sStreamID
                            //});
                            //FiddlerApplication.get_Log().LogFormat("Missing data from offset 0x{0:x} to 0x{1:x}\n{2}\n", new object[]
                            //{
                            //    num,
                            //    current.uiSeqNum,
                            //    this.ListMessagesBySequence()
                            //});
						}
						return false;
					}
					uint num2 = current.PayloadLength;
					if ((current.flagsTCP & (TCPFlags.FIN | TCPFlags.SYN)) > TCPFlags.None)
					{
						num2 += 1u;
					}
					if (num2 != 0u)
					{
						num = current.uiSeqNum + (ulong)(num2 % 4294967295u);
					}
				}
				return true;
			}
		}
		public string sStreamID
		{
			get
			{
				return this.tcpEndpoints.ToString();
			}
		}
		public int FrameCount
		{
			get
			{
				return this.listFrames.Count;
			}
		}
		public bool HasFIN
		{
			get
			{
				return this._seenFIN;
			}
		}
		public bool HasSYN
		{
			get
			{
				return this._seenSYN;
			}
		}
		public bool HasSYNACK
		{
			get
			{
				return this._seenSYNACK;
			}
		}
		public TCPStream(TCPEndpoints _oEP)
		{
			this.tcpEndpoints = _oEP;
		}
		public DateTime GetTimestampAtByte(int iByteOffset)
		{
			DateTime dtTime = new DateTime(0L);
			foreach (TCPStream.PayloadPacketTime current in this.listPayloadPacketTimes)
			{
				if (iByteOffset < current.iOffset)
				{
					break;
				}
				dtTime = current.dtTime;
			}
			return dtTime;
		}
		public string ListMessagesBySequence()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (TCPFrame current in this.listFrames)
			{
				stringBuilder.AppendFormat("0x{0:x},", current.uiSeqNum);
			}
			return stringBuilder.ToString();
		}
		public void SortMessages()
		{
			if (this.listFrames.Count < 2)
			{
				return;
			}
			this.listFrames.Sort(delegate(TCPFrame a, TCPFrame b)
			{
				int num = a.uiSeqNum.CompareTo(b.uiSeqNum);
				if (num != 0)
				{
					return num;
				}
				return -a.PayloadLength.CompareTo(b.PayloadLength);
			});
			ulong uiSeqNum = this.listFrames[this.listFrames.Count - 1].uiSeqNum;
            if (uiSeqNum > (ulong)0xFFFEFFEB) //-65557)
                {
                    ulong uiSeqNum2 = this.listFrames[0].uiSeqNum;
                    if (uiSeqNum - uiSeqNum2 > 2147483647uL)
                    {
                        this.listFrames.ForEach(delegate(TCPFrame aFrame)
                        {
                            if (aFrame.uiSeqNum < 2147483647uL)
                            {
                                unchecked
                                {
                                    if (PacketCaptureImport.bVerboseDebug)
                                    {
                                        Console.WriteLine(String.Format("Changing packet Sequence Number from {0} to {1}", aFrame.uiSeqNum, aFrame.uiSeqNum + (ulong)0xFFFFFFFF));
                                    }
                                    aFrame.uiSeqNum += (ulong)0xFFFFFFFF;
                                }
                            }
                        });
                        this.listFrames.Sort(delegate(TCPFrame a, TCPFrame b)
                        {
                            int num = a.uiSeqNum.CompareTo(b.uiSeqNum);
                            if (num != 0)
                            {
                                return num;
                            }
                            return -a.PayloadLength.CompareTo(b.PayloadLength);
                        });
                    }
                }
		}
		internal MemoryStream GetPayloadStream(StringBuilder sbDebugInfo, int iMaxSize)
		{
			MemoryStream memoryStream = new MemoryStream();
			this.listPayloadPacketTimes = new List<TCPStream.PayloadPacketTime>();
			try
			{
				if (sbDebugInfo != null)
				{
					foreach (TCPFrame current in this.listFrames)
					{
						sbDebugInfo.AppendFormat("Packet #{0}:\tSeq #{1}\tPayloadLength: {2}\n", current._uiFrameID, current.uiSeqNum, current.PayloadLength);
					}
				}
				ulong num = 0uL;
				foreach (TCPFrame current2 in this.listFrames)
				{
					if (current2.PayloadLength >= 1u)
					{
						if (current2.uiEndsAtSeqNum < num)
						{
							if (PacketCaptureImport.bVerboseDebug)
							{
                                //FiddlerApplication.get_Log().LogFormat(string.Concat(new string[]
                                //{
                                //    "Skipping Packet\n\nNext\t",
                                //    num.ToString("N0"),
                                //    "\n\nStart\t",
                                //    current2.uiSeqNum.ToString("N0"),
                                //    "\nStop\t",
                                //    current2.uiEndsAtSeqNum.ToString("N0")
                                //}), new object[0]);
							}
						}
						else
						{
							if (memoryStream.Length == 0L)
							{
								this.dtFirstPayload = current2.dtWhen;
							}
							this.listPayloadPacketTimes.Add(new TCPStream.PayloadPacketTime
							{
								dtTime = current2.dtWhen,
								iOffset = (int)memoryStream.Length
							});
							uint num2 = 0u;
							if (num > 0uL)
							{
								if (num < current2.uiSeqNum)
								{
									uint num3 = (uint)(current2.uiSeqNum - num);
									if (PacketCaptureImport.bVerboseDebug)
									{
                                        //FiddlerApplication.get_Log().LogFormat("! Missing {0} bytes of data between {1:N0} and {2:N0} in Stream\n{3}", new object[]
                                        //{
                                        //    num3,
                                        //    num,
                                        //    current2.uiSeqNum,
                                        //    this.ToString()
                                        //});
									}
									memoryStream.Seek((long)((ulong)num3), SeekOrigin.Current);
									num = current2.uiSeqNum;
								}
								try
								{
									num2 = checked((uint)(num - current2.uiSeqNum));
								}
								catch (ArithmeticException)
								{
                                    //FiddlerApplication.get_Log().LogFormat(string.Concat(new string[]
                                    //{
                                    //    "Threw on Frame #",
                                    //    current2._uiFrameID.ToString("N0"),
                                    //    "\n\nNext\t",
                                    //    num.ToString("N0"),
                                    //    "\n\nStart\t",
                                    //    current2.uiSeqNum.ToString("N0"),
                                    //    "\nStop\t",
                                    //    current2.uiEndsAtSeqNum.ToString("N0")
                                    //}), new object[0]);
								}
							}
							if (sbDebugInfo != null)
							{
								sbDebugInfo.AppendFormat("{5}Packet #{0,-5} has {1,-5} of {2,-5} usable bytes\tFilling {3:N0}-{4:N0}\n", new object[]
								{
									current2._uiFrameID,
									current2.PayloadLength - num2,
									current2.PayloadLength,
									memoryStream.Length,
									(ulong)(memoryStream.Length + (long)((ulong)(current2.PayloadLength - num2))),
									(num2 == 0u) ? string.Empty : "!! PARTIAL\n"
								});
							}
							current2.WritePayloadToMemoryStream(memoryStream, num2);
							num = current2.uiEndsAtSeqNum + 1uL;
							if (iMaxSize > 0 && memoryStream.Length > (long)iMaxSize)
							{
								break;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				FiddlerApplication.ReportException(ex, "GetPayloadStream() Error", "Failure when constructing payload stream for " + this.ToString());
			}
			return memoryStream;
		}
		public void SniffStream()
		{
			byte[] array = this.GetPayloadStream(null, 1024).ToArray();
			if (Utilities.IsNullOrEmpty(array))
			{
				this.ctContentType = TCPStreamContent.NoPayload;
				return;
			}
			if (array.Length > 2 && 22 == array[0] && 3 == array[1])
			{
				this.ctContentType = TCPStreamContent.SSLTLS;
				return;
			}
			if (array[0] < 65 || array[0] > 122 || (array[0] > 90 && array[0] < 97))
			{
				this.ctContentType = TCPStreamContent.Unknown;
				return;
			}
			if (Utilities.HasMagicBytes(array, "HTTP/"))
			{
				this.ctContentType = (TCPStreamContent.Server | TCPStreamContent.HTTP);
				return;
			}
			this.ctContentType = (TCPStreamContent.Client | TCPStreamContent.HTTP);
		}
		public string ToString(bool bShowPeer)
		{
			return string.Format("{0}\tFrameCount: {1}\t{2}{3}{4}{5} {6}\n{7}", new object[]
			{
				this.sStreamID,
				this.FrameCount,
				this.HasSYN ? " SYN (Client)" : string.Empty,
				this.HasSYNACK ? " SYNACK (Server)" : string.Empty,
				this.HasFIN ? " FIN" : string.Empty,
				this.IsComplete ? " COMPLETE" : " INCOMPLETE",
				this.ctContentType,
				(bShowPeer && this.tcpsPeer != null) ? this.tcpsPeer.ToString(false) : (bShowPeer ? "** UNPEERED **" : string.Empty)
			});
		}
		public override string ToString()
		{
			return this.ToString(true);
		}
		public void AddFrame(TCPFrame tcpFrame)
		{
			if (TCPFlags.FIN == (tcpFrame.flagsTCP & TCPFlags.FIN))
			{
				this._seenFIN = true;
			}
			if (TCPFlags.SYNACK == (tcpFrame.flagsTCP & TCPFlags.SYNACK))
			{
				this._seenSYNACK = true;
				this.InitialSequenceNumber = tcpFrame.uiSeqNum;
				this.dtConnectStart = tcpFrame.dtWhen;
			}
			else
			{
				if (TCPFlags.SYN == (tcpFrame.flagsTCP & TCPFlags.SYN))
				{
					this._seenSYN = true;
					this.InitialSequenceNumber = tcpFrame.uiSeqNum;
					this.dtConnectStart = tcpFrame.dtWhen;
				}
			}
			this.listFrames.Add(tcpFrame);
		}
	}
}
