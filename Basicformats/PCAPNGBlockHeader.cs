using System;
using System.IO;
namespace BasicFormats
{
	internal class PCAPNGBlockHeader
	{
		public PCAPNGBlockType uiBlockType;
		public uint uiBlockLength;
		public static PCAPNGBlockHeader CreateFromReader(BinaryReader rdrFrom)
		{
			return new PCAPNGBlockHeader
			{
				uiBlockType = (PCAPNGBlockType)rdrFrom.ReadUInt32(),
				uiBlockLength = rdrFrom.ReadUInt32()
			};
		}
		public override string ToString()
		{
			return string.Format("BlockType:\t{0} ({0:x})\nBlockLength: \t{1} bytes\n", this.uiBlockType, this.uiBlockLength);
		}
	}
}
