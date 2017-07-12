using System;
namespace BasicFormats
{
	internal class DNSResponse
	{
		public uint uiTransactionID;
		public uint uiFlags;
		public uint uiAnswerCount;
		public static DNSResponse Parse(IPFrame thisIPFrame, byte[] arrThisFrame)
		{
			DNSResponse dNSResponse = new DNSResponse();
			int iPayloadOffset = (int)thisIPFrame.iPayloadOffset;
			dNSResponse.uiTransactionID = (uint)((int)arrThisFrame[iPayloadOffset + 8] << 8 | (int)arrThisFrame[iPayloadOffset + 9]);
			dNSResponse.uiFlags = (uint)((int)arrThisFrame[iPayloadOffset + 10] << 8 | (int)arrThisFrame[iPayloadOffset + 11]);
			dNSResponse.uiAnswerCount = (uint)((int)arrThisFrame[iPayloadOffset + 14] << 8 | (int)arrThisFrame[iPayloadOffset + 15]);
			return dNSResponse;
		}
	}
}
