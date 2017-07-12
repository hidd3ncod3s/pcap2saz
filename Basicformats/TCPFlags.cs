using System;
namespace BasicFormats
{
	[Flags]
	public enum TCPFlags
	{
		None = 0,
		FIN = 1,
		SYN = 2,
		SYNACK = 18,
		RST = 4,
		PSH = 8,
		ACK = 16,
		URG = 32
	}
}
