using Fiddler;
using System;
using System.IO;
namespace BasicFormats
{
	public class TCPFrame
	{
		public uint _uiFrameID;
		public DateTime dtWhen;
		public ulong uiSeqNum;
		public uint uiAckNum;
		private byte[] arrPayload;
		public TCPFlags flagsTCP;
		public ushort SrcPort;
		public ushort DstPort;
		public ulong uiEndsAtSeqNum
		{
			get
			{
				return this.uiSeqNum + (ulong)this.PayloadLength - 1uL;
			}
		}
		public uint PayloadLength
		{
			get
			{
				if (this.arrPayload == null)
				{
					return 0u;
				}
				return (uint)this.arrPayload.Length;
			}
		}
		public TCPFrame(uint uiFrameID)
		{
			this._uiFrameID = uiFrameID;
		}
		public void WritePayloadToMemoryStream(MemoryStream oMS, uint iOffset)
		{
			oMS.Write(this.arrPayload, (int)iOffset, (int)((long)this.arrPayload.Length - (long)((ulong)iOffset)));
		}
		public override string ToString()
		{
			return string.Format("{0} {1}> {2}->{3}\n-- {4} bytes ----\n{5}\n", new object[]
			{
				this._uiFrameID,
				this.flagsTCP,
				this.SrcPort,
				this.DstPort,
				this.arrPayload.Length,
				Utilities.ByteArrayToHexView(this.arrPayload, 16)
			});
		}
		public static TCPFrame Parse(IPFrame thisIPFrame, byte[] arrThisFrame)
		{
			TCPFrame tCPFrame = new TCPFrame(thisIPFrame.iFrameNumber);
			int iPayloadOffset = (int)thisIPFrame.iPayloadOffset;
			tCPFrame.SrcPort = (ushort)((int)arrThisFrame[iPayloadOffset] << 8 | (int)arrThisFrame[iPayloadOffset + 1]);
			tCPFrame.DstPort = (ushort)((int)arrThisFrame[iPayloadOffset + 2] << 8 | (int)arrThisFrame[iPayloadOffset + 3]);
			int num = 4 * (arrThisFrame[iPayloadOffset + 12] >> 4);
			tCPFrame.dtWhen = thisIPFrame.dtWhen;
			tCPFrame.uiSeqNum = (ulong)((int)arrThisFrame[iPayloadOffset + 4] << 24 | (int)arrThisFrame[iPayloadOffset + 5] << 16 | (int)arrThisFrame[iPayloadOffset + 6] << 8 | (int)arrThisFrame[iPayloadOffset + 7]);
			tCPFrame.uiAckNum = (uint)((int)arrThisFrame[iPayloadOffset + 8] << 24 | (int)arrThisFrame[iPayloadOffset + 9] << 16 | (int)arrThisFrame[iPayloadOffset + 10] << 8 | (int)arrThisFrame[iPayloadOffset + 11]);
			tCPFrame.flagsTCP = (TCPFlags)arrThisFrame[iPayloadOffset + 13];
            //Console.WriteLine(String.Format("thisIPFrame.iPayloadLen= {0}, num= {1}", thisIPFrame.iPayloadLen, num));
			if ((int)thisIPFrame.iPayloadLen > num)
			{
				tCPFrame.arrPayload = new byte[(int)thisIPFrame.iPayloadLen - num];
				Buffer.BlockCopy(arrThisFrame, iPayloadOffset + num, tCPFrame.arrPayload, 0, tCPFrame.arrPayload.Length);
			}
			return tCPFrame;
		}
	}
}
