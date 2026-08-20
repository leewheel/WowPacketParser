using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V3_4_5_61815.Parsers
{
    public static class GarrisonHandler
    { 
        [Parser(Opcode.CMSG_ADVENTURE_MAP_START_QUEST, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAdventureMapStartQuest(Packet packet)
        {
            packet.ReadInt32<QuestId>("QuestID");
        }
    }
}
