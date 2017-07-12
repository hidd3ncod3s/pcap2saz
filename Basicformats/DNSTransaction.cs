using System;
namespace BasicFormats
{
	public class DNSTransaction
	{
		public string sQueryForHostname;
		public uint uiTransactionID;
		public bool bAAAAQuery;
		public DateTime dtQuerySent;
		public DateTime dtResponseReceived;
		public int iMSTaken
		{
			get
			{
				if (this.dtQuerySent.Ticks == 0L || this.dtResponseReceived.Ticks == 0L)
				{
					return -1;
				}
				return (int)(this.dtResponseReceived - this.dtQuerySent).TotalMilliseconds;
			}
		}
	}
}
