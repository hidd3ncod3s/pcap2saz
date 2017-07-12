using System;
using System.IO;
namespace BasicFormats
{
	internal class PCAPNGEnhancedPacketHeader
	{
		public uint InterfaceID;
		public ulong Timestamp;
		public uint PacketSavedSize;
		public uint PacketOriginalSize;
		public DateTime dtPacket
		{
			get
			{
				DateTime result = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
				result = result.AddMilliseconds(this.Timestamp / 1000.0);
				return result;
			}
		}
		public static PCAPNGEnhancedPacketHeader CreateFromReader(BinaryReader rdrFrom)
		{
			PCAPNGEnhancedPacketHeader pCAPNGEnhancedPacketHeader = new PCAPNGEnhancedPacketHeader();
			pCAPNGEnhancedPacketHeader.InterfaceID = rdrFrom.ReadUInt32();
			pCAPNGEnhancedPacketHeader.Timestamp = (ulong)rdrFrom.ReadUInt32();
			pCAPNGEnhancedPacketHeader.Timestamp = (pCAPNGEnhancedPacketHeader.Timestamp << 32) + (ulong)rdrFrom.ReadUInt32();
			pCAPNGEnhancedPacketHeader.PacketSavedSize = rdrFrom.ReadUInt32();
			pCAPNGEnhancedPacketHeader.PacketOriginalSize = rdrFrom.ReadUInt32();
			return pCAPNGEnhancedPacketHeader;
		}
		public override string ToString()
		{
			return string.Format("TimeStamp:\t{0}\nDateTime: \t{1}\nCapturedSize:\t{2}\nOriginalSize:\t{3}\n", new object[]
			{
				this.Timestamp,
				this.dtPacket.ToString("M/d/yyyy HH:mm:ss:ffff"),
				this.PacketSavedSize,
				this.PacketOriginalSize
			});
		}
	}
}
