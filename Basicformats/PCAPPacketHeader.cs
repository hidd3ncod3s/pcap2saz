using System;
using System.IO;
namespace BasicFormats
{
	internal class PCAPPacketHeader
	{
		public uint TimestampSec;
		public uint TimestampMicrosec;
		public uint PacketSavedSize;
		public uint PacketOriginalSize;
		public DateTime dtPacket
		{
			get
			{
				DateTime result = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
				result = result.AddSeconds(this.TimestampSec);
				result = result.AddMilliseconds(this.TimestampMicrosec / 1000.0);
				return result;
			}
		}
		public static PCAPPacketHeader CreateFromReader(BinaryReader rdrFrom)
		{
			return new PCAPPacketHeader
			{
				TimestampSec = rdrFrom.ReadUInt32(),
				TimestampMicrosec = rdrFrom.ReadUInt32(),
				PacketSavedSize = rdrFrom.ReadUInt32(),
				PacketOriginalSize = rdrFrom.ReadUInt32()
			};
		}
		public override string ToString()
		{
			return string.Format("TimeStamp:\t{0}.{1}\nDateTime: \t{4}\nCapturedSize:\t{2}\nOriginalSize:\t{3}\n", new object[]
			{
				this.TimestampSec,
				this.TimestampMicrosec,
				this.PacketSavedSize,
				this.PacketOriginalSize,
				this.dtPacket.ToString("M/d/yy HH:mm:ss:ffff")
			});
		}
	}
}
