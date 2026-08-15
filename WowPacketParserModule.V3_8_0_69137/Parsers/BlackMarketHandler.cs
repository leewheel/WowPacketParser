using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V3_8_0_69137.Parsers
{
    public static class BlackMarketHandler
    {
        public static void ReadBlackMarketItem(Packet packet, params object[] idx)
        {
            packet.ReadInt32("MarketID", idx);
            packet.ReadInt32<UnitId>("SellerNPC", idx);
            packet.ReadInt32("Quantity", idx);
            packet.ReadUInt64("MinBid", idx);
            packet.ReadUInt64("MinIncrement", idx);
            packet.ReadUInt64("CurrentBid", idx);
            packet.ReadInt32("SecondsRemaining", idx);
            packet.ReadInt32("NumBids", idx);
            Substructures.ItemHandler.ReadItemInstance(packet, "Item", idx);
            packet.ResetBitReader();
            packet.ReadBit("HighBid", idx);
        }

        [Parser(Opcode.SMSG_BLACK_MARKET_REQUEST_ITEMS_RESULT)]
        public static void HandleBlackMarketRequestItemsResult(Packet packet)
        {
            // 国服 3.80.2：包结构与 V5_5_3 可能不同
            var remaining = packet.Length - packet.Position;
            if (remaining > 0)
                packet.ReadBytes("UnkData", (int)remaining);
        }

        [Parser(Opcode.SMSG_BLACK_MARKET_BID_ON_ITEM_RESULT)]
        public static void HandleBlackMarketBidOnItemResult(Packet packet)
        {
            packet.ReadInt32("MarketID");
            packet.ReadInt32("Result");
            Substructures.ItemHandler.ReadItemInstance(packet, "Item");
        }

        [Parser(Opcode.SMSG_BLACK_MARKET_OUTBID)]
        [Parser(Opcode.SMSG_BLACK_MARKET_WON)]
        public static void HandleBlackMarketOutbidOrWon(Packet packet)
        {
            packet.ReadInt32("MarketID");
            packet.ReadInt32("RandomPropertiesID");
            Substructures.ItemHandler.ReadItemInstance(packet, "Item");
        }

        [Parser(Opcode.CMSG_BLACK_MARKET_OPEN)]
        public static void HandleBlackMarketOpen(Packet packet)
        {
            packet.ReadPackedGuid128("NpcGUID");
        }

        [Parser(Opcode.CMSG_BLACK_MARKET_REQUEST_ITEMS)]
        public static void HandleBlackMarketRequestItems(Packet packet)
        {
            packet.ReadPackedGuid128("NpcGUID");
            packet.ReadTime64("LastUpdate");
        }

        [Parser(Opcode.CMSG_BLACK_MARKET_BID_ON_ITEM)]
        public static void HandleBlackMarketBidOnItem(Packet packet)
        {
            packet.ReadPackedGuid128("NpcGUID");
            packet.ReadInt32("MarketID");
            packet.ReadUInt64("BidAmount");
            Substructures.ItemHandler.ReadItemInstance(packet, "Item");
        }
    }
}
