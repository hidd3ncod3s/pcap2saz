using System;
namespace BasicFormats
{
	[Flags]
	public enum FrameTypes
	{
		None = 0,
		IPv4 = 1,
		IPv6 = 2,
		TCPIP = 4,
		UDP = 8,
		DNSQuery = 16,
		DNSResponse = 32
	}
}
