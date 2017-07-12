using Fiddler;
using System;
using System.Net;
namespace BasicFormats
{
	public class IPFrame
	{
		public uint iFrameNumber;
		public byte IPVersion;
		public IPAddress ipSrc;
		public IPAddress ipDest;
		public IPSubProtocols NextProtocol;
		public ushort iPayloadOffset;
		public ushort iPayloadLen;
		public bool bIsFragment;
		public DateTime dtWhen;
		public static IPFrame FakeAsIPFrame(uint iFrameNum, byte[] arrThisFrame, DateTime dtWhen)
		{
                IPFrame iPFrame = new IPFrame();
                iPFrame.iFrameNumber = iFrameNum;
                iPFrame.dtWhen = dtWhen;
                iPFrame.IPVersion = 4;
                iPFrame.NextProtocol = (IPSubProtocols)arrThisFrame[8];
                long newAddress = (long)((int)arrThisFrame[0] << 24 | (int)arrThisFrame[1] << 16 | (int)arrThisFrame[2] << 8 | (int)arrThisFrame[3]) & (long)((ulong)0xFFFFFFFF);
                iPFrame.ipSrc = new IPAddress(newAddress);
                long newAddress2 = (long)((int)arrThisFrame[4] << 24 | (int)arrThisFrame[5] << 16 | (int)arrThisFrame[6] << 8 | (int)arrThisFrame[7]) & (long)((ulong)0xFFFFFFFF);
                iPFrame.ipDest = new IPAddress(newAddress2);
                iPFrame.iPayloadOffset = 19;
                iPFrame.iPayloadLen = (ushort)((int)arrThisFrame[18] << 8 | (int)arrThisFrame[17]);
                return iPFrame;
		}
		public static IPFrame ParseAsIPFrame(uint iFrameNum, byte[] arrThisFrame, DateTime dtWhen)
		{
                IPFrame iPFrame = new IPFrame();
                iPFrame.iFrameNumber = iFrameNum;
                iPFrame.dtWhen = dtWhen;
                if (arrThisFrame[12] == 8 && arrThisFrame[13] == 0)
                {
                        iPFrame.IPVersion = 4;
                        iPFrame.NextProtocol = (IPSubProtocols)arrThisFrame[23];
                        long newAddress = (long)((int)arrThisFrame[29] << 24 | (int)arrThisFrame[28] << 16 | (int)arrThisFrame[27] << 8 | (int)arrThisFrame[26]) & (long)((ulong)0xFFFFFFFF);
                        iPFrame.ipSrc = new IPAddress(newAddress);
                        long newAddress2 = (long)((int)arrThisFrame[33] << 24 | (int)arrThisFrame[32] << 16 | (int)arrThisFrame[31] << 8 | (int)arrThisFrame[30]) & (long)((ulong)0xFFFFFFFF);
                        iPFrame.ipDest = new IPAddress(newAddress2);
                        ushort num = (ushort)(4 * (arrThisFrame[14] & 15));
                        iPFrame.iPayloadOffset = (ushort)(14 + num);
                        ushort num2 = (ushort)((int)arrThisFrame[16] << 8 | (int)arrThisFrame[17]);
                        if (num2 == 0)
                        {
                            num2 = (ushort)(arrThisFrame.Length - 14);
                        }
                        if (num2 < num)
                        {
                            Console.WriteLine(String.Format("! Warning: Frame {0} contained malformed IP data. Total_Length ({1:N0} bytes) < Header_Length ({2:N0} bytes)", iPFrame.iFrameNumber,num2,num));
                        }
                        else
                        {
                           iPFrame.iPayloadLen = (ushort)(num2 - num);
                        }
                        byte b = (byte)(arrThisFrame[20] >> 5);
                        uint num3 = (uint)(8 * (((int)arrThisFrame[20] << 8 | (int)arrThisFrame[21]) & 8191));
                        if (1 == (b & 1) || num3 != 0u)
                        {
                            //    FiddlerApplication.get_Log().LogFormat("! Warning: Frame {0} fragmented. Fragment reassembly not yet implemented", new object[]
                            //{
                            //    iPFrame.iFrameNumber
                            //});
                            iPFrame.bIsFragment = true;
                        }
                        return iPFrame;
                }
			if (arrThisFrame[12] == 134 && arrThisFrame[13] == 221)
			{
                Console.WriteLine(String.Format("v6"));
				iPFrame.IPVersion = 6;
				iPFrame.NextProtocol = (IPSubProtocols)arrThisFrame[20];
				byte[] array = new byte[16];
				Buffer.BlockCopy(arrThisFrame, 22, array, 0, 16);
				iPFrame.ipSrc = new IPAddress(array);
				byte[] array2 = new byte[16];
				Buffer.BlockCopy(arrThisFrame, 38, array2, 0, 16);
				iPFrame.ipDest = new IPAddress(array2);
				iPFrame.iPayloadOffset = 54;
				iPFrame.iPayloadLen = (ushort)((int)arrThisFrame[18] << 8 | (int)arrThisFrame[19]);
				return iPFrame;
			}
			return null;
		}
		public override string ToString()
		{
			return string.Format("#{0} - IPv{1}/{2} - {3}->{4} ({5} bytes)", new object[]
			{
				this.iFrameNumber,
				this.IPVersion,
				this.NextProtocol,
				this.ipSrc,
				this.ipDest,
				this.iPayloadLen
			});
		}
	}
}
