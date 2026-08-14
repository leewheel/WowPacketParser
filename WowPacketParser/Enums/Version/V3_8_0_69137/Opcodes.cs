using System.Collections.Generic;
using WowPacketParser.Misc;
using WowPacketParser.Enums.Version.V5_5_3_64802;

namespace WowPacketParser.Enums.Version.V3_8_0_69137
{
    /// <summary>
    /// 国服经典服 3.80.2 (build 69137) 专用 Opcode 表。
    ///
    /// 【历史教训】早期假设"所有 opcode = V5_5_3 + 0x0C0000 统一偏移"，已被实测证明错误：
    ///   统一偏移对 V5_5_3 的 SMSG 命中率仅 ~9%（约 101/1072）。
    ///   正确规律：V5_5_3 的低段系统 opcode（&lt;0x420000：0x3A00xx→0x4600xx、0x3E00xx→0x4A00xx、
    ///   0x3F00xx→0x4B00xx、0x4000xx→0x4C00xx、0x4100xx→0x4D00xx、0x4200xx→0x4E00xx）确实整体 +0x0C0000；
    ///   但 V5_5_3 高段在国服发生了独立重排（任务/对话包在 0x6400xx，而非派生的 0x5B00xx 等）。
    ///
    /// 【实现】基础层 = V5_5_3 全表 +0x0C0000；覆盖层 = 实测确认的真实 opcode（先移除冲突派生值再覆盖）。
    /// </summary>
    public static class Opcodes_3_8_0
    {
        private const int OpcodeOffset = 0x0C0000;

        // 国服实测真实 opcode 覆盖（S2C 任务/对象/对话段），来源：大抓包实测
        private static readonly Dictionary<Opcode, int> ServerOverrides = new Dictionary<Opcode, int>
        {
            // 任务/对话段（0x4F00xx 任务段 → 国服 0x6400xx，已实测验证结构兼容 V5_5_0 QuestHandler）
            { Opcode.SMSG_QUEST_GIVER_QUEST_DETAILS,        0x640012 },
            { Opcode.SMSG_QUEST_GIVER_OFFER_REWARD_MESSAGE,  0x640014 },
            { Opcode.SMSG_QUEST_GIVER_REQUEST_ITEMS,         0x640016 },
            // 对象更新（V5_5_3 的 0x4A0000 SMSG_UPDATE_OBJECT → 国服 0x5C0000，大包 40KB/高频率特征匹配）
            { Opcode.SMSG_UPDATE_OBJECT,                      0x5C0000 },
            // 法术/光环（V5_5_3 的 0x510011 SMSG_AURA_UPDATE → 国服 0x660011，高频小包特征匹配）
            { Opcode.SMSG_AURA_UPDATE,                         0x660011 },
        };

        public static BiDictionary<Opcode, int> Opcodes(Direction direction)
        {
            var result = new BiDictionary<Opcode, int>();
            var overrides = direction == Direction.ServerToClient ? ServerOverrides : null;

            foreach (var baseEntry in Opcodes_5_5_3.Opcodes(direction))
            {
                var newValue = baseEntry.Value + OpcodeOffset;

                // 若该 Opcode 有国服真实覆盖值，则用覆盖值替换派生值
                if (overrides != null && overrides.TryGetValue(baseEntry.Key, out var realValue))
                    newValue = realValue;

                // 避免 BiDictionary 对同一数值重复（国服实测才可能出现的值冲突，忽略重复键）
                if (!result.ContainsValue(newValue))
                    result.Add(baseEntry.Key, newValue);
            }

            return result;
        }
    }
}
