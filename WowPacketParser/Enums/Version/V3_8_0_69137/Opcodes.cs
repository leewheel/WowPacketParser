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
        {            // 任务/对话段（0x4F00xx 任务段 → 国服 0x6400xx，末字节一一对应，已实测验证：
            //   0x4F0012→0x640012 QUEST_DETAILS、0x4F0014→0x640014 OFFER_REWARD、
            //   0x4F0013→0x640013 REQUEST_ITEMS、0x4F0016→0x640016 QUERY_QUEST_INFO_RESPONSE）
            { Opcode.SMSG_QUEST_GIVER_QUEST_DETAILS,        0x640012 },
            { Opcode.SMSG_QUEST_GIVER_OFFER_REWARD_MESSAGE,  0x640014 },
            { Opcode.SMSG_QUEST_GIVER_REQUEST_ITEMS,         0x640013 },
            // 注意：0x640016 曾误映射为 REQUEST_ITEMS（结构不符，实测为 QUERY_QUEST_INFO_RESPONSE，
            // 载荷以 QuestID 开头 + RewardChoiceItem + 目标数据 + 文本，与 0x3E0135 查询一一对应）
            { Opcode.SMSG_QUERY_QUEST_INFO_RESPONSE,          0x640016 },
            // 对象更新（V5_5_3 的 0x4A0000 SMSG_UPDATE_OBJECT → 国服 0x5C0000，大包 40KB/高频率特征匹配）
            { Opcode.SMSG_UPDATE_OBJECT,                      0x5C0000 },
            // 法术/光环（V5_5_3 的 0x510011 SMSG_AURA_UPDATE → 国服 0x660011，高频小包特征匹配）
            { Opcode.SMSG_AURA_UPDATE,                         0x660011 },
            // 移动段（V5_5_3 的 0x4C00xx 移动段 → 国服 0x5E00xx，结构不同，自定义 handler 解析）
            { Opcode.SMSG_MOVE_UPDATE,                          0x5E000E },
            { Opcode.SMSG_ON_MONSTER_MOVE,                      0x5E0002 },
            // 国服新增包（V5_5_3 无对应枚举，使用 Opcode 主枚举中的预留值）
            { Opcode.SMSG_CRITERIA_UNKNOWN,                     0x46018F },
        };

        // 国服实测 C2S opcode 覆盖（V5_5_3 派生值在国服被重排，实测载荷验证）
        private static readonly Dictionary<Opcode, int> ClientOverrides = new Dictionary<Opcode, int>
        {
            // 0x3F0026 实测 8 个实例载荷均为 QuestGiverGUID(packed128)+QuestID+bit，
            // 与 CMSG_QUEST_GIVER_ACCEPT_QUEST 结构完全吻合（接取任务：783/5261/33/7/3102/18/15/93950）；
            // V5_5_3 派生值 0x3F0026=CMSG_JOIN_RATED_BATTLEGROUND 错误
            { Opcode.CMSG_QUEST_GIVER_ACCEPT_QUEST,              0x3F0026 },
        };

        public static BiDictionary<Opcode, int> Opcodes(Direction direction)
        {
            var result = new BiDictionary<Opcode, int>();
            var overrides = direction == Direction.ServerToClient ? ServerOverrides : ClientOverrides;

            foreach (var baseEntry in Opcodes_5_5_3.Opcodes(direction))
            {
                var newValue = baseEntry.Value + OpcodeOffset;
                bool isOverridden = false;

                // 若该 Opcode 有国服真实覆盖值，则用覆盖值替换派生值
                if (overrides != null && overrides.TryGetValue(baseEntry.Key, out var realValue))
                {
                    newValue = realValue;
                    isOverridden = true;
                }

                // 跳过派生值落入国服不存在段的 opcode（VoidStorage 段 0x5E00xx 等在国服被重排，派生值无意义）
                // 覆盖值（如 0x640012/0x5E0002 等）不受此限制
                if (!isOverridden && direction == Direction.ServerToClient && newValue >= 0x5E0000 && newValue < 0x5F0000)
                    continue;

                // 国服 C2S 重排：V5_5_3 的 CMSG_JOIN_RATED_BATTLEGROUND(0x330026) 派生值 0x3F0026
                // 实测为 CMSG_QUEST_GIVER_ACCEPT_QUEST（8 个实例载荷=guid+QuestID），跳过派生避免占用冲突
                if (!isOverridden && direction == Direction.ClientToServer && baseEntry.Key == Opcode.CMSG_JOIN_RATED_BATTLEGROUND)
                    continue;

                // 避免 BiDictionary 对同一数值重复
                if (!result.ContainsValue(newValue))
                    result.Add(baseEntry.Key, newValue);
            }

            // 额外添加国服特有 opcode（V5_5_3 表里没有对应枚举，无法通过派生+覆盖处理）
            // 对所有方向生效（S2C + C2S）
            {
                var extra = new Dictionary<Opcode, int>
                {
                    { Opcode.SMSG_CRITERIA_UNKNOWN, 0x46018F },
                    { Opcode.SMSG_UNKNOWN_3, 0x5E0000 },
                    { Opcode.SMSG_UNKNOWN_8, 0x660021 },
                    { Opcode.SMSG_UNKNOWN_13, 0x660024 },
                    { Opcode.SMSG_UNKNOWN_19, 0x660029 },
                    { Opcode.SMSG_UNKNOWN_22, 0x66002A },
                    { Opcode.SMSG_UNKNOWN_23, 0x66002B },
                    { Opcode.SMSG_UNKNOWN_24, 0x660048 },
                    { Opcode.SMSG_UNKNOWN_28, 0x460321 },
                    // 实测 0x630027 载荷以 Count(UInt32)+Result+PlayerGuid 开头且含中文玩家名，
                    // 与 V3_4 HandleNameQueryResponse/ReadNameCacheLookupResult 结构完全吻合 → 玩家名查询响应
                    { Opcode.SMSG_QUERY_PLAYER_NAMES_RESPONSE, 0x630027 },
                    { Opcode.SMSG_UNKNOWN_54, 0x640011 },
                    { Opcode.SMSG_UNKNOWN_56, 0x64001B },
                    { Opcode.SMSG_CRITERIA_UPDATE, 0x4602BA },
                    // 实测 0x3E0135 载荷=QuestID(6B)，与 0x640016 查询响应一一对应 → CMSG_QUERY_QUEST_INFO
                    // （0x3E0114 载荷实为 packed128 guid，是另一类 guid 查询，非任务查询）
                    { Opcode.CMSG_QUERY_QUEST_INFO, 0x3E0135 },
                    { Opcode.CMSG_QUERY_QUEST_COMPLETION_NPCS, 0x3F0022 },
                    { Opcode.CMSG_QUERY_QUESTS_COMPLETED, 0x45000F },
                    { Opcode.CMSG_GOSSIP_SELECT_OPTION, 0x3E0115 },
                };
                foreach (var e in extra)
                    if (!result.ContainsValue(e.Value))
                        result.Add(e.Key, e.Value);
            }

            return result;
        }
    }
}
