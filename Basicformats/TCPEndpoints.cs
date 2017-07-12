using System;
using System.Net;
namespace BasicFormats
{
	public class TCPEndpoints
	{
		public IPAddress addrSrc;
		public IPAddress addrDst;
		public ushort iSrcPort;
		public ushort iDstPort;
		public TCPEndpoints(IPAddress _src, IPAddress _dst, ushort _srcPort, ushort _dstPort)
		{
			this.addrSrc = _src;
			this.addrDst = _dst;
			this.iSrcPort = _srcPort;
			this.iDstPort = _dstPort;
		}
		public override string ToString()
		{
			return string.Format("{0}:{1}->{2}:{3}", new object[]
			{
				this.addrSrc,
				this.iSrcPort,
				this.addrDst,
				this.iDstPort
			});
		}
		public string ComputePeerId()
		{
			return string.Format("{0}:{1}->{2}:{3}", new object[]
			{
				this.addrDst,
				this.iDstPort,
				this.addrSrc,
				this.iSrcPort
			});
		}
	}
}
