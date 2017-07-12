using System;
using System.IO;
namespace BasicFormats
{
	internal class NetmonPacketHeader
	{
		private long microsecondsOffset;
		public uint PacketSavedSize;
		public uint PacketOriginalSize;
		public MediaTypes MediaType;
		public uint ProcessTableIndex;
		private DateTime dtCaptureStarted;
		public DateTime dtPacket
		{
			get
			{
				return this.dtCaptureStarted.AddMilliseconds((double)this.microsecondsOffset / 1000.0);
			}
		}
		public static NetmonPacketHeader CreateFromReader(BinaryReader rdrFrom, DateTime dtStarted)
		{
			NetmonPacketHeader netmonPacketHeader = new NetmonPacketHeader();
			netmonPacketHeader.dtCaptureStarted = dtStarted;
			netmonPacketHeader.microsecondsOffset = rdrFrom.ReadInt64();
			netmonPacketHeader.PacketSavedSize = rdrFrom.ReadUInt32();
			netmonPacketHeader.PacketOriginalSize = rdrFrom.ReadUInt32();
			rdrFrom.BaseStream.Position += (long)((ulong)netmonPacketHeader.PacketSavedSize);
			netmonPacketHeader.MediaType = (MediaTypes)rdrFrom.ReadUInt16();
			netmonPacketHeader.ProcessTableIndex = rdrFrom.ReadUInt32();
			return netmonPacketHeader;
		}
		public override string ToString()
		{
			return string.Format("MediaType:\t{4}\nTimeStamp:\t{0}\nDateTime: \t{1}\nCapturedSize:\t{2}/{3}\n", new object[]
			{
				this.microsecondsOffset,
				this.dtPacket.ToString("M/d/yy HH:mm:ss:ffff"),
				this.PacketSavedSize,
				this.PacketOriginalSize,
				this.MediaType
			});
		}
	}
}
