using System;
using System.IO;
namespace BasicFormats
{
	internal class PCAPFileHeader
	{
		public const uint MAGIC_NUMBER_MS = 2712847316u;
		public const uint MAGIC_NUMBER_MS_FLIPPED = 3569595041u;
		public const uint MAGIC_NUMBER_NS = 2712812621u;
		public const uint MAGIC_NUMBER_NS_FLIPPED = 1295823521u;
		public uint MagicNumber;
		public ushort VerMajor;
		public ushort VerMinor;
		public int TimeZoneCorrection;
		public uint TimestampAccuracy;
		public uint MaxPacketLength;
		public uint LinkType;
		public bool IsByteOrderSwapped
		{
			get
			{
				return this.MagicNumber == 3569595041u || this.MagicNumber == 1295823521u;
			}
		}
		public bool IsNanoSecondFormat
		{
			get
			{
				return this.MagicNumber == 2712812621u || this.MagicNumber == 1295823521u;
			}
		}
		public override string ToString()
		{
			return string.Format("Magic:\t0x{0:x}\nVersion:\t{1}.{2}\nAccuracy:\t{3}\nns Format:\t{4}\nMaxSize:\t{5}\nLinkType:\t{6}\n", new object[]
			{
				this.MagicNumber,
				this.VerMajor,
				this.VerMinor,
				this.TimestampAccuracy,
				this.IsNanoSecondFormat ? "yes" : "no",
				this.MaxPacketLength,
				this.LinkType
			});
		}
		public static PCAPFileHeader CreateFromReader(BinaryReader rdrFrom, uint uiMagic)
		{
			return new PCAPFileHeader
			{
				MagicNumber = uiMagic,
				VerMajor = rdrFrom.ReadUInt16(),
				VerMinor = rdrFrom.ReadUInt16(),
				TimeZoneCorrection = rdrFrom.ReadInt32(),
				TimestampAccuracy = rdrFrom.ReadUInt32(),
				MaxPacketLength = rdrFrom.ReadUInt32(),
				LinkType = rdrFrom.ReadUInt32()
			};
		}
	}
}
