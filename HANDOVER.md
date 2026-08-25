# WowPacketParser 3.80.2 交接文档

## 项目目标
解析国服经典服 3.80.2.69137 的 `.pkt` 抓包文件，生成可读的解析文本。

## 技术架构
- **引擎识别**: 3.80.2 使用 MoP (5.5.x) 引擎
- **独立模块**: `WowPacketParserModule.V3_80_2_69137` 完全独立的 handler 模块
- **Opcode 表**: `WowPacketParser\Enums\Version\V3_80_2_69137\Opcodes.cs`
  - 基础层 = V5_5_3 全表 +0x0C0000 偏移
  - 覆盖层 = 实测确认的真实 opcode（S2C 任务/对象/对话段）
  - 额外层 = 国服特有 opcode
- **UpdateFields**: 映射到 V5_5_3_64802 的字段字典

## 当前解析率（2026-08-18 第三轮修复后）
| 抓包文件 | 解析率 | 错误数 | 说明 |
|----------|--------|--------|------|
| 08-14_08-17-39 (24876 包) | **93.4%** | 554 (2.2%) | 主游戏段，修复前 88.6%/1553 错误 |
| 08-15_03-31-39 (11036 包) | **92.0%** | 199 (1.8%) | 修复前 85.7%/762 |
| 08-18_01-51-24 (8252 包) | **91.7%** | 147 (1.8%) | 新抓包，修复前 86.7%/313 |
| 08-14_15-50-38 (6044 包) | **91.2%** | 128 (2.1%) | 修复前 84.1%/433 |
| 08-14_12-22-22 (2106 包) | **83.6%** | 71 (3.4%) | 修复前 75.0%/128 |

## 第三轮修复（2026-08-18）— 本轮核心突破
1. **【重大】UpdateFields handler 映射修正**: `GetUpdateFieldDictionaryBuild(V3_80_2_69137)` 从 V3_4_0_45166 改为 **V5_5_3_64802**
   - 根因：handler (UpdateFieldsHandler553) 注册 key 为 V5_5_3_64802，但字典映射查 V3_4_0_45166 → GetHandler() 返回 null → Create 阶段 NullReferenceException
   - 效果：消除 NRE，UPDATE_OBJECT 错误从 1446 → 459
2. **【重大】EntityFragmentID 枚举修正**: `ReadEntityFragments` 从 `WowCSEntityFragments1100` 改为 **`WowCSEntityFragments1127`**
   - 根因：国服 3.80.2 fragment 值域为 1127 风格（Tag_GameObject=206、Tag_Container=201、CGObject=2），旧代码用 1100 读取（Tag_Container=2）导致 206 触发 ArgumentOutOfRangeException，整个 UPDATE_OBJECT 包中断
   - 效果：EntityFragmentID 输出从错误（Tag_Container/206）变为正确（CGObject/Tag_GameObject）
3. **【重大】Opcode 映射修正**: 0x640016 从 `SMSG_QUEST_GIVER_REQUEST_ITEMS` 改为 **`SMSG_QUERY_QUEST_INFO_RESPONSE`**
   - 根因：V5_5_3 任务段 0x4F00xx → 国服 0x6400xx 末字节一一对应（0x4F0016=QUERY_RESPONSE→0x640016），真正的 REQUEST_ITEMS 是 **0x640013**
   - 证据：0x640016 载荷以 QuestID 开头 + RewardChoiceItem + 目标数据 + 文本，与 C2S 0x3E0135 一一对应（各 2 次）
4. **SMSG_DB_REPLY handler 新增**: V3_8 模块 HotfixHandler 添加 DB_REPLY（TableHash+RecordID+Timestamp+Status(2bits)+Allow+Size+Data），185 个包从"无结构"转正
5. **QUERY_QUEST_INFO_RESPONSE 结构修正**（0x640016）:
   - HasData 从 1 bit 改为 **1 字节**（0x80=有数据）
   - Artifact 字段顺序：XPDifficulty + **CategoryID** + XPMultiplier（标准为 XPDifficulty+XPMultiplier+CategoryID）
   - ItemDropQuantity 从 4 组改为 **5 组**（RewardItems 段 68 字节）
   - **Sound 字段顺序**：CompleteSoundKitID + AreaGroupID + AcceptedSoundKitID（标准为 Accepted+Complete+Area），实测 890/878/0 全部合理
   - **目标前缀 +8 字节**：ConditionalQuestCompletionLogCount 后 2 个额外 int32（目标段前固定区 32B 而非 24B）
   - **目标结构 +9 字节**：每个目标 43 字节 = V5_5 标准 33B + 2 个额外 int32(8B) + desc bits(1B) + 对齐(1B)
   - 效果：RewardChoiceItemID(3270/3273/3272)、目标(380388/1504/8 与 380389/1505/5)、LogTitle/LogDescription/QuestDescription 全部正确解析
   - 参照 3.4.4.61581 抓包（D:\WoWSourcedCode\Sniff\3.4.5\ymir_classic_wotlk_3.4.4.61581\dumps）验证标准结构差异
