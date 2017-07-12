using System;
namespace BasicFormats
{
	internal enum PCAPNGBlockType : uint
	{
		InterfaceDescriptionBlock = 1u,
		PacketBlock,
		SimplePacketBlock,
		NameResolutionBlock,
		InterfaceStatisticsBlock,
		EnhancedPacketBlock,
		IRIGTimestampBlock,
		ARINC429Block,
		SectionHeaderBlock = 168627466u
	}
}
