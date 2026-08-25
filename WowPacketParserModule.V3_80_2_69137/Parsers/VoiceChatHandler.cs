using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V3_80_2_69137.Parsers
{
    public static class VoiceChatHandler
    {
        [Parser(Opcode.CMSG_VOICE_CHAT_JOIN_CHANNEL)]
        public static void HandleVoiceChatJoinChannel(Packet packet)
        {
            packet.ReadByte("ChannelID");
        }

        [Parser(Opcode.CMSG_VOICE_CHAT_LOGIN)]
        public static void HandleVoiceChatZero(Packet packet)
        {
        }
    }
}