6. **FormatFloat NaN 防御**: `Substring(0,20)` 对 NaN/Infinity 短字符串越界 → 加 Math.Min 防护
7. **PlayerChoice 对齐上游**: `ForceDontShowChoicesAsList` 属性上游已重命名为 `HasPowerChoice`（commit 656c350eb 同步改 V5_5_0_61735/V9_0_1_36216），V3_8 对齐

## 修改的文件列表（第三轮）
- `WowPacketParser\Enums\Version\UpdateFields.cs` - V3_8 UpdateFields 字典映射 → V5_5_3_64802
- `WowPacketParserModule.V3_80_2_69137\Parsers\UpdateHandler.cs` - ReadEntityFragments 用 1127 枚举
- `WowPacketParser\Enums\Version\V3_80_2_69137\Opcodes.cs` - 0x640016 → QUERY_RESPONSE、0x640013 → REQUEST_ITEMS
- `WowPacketParserModule.V3_80_2_69137\Parsers\HotfixHandler.cs` - 新增 SMSG_DB_REPLY
- `WowPacketParserModule.V3_80_2_69137\Parsers\QuestHandler.cs` - QUERY_RESPONSE 结构修正（HasData 1B/Artifact 顺序/ItemDropQuantity 5 组）、PlayerChoice HasPowerChoice
- `WowPacketParser\Misc\PacketReads.cs` - FormatFloat NaN 防御

## 待解决问题（剩余错误 ~147-554/文件）
1. **SMSG_UPDATE_OBJECT movement 块结构差异（最大头）**: MovementForceCount 读到 1078530011（=float 3.167 速度值），说明 movement 块内字段错位。疑点：
   - MovementFlag3/StepUpStartElevation/AdvFlying 系列/MovementForce 系统可能是 5.5.0 PTR 实验字段，国服 3.80.2（正式 MoP 引擎）未必全部存在
   - 已实验去掉 MovementFlags3 无效，需要进一步对比国服 movement 块实测字节（MoverGUID 后逐字段）
   - 参考 V5_4_8_18291（正式 MoP）movement 结构：无 Flags3/AdvFlying/MovementForce
2. **QUERY_QUEST_INFO_RESPONSE 目标段 1 字节偏移**: VisualEffects 循环读到中文文本字节（文本长度 bits 数与标准不同，目标段前偏移 1 字节）
3. **CMSG_QUERY_QUESTS_COMPLETED (0x45000F) 等 C2S 包**: 结构差异
4. **未识别 C2S opcode**: 0x450010、0x3E0060、0x3E0008、0x3F0068 等（国服 C2S 段重排，V5_5_3 无对应），可加吸收 handler 减少"无结构"计数

## 已验证的国服结构事实（重要参考）
- 任务段 S2C：0x6400xx = V5_5_3 0x4F00xx 末字节一一对应
- EntityFragmentID：1127 风格值域（不是 1100）
- QUERY_RESPONSE HasData：1 字节（不是 bit）
- DB_REPLY：V9 风格结构
- **SMSG_LOG_XP_GAIN（第四轮 8d1c4e42 修复）**：guid 后 8 字节恒定前缀（18507488 + 140749825，全包恒定），真实 Amount 在其后（偏移 +13），最后 GroupBonus(float)。实测 144 个包全部同构。修正前 Amount 错位读到 18507488。
- **C2S 任务段（第五轮 17f0c3931 修复，实测验证）**：
  - 0x3F0026 = CMSG_QUEST_GIVER_ACCEPT_QUEST（8 实例载荷=QuestGiverGUID+QuestID+bit；V5_5_3 派生值 JOIN_RATED_BG 错误，需跳过派生再覆盖）
  - 0x3E0135 = CMSG_QUERY_QUEST_INFO（载荷=QuestID(4B)+2B 尾部共 6B；旧映射 0x3E0114 实为 guid 查询）
  - 0x640011 = SMSG_QUEST_GIVER_STATUS_MULTIPLE（开头 Count 非 QuestID，勿误判）
  - 0x630027 = SMSG_QUERY_PLAYER_NAMES_RESPONSE（玩家名+种族职业，Count+Result+PlayerGuid 结构）
  - 任务查询请求在 ConnIdx 1、响应回包可能在 ConnIdx 0（跨连接回包），确认 QuestID 必须看 C2S 请求载荷

## 编译与测试
```powershell
cd D:\WoWSourcedCode\Tools\WowPacketParser
dotnet build -c Release
& ".\WowPacketParser\bin\Release\WowPacketParser.exe" "path\to\file.pkt"
```
- 启用错误日志：bin/Release/WowPacketParser.dll.config 中 LogPacketErrors=true
