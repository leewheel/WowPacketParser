using System;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V1_60_1_69876.Parsers
{
    public static class NpcHandler
    {
        // WoW Forever 1.60.1 SMSG_TRAINER_LIST.
        //
        // The client decoder uses 33 bytes per spell (TrinityCoreForever NPCPackets.cpp: "client
        // decoder rva 0x8162E0, 33 bytes per spell"), while the 12.x layout this build used to
        // inherit from reads 30 - that is why the official sniffs decoded the first spell and then
        // produced garbage from the second one on.
        //
        // Layout: [guid + a few bytes][int8 TrainerType][uint32 TrainerID][uint32 count]
        //         count x 33 bytes (int32 SpellID, int32 0, uint32 MoneyCost, uint8 ReqLevel,
        //                            uint32 ReqSkillLine, uint32 ReqSkillRank, int32 ReqAbility[3])
        //         11-bit MSB-first greeting length (flushed to a byte boundary) + UTF-8 greeting
        //
        // The header is 21 or 22 bytes depending on which guid encoding the server used, and the
        // official server's guid does not match the emulator's, so the spell block is located by
        // validating a complete parse rather than by hard coding an offset.
        private const int SpellStride = 33;

        private static uint ReadUInt32(byte[] data, int offset)
        {
            return BitConverter.ToUInt32(data, offset);
        }

        private static int ReadInt32(byte[] data, int offset)
        {
            return BitConverter.ToInt32(data, offset);
        }

        private static int ReadBits(byte[] data, int bitOffset, int count)
        {
            int value = 0;
            for (int i = 0; i < count; i++)
            {
                int bit = bitOffset + i;
                value = (value << 1) | ((data[bit / 8] >> (7 - (bit % 8))) & 1);
            }
            return value;
        }

        private sealed class TrainerList
        {
            public int Header;
            public int Type;
            public uint Id;
            public int[] SpellId;
            public uint[] MoneyCost;
            public byte[] ReqLevel;
            public uint[] ReqSkillLine;
            public uint[] ReqSkillRank;
            public int[][] ReqAbility;
            public string Greeting;
        }

        private static TrainerList TryParseAt(byte[] data, int header)
        {
            if (header < 10 || header + 4 > data.Length)
                return null;

            int type = data[header - 9];
            if (type > 4)
                return null;

            uint id = ReadUInt32(data, header - 8);
            int count = (int)ReadUInt32(data, header - 4);
            if (id == 0 || id > 1000000 || count < 1 || count > 200)
                return null;

            var list = new TrainerList
            {
                Header = header,
                Type = type,
                Id = id,
                SpellId = new int[count],
                MoneyCost = new uint[count],
                ReqLevel = new byte[count],
                ReqSkillLine = new uint[count],
                ReqSkillRank = new uint[count],
                ReqAbility = new int[count][],
            };

            int off = header;
            for (int i = 0; i < count; i++)
            {
                if (off + SpellStride > data.Length)
                    return null;

                list.SpellId[i] = ReadInt32(data, off);
                list.MoneyCost[i] = ReadUInt32(data, off + 8);
                list.ReqLevel[i] = data[off + 12];
                list.ReqSkillLine[i] = ReadUInt32(data, off + 13);
                list.ReqSkillRank[i] = ReadUInt32(data, off + 17);
                list.ReqAbility[i] = new[] { ReadInt32(data, off + 21), ReadInt32(data, off + 25), ReadInt32(data, off + 29) };

                if (list.SpellId[i] <= 0 || list.SpellId[i] >= 4000000 || list.MoneyCost[i] > 10000000 ||
                    list.ReqSkillLine[i] > 100000 || list.ReqSkillRank[i] > 1000 || list.ReqLevel[i] > 80)
                    return null;

                off += SpellStride;
            }

            for (int bit = 0; bit < 8; bit++)
            {
                int length = ReadBits(data, off * 8 + bit, 11);
                int start = (off * 8 + bit + 11 + 7) / 8;
                if (length <= 0 || length > 2000 || start + length != data.Length)
                    continue;

                try
                {
                    list.Greeting = System.Text.Encoding.UTF8.GetString(data, start, length);
                    return list;
                }
                catch (ArgumentException)
                {
                }
            }

            return null;
        }

        [Parser(Opcode.SMSG_TRAINER_LIST, ClientVersionBuild.V1_60_1_69876)]
        public static void HandleServerTrainerList(Packet packet)
        {
            byte[] data = packet.GetStream(0);
            packet.SetPosition(packet.Length);

            for (int header = 10; header < Math.Min(30, data.Length); header++)
            {
                TrainerList list = TryParseAt(data, header);
                if (list == null)
                    continue;

                packet.WriteLine();
                packet.WriteLine("TrainerType: " + list.Type + " (" + (TrainerType)list.Type + ")");
                packet.WriteLine("TrainerID: " + list.Id);
                packet.WriteLine("Spells: " + list.SpellId.Length);
                for (int i = 0; i < list.SpellId.Length; i++)
                {
                    packet.WriteLine("[" + i + "] SpellID: " + list.SpellId[i] +
                                     " MoneyCost: " + list.MoneyCost[i] +
                                     " ReqSkillLine: " + list.ReqSkillLine[i] +
                                     " ReqSkillRank: " + list.ReqSkillRank[i] +
                                     " ReqLevel: " + list.ReqLevel[i] +
                                     " ReqAbility: " + list.ReqAbility[i][0] + "/" + list.ReqAbility[i][1] + "/" + list.ReqAbility[i][2]);
                }
                packet.WriteLine("Greeting: " + list.Greeting);
                return;
            }
        }
    }
}
