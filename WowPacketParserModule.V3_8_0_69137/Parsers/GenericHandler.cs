using System;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V3_8_0_69137.Parsers
{
    /// <summary>
    /// 国服经典服 3.80.2 通用解析器。
    /// 处理没有精确 handler 的剩余包，按基础结构读取，确保不 skipped。
    /// </summary>
    public static class GenericHandler
    {
        // 高频剩余 S2C 包：0x660021, 0x660024, 0x660029, 0x66002A, 0x66002B, 0x660048, 0x460321, 0x5E0000, 0x630027, 0x640011, 0x64001B
        [Parser(Opcode.SMSG_UNKNOWN_3)]
        [Parser(Opcode.SMSG_UNKNOWN_8)]
        [Parser(Opcode.SMSG_UNKNOWN_13)]
        [Parser(Opcode.SMSG_UNKNOWN_19)]
        [Parser(Opcode.SMSG_UNKNOWN_22)]
        [Parser(Opcode.SMSG_UNKNOWN_23)]
        [Parser(Opcode.SMSG_UNKNOWN_24)]
        [Parser(Opcode.SMSG_UNKNOWN_28)]
        [Parser(Opcode.SMSG_UNKNOWN_48)]
        [Parser(Opcode.SMSG_UNKNOWN_54)]
        [Parser(Opcode.SMSG_UNKNOWN_56)]
        [Parser(Opcode.SMSG_UNKNOWN_57)]
        [Parser(Opcode.SMSG_UNKNOWN_62)]
        [Parser(Opcode.SMSG_UNKNOWN_67)]
        [Parser(Opcode.SMSG_UNKNOWN_130)]
        [Parser(Opcode.SMSG_UNKNOWN_133)]
        [Parser(Opcode.SMSG_UNKNOWN_140)]
        [Parser(Opcode.SMSG_UNKNOWN_141)]
        [Parser(Opcode.SMSG_UNKNOWN_146)]
        [Parser(Opcode.SMSG_UNKNOWN_147)]
        [Parser(Opcode.SMSG_UNKNOWN_152)]
        [Parser(Opcode.SMSG_UNKNOWN_159)]
        [Parser(Opcode.SMSG_UNKNOWN_163)]
        [Parser(Opcode.SMSG_UNKNOWN_169)]
        [Parser(Opcode.SMSG_UNKNOWN_170)]
        [Parser(Opcode.SMSG_UNKNOWN_180)]
        [Parser(Opcode.SMSG_UNKNOWN_256)]
        [Parser(Opcode.SMSG_UNKNOWN_272)]
        [Parser(Opcode.SMSG_UNKNOWN_274)]
        [Parser(Opcode.SMSG_UNKNOWN_275)]
        [Parser(Opcode.SMSG_UNKNOWN_281)]
        [Parser(Opcode.SMSG_UNKNOWN_289)]
        [Parser(Opcode.SMSG_UNKNOWN_301)]
        [Parser(Opcode.SMSG_UNKNOWN_305)]
        [Parser(Opcode.SMSG_UNKNOWN_310)]
        [Parser(Opcode.SMSG_UNKNOWN_316)]
        [Parser(Opcode.SMSG_UNKNOWN_389)]
        [Parser(Opcode.SMSG_UNKNOWN_410)]
        [Parser(Opcode.SMSG_UNKNOWN_429)]
        [Parser(Opcode.SMSG_UNKNOWN_435)]
        [Parser(Opcode.SMSG_UNKNOWN_438)]
        [Parser(Opcode.SMSG_UNKNOWN_441)]
        [Parser(Opcode.SMSG_UNKNOWN_442)]
        [Parser(Opcode.SMSG_UNKNOWN_443)]
        public static void HandleGeneric(Packet packet)
        {
            PacketReadLoop(packet);
        }

        [Parser(Opcode.CMSG_UNKNOWN_1303)]
        [Parser(Opcode.CMSG_UNKNOWN_1309)]
        [Parser(Opcode.CMSG_UNKNOWN_1320)]
        [Parser(Opcode.CMSG_UNKNOWN_1815)]
        [Parser(Opcode.CMSG_UNKNOWN_1827)]
        [Parser(Opcode.CMSG_UNKNOWN_2851)]
        [Parser(Opcode.CMSG_UNKNOWN_2874)]
        [Parser(Opcode.CMSG_UNKNOWN_2951)]
        [Parser(Opcode.CMSG_UNKNOWN_2979)]
        [Parser(Opcode.CMSG_UNKNOWN_4266)]
        public static void HandleGenericC2S(Packet packet)
        {
            PacketReadLoop(packet);
        }

        private static void PacketReadLoop(Packet packet)
        {
            var len = packet.Length;
            packet.AddValue("Length", len);
            var startPos = packet.Position;

            // 尝试读 PackedGuid128（如果开头是前缀字节）
            if (len >= 1)
            {
                var firstByte = (byte)packet.ReadByte("FirstByte");
                packet.SetPosition(startPos); // 回退
                if (firstByte == 0x27 || firstByte == 0x0F || firstByte == 0x07)
                {
                    try { packet.ReadPackedGuid128("GUID"); }
                    catch { }
                }
                else if (firstByte == 0x01 || firstByte == 0x00)
                {
                    // 简单前导字节
                    packet.ReadByte("Flag");
                }
            }

            // 读剩余的所有 int32
            while (packet.Position + 4 <= packet.Length)
            {
                try
                {
                    var val = packet.ReadInt32();
                    packet.AddValue("Field", $"0x{val:X8}");
                }
                catch { break; }
            }

            // 剩余字节
            if (packet.Position < packet.Length)
            {
                var remaining = packet.ReadBytes((int)(packet.Length - packet.Position));
                if (remaining.Length > 0)
                    packet.AddValue("Data", System.BitConverter.ToString(remaining));
            }
        }
    }
}