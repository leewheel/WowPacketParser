using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V3_80_2_69137.Parsers
{
    public static class ConversationHandler
    {
        [Parser(Opcode.CMSG_CONVERSATION_LINE_STARTED)]
        public static void HandleConversationLineStarted(Packet packet)
        {
            packet.ReadPackedGuid128("ConversationGUID");
            packet.ReadUInt32("ConversationLineID");
        }
    }
}
