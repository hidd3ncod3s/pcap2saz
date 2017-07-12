using System;
using System.IO;
namespace BasicFormats
{
	internal class PCAPNGFileHeader
	{
		public const uint MAGIC_NUMBER = 168627466u;
		public uint MagicNumber;
		public uint SectionHeaderLength;
		public uint RepeatedHeaderLength;
		public uint ByteOrderMagic;
		public ushort VerMajor;
		public ushort VerMinor;
		public long longSectionLength;
		public override string ToString()
		{
			return string.Format("Magic:\t\t0x{0:x}\nFormat:\tv{1}.{2}\n\n", this.MagicNumber, this.VerMajor, this.VerMinor);
		}
		public static PCAPNGFileHeader CreateFromReader(BinaryReader rdrFrom, uint uiMagic)
		{
			PCAPNGFileHeader pCAPNGFileHeader = new PCAPNGFileHeader();
			pCAPNGFileHeader.MagicNumber = uiMagic;
			pCAPNGFileHeader.SectionHeaderLength = rdrFrom.ReadUInt32();
			pCAPNGFileHeader.ByteOrderMagic = rdrFrom.ReadUInt32();
			pCAPNGFileHeader.VerMajor = rdrFrom.ReadUInt16();
			pCAPNGFileHeader.VerMinor = rdrFrom.ReadUInt16();
			pCAPNGFileHeader.longSectionLength = rdrFrom.ReadInt64();
			uint count = pCAPNGFileHeader.SectionHeaderLength - 28u;
			rdrFrom.ReadBytes((int)count);
			pCAPNGFileHeader.RepeatedHeaderLength = rdrFrom.ReadUInt32();
			return pCAPNGFileHeader;
		}
	}
}
