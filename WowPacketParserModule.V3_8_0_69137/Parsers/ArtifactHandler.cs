using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V3_8_0_69137.Parsers
{
    public static class ArtifactHandler
    {
        [Parser(Opcode.SMSG_OPEN_ARTIFACT_FORGE)]
        [Parser(Opcode.SMSG_ARTIFACT_RESPEC_PROMPT)]
        public static void HandleOpenArtifactForge(Packet packet)
        {
            packet.ReadPackedGuid128("ArtifactGUID");
            packet.ReadPackedGuid128("ForgeGUID");
        }

        [Parser(Opcode.SMSG_ARTIFACT_XP_GAIN)]
        public static void HandleArtifactXPGain(Packet packet)
        {
            packet.ReadPackedGuid128("ArtifactGUID");
            packet.ReadUInt64("Amount");
        }
    }
}
