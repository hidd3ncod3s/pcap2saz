using System;
namespace BasicFormats
{
	public class UDPMessage
	{
		public WellKnownPorts SrcPort;
		public WellKnownPorts DstPort;
		public static UDPMessage Parse(IPFrame ipFrame, byte[] arrFrame)
		{
			return new UDPMessage
			{
				DstPort = (WellKnownPorts)((int)arrFrame[(int)(ipFrame.iPayloadOffset + 2)] << 8 | (int)arrFrame[(int)(ipFrame.iPayloadOffset + 3)]),
				SrcPort = (WellKnownPorts)((int)arrFrame[(int)ipFrame.iPayloadOffset] << 8 | (int)arrFrame[(int)(ipFrame.iPayloadOffset + 1)])
			};
		}
	}
}
