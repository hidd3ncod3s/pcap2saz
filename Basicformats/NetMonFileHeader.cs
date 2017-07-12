using Fiddler;
using System;
using System.IO;
using System.Text;
namespace BasicFormats
{
	internal class NetMonFileHeader
	{
		public const uint MAGIC_NUMBER_V2 = 1430408519u;
		public const uint MAGIC_NUMBER_V1 = 1397970002u;
		public uint MagicNumber;
		public byte VerMajor;
		public byte VerMinor;
		public DateTime dtCapture;
		public ushort MacType;
		private uint FrameTableOffset;
		private uint FrameTableLength;
		private uint UserDataOffset;
		private uint UserDataLength;
		private uint CommentDataOffset;
		private uint CommentDataLength;
		private uint StatisticsOffset;
		private uint StatisticsLength;
		private uint ProcessListOffset;
		private uint ProcessListCount;
		private uint ExtendedInfoOffset;
		private uint ExtendedInfoLength;
		private uint ConversationStatsOffset;
		private uint ConversationStatsLength;
		public string[] arrProcesses;
		public uint uiFrameCount
		{
			get
			{
				return this.FrameTableLength / 4u;
			}
		}
		public override string ToString()
		{
			return string.Format("Magic:\t0x{0:x}\nVersion:\t{1}.{2}\nFrom:\t{3}\nMacType:\t{4}\n#Frames:\t{5}\nsizeof(comment):\t{6}\nCount(ProcInfo):\t{7}", new object[]
			{
				this.MagicNumber,
				this.VerMajor,
				this.VerMinor,
				this.dtCapture.ToString("M/d/yyyy h:mm:ss.ffff"),
				this.MacType,
				this.uiFrameCount,
				this.CommentDataLength,
				this.ProcessListCount
			});
		}
		private void FillProcessList(BinaryReader oBR)
		{
			oBR.BaseStream.Position = (long)((ulong)this.ProcessListOffset);
			oBR.ReadUInt16();
			this.arrProcesses = new string[this.ProcessListCount];
			int num = 0;
			while ((long)num < (long)((ulong)this.ProcessListCount))
			{
				uint count = oBR.ReadUInt32();
				byte[] bytes = oBR.ReadBytes((int)count);
				uint num2 = oBR.ReadUInt32();
				oBR.BaseStream.Position += (long)((ulong)num2);
				uint num3 = oBR.ReadUInt32();
				string arg_7B_0 = Encoding.Unicode.GetString(bytes);
				char[] trimChars = new char[1];
				string text = arg_7B_0.TrimEnd(trimChars).ToLower();
				text = Utilities.TrimBeforeLast(text, '\\');
				int num4 = text.LastIndexOf('.');
				if (num4 > 0)
				{
					text = text.Substring(0, num4);
				}
				this.arrProcesses[num] = string.Format("{0}:{1}", text, num3);
				oBR.BaseStream.Position += 8L;
				oBR.ReadUInt32();
				oBR.BaseStream.Position += 32L;
				num++;
			}
		}
		public static NetMonFileHeader CreateFromReader(BinaryReader rdrFrom, uint uiMagic)
		{
			NetMonFileHeader netMonFileHeader = new NetMonFileHeader();
			netMonFileHeader.MagicNumber = uiMagic;
			netMonFileHeader.VerMinor = rdrFrom.ReadByte();
			netMonFileHeader.VerMajor = rdrFrom.ReadByte();
			netMonFileHeader.MacType = rdrFrom.ReadUInt16();
			ushort year = rdrFrom.ReadUInt16();
			ushort month = rdrFrom.ReadUInt16();
			rdrFrom.ReadUInt16();
			ushort day = rdrFrom.ReadUInt16();
			ushort hour = rdrFrom.ReadUInt16();
			ushort minute = rdrFrom.ReadUInt16();
			ushort second = rdrFrom.ReadUInt16();
			ushort millisecond = rdrFrom.ReadUInt16();
			netMonFileHeader.dtCapture = new DateTime((int)year, (int)month, (int)day, (int)hour, (int)minute, (int)second, (int)millisecond);
			netMonFileHeader.FrameTableOffset = rdrFrom.ReadUInt32();
			netMonFileHeader.FrameTableLength = rdrFrom.ReadUInt32();
			netMonFileHeader.UserDataOffset = rdrFrom.ReadUInt32();
			netMonFileHeader.UserDataLength = rdrFrom.ReadUInt32();
			netMonFileHeader.CommentDataOffset = rdrFrom.ReadUInt32();
			netMonFileHeader.CommentDataLength = rdrFrom.ReadUInt32();
			netMonFileHeader.ProcessListOffset = rdrFrom.ReadUInt32();
			netMonFileHeader.ProcessListCount = rdrFrom.ReadUInt32();
			netMonFileHeader.StatisticsOffset = rdrFrom.ReadUInt32();
			netMonFileHeader.StatisticsLength = rdrFrom.ReadUInt32();
			netMonFileHeader.ExtendedInfoOffset = rdrFrom.ReadUInt32();
			netMonFileHeader.ExtendedInfoLength = rdrFrom.ReadUInt32();
			netMonFileHeader.ConversationStatsOffset = rdrFrom.ReadUInt32();
			netMonFileHeader.ConversationStatsLength = rdrFrom.ReadUInt32();
			if (netMonFileHeader.VerMajor == 2 && netMonFileHeader.VerMinor > 1)
			{
				netMonFileHeader.FillProcessList(rdrFrom);
			}
            // [hidd3ncod3s]I fixed it.
            //FiddlerApplication.get_Log().LogFormat("Importing NetMon Capture\n{0}\n", new object[]
            //{
            //    netMonFileHeader
            //});

            Console.WriteLine(String.Format("Importing NetMon Capture\n{0}\n",netMonFileHeader));
			return netMonFileHeader;
		}
		internal uint[] GetFrameOffsets(BinaryReader rdr)
		{
			uint[] array = new uint[this.uiFrameCount];
			rdr.BaseStream.Position = (long)((ulong)this.FrameTableOffset);
			uint num = 0u;
			while ((ulong)num < (ulong)((long)array.Length))
			{
				array[(int)((UIntPtr)num)] = rdr.ReadUInt32();
				num += 1u;
			}
			return array;
		}
	}
}
