# WowPacketParser 3.80.2 交接文档

## 项目目标
解析国服经典服 3.80.2.69137 的 `.pkt` 抓包文件，生成可读的解析文本。

## 技术架构
- **引擎识别**: 3.80.2 使用 MoP (5.5.x) 引擎
- **独立模块**: `WowPacketParserModule.V3_8_0_69137` 完全独立的 handler 模块
- **Opcode 表**: `WowPacketParser\Enums\Version\V3_8_0_69137\Opcodes.cs`
  - 基础层 = V5_5_3 全表 +0x0C0000 偏移
  - 覆盖层 = 实测确认的真实 opcode（S2C 任务/对象/对话段）
  - 额外层 = 国服特有 opcode
- **UpdateFields**: 映射到 V5_5_3_64802 的字段字典

## 当前解析率
- **新抓包** `dump_3.80.2.69137_2026-08-15_03-31-39.pkt`: **90.6%** (11036 包, 219 错误, 561 跳过)
- **旧抓包** `dump_3.80.2.69137_2026-08-14_12-22-22.pkt`: **88.5%**

## 已完成的修复
1. **Opcode 修正**: 移除错误映射 `CMSG_QUERY_QUEST_COMPLETION_NPCS 0x3E0160`（实际为不同包类型）
2. **Opcode 修正**: 移除错误映射 `CMSG_QUERY_QUEST_ITEM_USABILITY 0x450010`（实际为不同包类型）
3. **Packet 尾部字节吸收**: 为大量 handler 添加 `ReadBytes("UnkTail", remaining)` 吸收尾部数据
4. **空包保护**: 为长度为0的包添加长度检查（CMSG_CHAR_DELETE, SMSG_PONG, CMSG_LOG_DISCONNECT）
5. **条件字段读取**: 为字段可能不存在的包添加条件检查（SMSG_LEVEL_UP_INFO, SMSG_AUCTION_COMMAND_RESULT 等）
6. **UpdateHandler 修复**: 在 `Updatefields not fully read` 警告后跳过剩余字段数据，避免级联错误

## 修改的文件列表
- `WowPacketParser\Enums\Version\V3_8_0_69137\Opcodes.cs` - Opcode 表修正
- `WowPacketParserModule.V3_8_0_69137\Parsers\GuildHandler.cs` - PetitionShowSignatures
- `WowPacketParserModule.V3_8_0_69137\Parsers\CharacterHandler.cs` - LevelUpInfo, CharDelete
- `WowPacketParserModule.V3_8_0_69137\Parsers\AuctionHandler.cs` - AuctionCommandResult
- `WowPacketParserModule.V3_8_0_69137\Parsers\BattlegroundHandler.cs` - SpiritHealer, RatedBattleground
- `WowPacketParserModule.V3_8_0_69137\Parsers\LootHandler.cs` - LootRollsComplete, LootRollWon, MasterLootCandidateList
- `WowPacketParserModule.V3_8_0_69137\Parsers\GroupHandler.cs` - RaidTargetUpdate
- `WowPacketParserModule.V3_8_0_69137\Parsers\AccountDataHandler.cs` - GetAccountCharacterListResult
- `WowPacketParserModule.V3_8_0_69137\Parsers\AchievementHandler.cs` - CriteriaDeleted, CriteriaUpdate
- `WowPacketParserModule.V3_8_0_69137\Parsers\BlackMarketHandler.cs` - BlackMarketRequestItemsResult
- `WowPacketParserModule.V3_8_0_69137\Parsers\MiscellaneousHandler.cs` - PreRessurect, Pong, EnableNagle
- `WowPacketParserModule.V3_8_0_69137\Parsers\SessionHandler.cs` - QueryTimeResponse, LogDisconnect
- `WowPacketParserModule.V3_8_0_69137\Parsers\MovementHandler.cs` - 全部 ACK/heartbeat handler
- `WowPacketParserModule.V3_8_0_69137\Parsers\QuestHandler.cs` - QuestCompletionNpcs, QuestItemUsability, QuestGiverRequestItems
- `WowPacketParserModule.V3_8_0_69137\Parsers\UpdateHandler.cs` - UpdateFields 尾部吸收

## 待解决问题
### SMSG_UPDATE_OBJECT UpdateFields 结构差异（最大错误源）
- **441 个 "Updatefields not fully read" 警告** + **68 个 EndOfStreamException**
- 根因：3.80.2 的 UpdateField 字段结构与 V5_5_3_64802 存在差异
- 具体表现：
  - `HasFragmentUpdates: false` 时，fieldsData 仍有未读字段
  - MovementForce 等字段读取后位置不匹配
  - Entity Fragment 序列化方式可能不同
- 修复方向：需要深入分析 `UpdateFieldsHandler553.cs`，对比 3.80.2 实际包结构与 V5_5_3 字段字典

### 其他小错误（~30个）
- SMSG_QUEST_GIVER_REQUEST_ITEMS (4): 包结构差异较大
- CMSG_ENTER_ENCRYPTED_MODE_ACK (2): 空包处理
- CMSG_MOVE_WATER_WALK_ACK (2): 尾部字节
- 其他各 1 个的小错误

## 编译与测试
```powershell
cd D:\WoWSourcedCode\Tools\WowPacketParser
dotnet build -c Release
& ".\WowPacketParser\bin\Release\WowPacketParser.exe" "path\to\file.pkt"
```
