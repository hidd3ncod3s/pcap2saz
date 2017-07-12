using System;
using System.Text;
namespace BasicFormats
{
	internal class DNSQuery
	{
		public uint uiTransactionID;
		public uint uiFlags;
		public uint uiQuestionCount;
		public DNSQueryType QueryType;
		public string sHostname;
		public static DNSQuery Parse(IPFrame thisIPFrame, byte[] arrThisFrame)
		{
			DNSQuery dNSQuery = new DNSQuery();
			int iPayloadOffset = (int)thisIPFrame.iPayloadOffset;
			dNSQuery.uiTransactionID = (uint)((int)arrThisFrame[iPayloadOffset + 8] << 8 | (int)arrThisFrame[iPayloadOffset + 9]);
			dNSQuery.uiFlags = (uint)((int)arrThisFrame[iPayloadOffset + 10] << 8 | (int)arrThisFrame[iPayloadOffset + 11]);
			dNSQuery.uiQuestionCount = (uint)((int)arrThisFrame[iPayloadOffset + 12] << 8 | (int)arrThisFrame[iPayloadOffset + 13]);
			byte arg_51_0 = arrThisFrame[iPayloadOffset + 14];
			byte arg_58_0 = arrThisFrame[iPayloadOffset + 15];
			byte arg_5F_0 = arrThisFrame[iPayloadOffset + 16];
			byte arg_66_0 = arrThisFrame[iPayloadOffset + 17];
			byte arg_6D_0 = arrThisFrame[iPayloadOffset + 18];
			byte arg_74_0 = arrThisFrame[iPayloadOffset + 19];
			int num = iPayloadOffset + 20;
			string empty = string.Empty;
			while (arrThisFrame[num] != 0)
			{
				int num2 = (int)arrThisFrame[num];
				string @string = Encoding.UTF8.GetString(arrThisFrame, num + 1, num2);
				dNSQuery.sHostname = string.Format("{0}{1}{2}", empty, (empty.Length > 0) ? "." : string.Empty, @string);
				num += 1 + num2;
			}
			dNSQuery.QueryType = (DNSQueryType)((int)arrThisFrame[num + 1] << 8 | (int)arrThisFrame[num + 2]);
			return dNSQuery;
		}
	}
}
