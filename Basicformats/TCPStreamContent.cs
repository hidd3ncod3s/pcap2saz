using System;
namespace BasicFormats
{
	[Flags]
	public enum TCPStreamContent
	{
		Unsniffed = 0,
		Client = 1,
		Server = 2,
		NoPayload = 4,
		Unknown = 8,
		HTTP = 16,
		WebSocket = 32,
		SSLTLS = 64
	}
}
