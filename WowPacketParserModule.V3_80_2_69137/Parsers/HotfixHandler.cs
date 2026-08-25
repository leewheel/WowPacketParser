using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using WowPacketParser.Enums;
using WowPacketParser.Hotfix;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using WowPacketParser.Proto;
using WowPacketParser.Store;
using WowPacketParser.Store.Objects;

namespace WowPacketParserModule.V3_80_2_69137.Parsers
{
    public static class HotfixHandler
    {
        [Parser(Opcode.SMSG_AVAILABLE_HOTFIXES)]
        public static void HandleAvailableHotfixes(Packet packet)
        {
            packet.ReadUInt32("VirtualRealmAddress");
            var hotfixCount = packet.ReadUInt32("HotfixCount");

            for (var i = 0u; i < hotfixCount; ++i)
            {
                packet.ReadUInt32("UniqueID", i, "HotfixUniqueID");
                packet.ReadInt32("PushID", i, "HotfixUniqueID");
            }
        }

        [Parser(Opcode.CMSG_HOTFIX_REQUEST)]
        public static void HandleHotfixRequest553(Packet packet)
        {
            packet.ReadUInt32("CurrentBuild");
            packet.ReadUInt32("InternalBuild");
            var hotfixCount = packet.ReadUInt32("HotfixCount");
            for (var i = 0u; i < hotfixCount; ++i)
                packet.ReadInt32("HotfixID", i);
        }

        [Parser(Opcode.CMSG_DB_QUERY_BULK)]
        public static void HandleDbQueryBulk(Packet packet)
        {
            packet.ReadInt32E<DB2Hash>("TableHash");

            var count = packet.ReadBits("Count", 13);
            for (var i = 0; i < count; ++i)
                packet.ReadInt32("RecordID", i);
        }

        // 国服 3.80.2 实测 DB_REPLY 结构（与 V9 一致，33 字节样本精确对齐）：
        //   TableHash(u32) + RecordID(i32) + Timestamp(u32) + Status(2bits) + Allow(bit) + Size(i32) + Data(Size 字节)
        // 核心 ItemHandler 的 DB_REPLY 仅覆盖 V4_3_0 之前，国服版本无 handler → 185 个包仅 hex dump 且计为错误
        [Parser(Opcode.SMSG_DB_REPLY)]
        public static void HandleDBReply(Packet packet)
        {
            packet.ReadUInt32("TableHash");
            packet.ReadInt32("RecordID");
            packet.ReadUInt32("Timestamp");

            packet.ResetBitReader();
            packet.ReadBits("Status", 2);
            packet.ReadBit("Allow");

            var size = packet.ReadInt32("Size");
            if (size > 0)
                packet.ReadBytes("Data", size);
        }
    }
}
