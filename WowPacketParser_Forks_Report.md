# WowPacketParser 活跃 Fork「超前代码」详细分析报告

> 分析对象：GitHub 上 [TrinityCore/WowPacketParser](https://github.com/TrinityCore/WowPacketParser) 的全部 fork
> 分析时间：2026-08-25
> 方法：通过 GitHub API 拉取全部 358 个 fork，并用 compare 接口逐一对比其与上游 `master` 分支的领先/落后情况；对每一个「领先（ahead>0）」的 fork，提取其独有提交（剔除合并提交与机器人提交），按提交信息与分支名判定其针对的游戏版本，并归纳其**功能性**内容。
> 过程数据均保留在仓库根目录下的 `TempFiles/` 目录（fork 列表、对比结果、各 fork 提交详情、分类索引、速查表）。

---

## 一、范围与总览

- 上游仓库 [TrinityCore/WowPacketParser](https://github.com/TrinityCore/WowPacketParser) 共有 **358** 个可访问 fork（GitHub 记录为 388，部分已被删除）。
- 经逐一对比，**68 个 fork 的默认分支相比上游 `master` 存在「超前提交」**（即包含上游没有的改动）。
- 但「数字上超前」≠「有功能改进」。本报告对 68 个 fork 逐一标注了**真实功能提交数**（剔除单纯合并上游、机器人同步、初始化等无意义提交）。大量 fork 的「超前」只是反复把上游合并进自己，或自动化工具制造的噪声。

---

## 二、关键结论（先看这里）

### 1. 你提到的版本号，真实含义如下

| 你写的版本 | 实际对应的游戏内容 | 说明 |
|---|---|---|
| **3.80.x / 3.8.x** | 《熊猫人之谜》经典服**国服（中国大陆）**客户端 `3.8.0.x` | 由 `leewheel` 的提交直接证实：`Add V3_8_0_69137 module (MoP Classic China)`、`Handle CN 3.80.2 packet format variations`。国际服同一资料片叫 5.4.x / 5.5.x，只是国服客户端版本号被命名为 3.8.0。 |
| **5.5.x** | 《熊猫人之谜》经典服国际服 `5.5.3` | `zzlyns` 已把解析器更新到 5.5.3.66382 / 5.5.3.66565。许多老 fork 仍停留在 5.4.7 / 5.4.8。 |
| **3.4.x** | 《巫妖王之怒》经典服（WotLK Classic）`3.4.x` | 上游与多个 fork 都在持续跟进。 |
| **1.1.5** | 经典旧世 / 探索赛季（Classic Era / Season of Discovery），客户端 `1.14.x` / `1.15.x` | 你写的「1.1.5」应是 1.14/1.15 的近似写法。`TheSCREWEDSoftware` 的 1.15.7 即为探索赛季（SoD）。 |
| **4.4.x** | 《大地的裂变》经典服（Cataclysm Classic）`4.4.x` | ⚠️ **重要：在所有「默认分支超前」的 fork 中，专门、持续维护 4.4.x 的几乎不存在**（详见 3.4 节）。 |

### 2. 各目标版本中「真正有实质内容」的 fork

- **熊猫人之谜 / 国服 3.8.0（5.4.x、5.5.x）**
  - `leewheel`（你自己的 fork，最新最完整，2026-08）
  - `zzlyns`（5.5.3 国际服）
  - `ChipLeo`（体量最大，839 条超前，另有同源家族 `Megosa` / `SkyFire` / `SkyFireArchives` / `belowzero` / `PavelDev`）
- **巫妖王经典 3.4.x**
  - `RioMcBoo`（最活跃且最新，2024–2025）
  - `leewheel`（3.4.5）、`DavuKnight`、`lineagedr`、`Vanheden`、`killerwife`（在分支中）
- **经典旧世 / 探索赛季（即你写的 1.1.5）**
  - `TheSCREWEDSoftware`（1.15.7 探索赛季）、`killerwife`（分支 SodVersions / 1_14_4）、`HelloKitty`（1.12.1 分支）
- **大地的裂变 4.4.x**：基本空白（见 3.4 节）
- **其他版本（非你点名、但有实质超前内容，值得记录）**
  - TBC 2.4.3：`Tremolo4`
  - 军团再临 7.3.5：`The-Legion-Preservation-Project`、`mdX7`
  - 燃烧远征经典 2.5.x：`WowLegacyCore`
  - 零售（BfA / 巨龙时代 / 地心之战 8/10/11）：`lyosky`（BfA 8.0.1 最全）、`dio85`、`AHigi`、`Christyan`

### 3. 重要陷阱提示
很多 fork 的「超前数」极具误导性：
- `BuloZB` 显示 69 条超前，但**全部是自动化机器人「activity sync」提交，零功能改动**。
- `ratkosrb` 显示 10 条超前，但**全部是合并上游的提交**（其真正的大灾变/TBC 工作只存在于分支里，不在默认分支）。
- `dio85` 显示 71 条超前，但只有 **6 条**是真实功能，其余均为合并。
- `Gultask` 显示 6 条超前，但提交信息是 `init` / `yea` 等，**无实质内容**（真正的 WotLK 工作在其 `wotlkclassic` 分支）。

---

## 三、按版本逐一详述「超前代码到底做了什么」

### 3.1 熊猫人之谜经典服（含国服 3.8.0 / 国际服 5.4.x、5.5.x）

#### [leewheel/WowPacketParser](https://github.com/leewheel/WowPacketParser)（你自己的 fork，26 条超前、0 落后，2026-08 最新）
这是目前**最完整、最新**的熊猫人之谜经典服解析 fork，且同时覆盖国服与国际服差异。其功能包括：
- **新增国服《熊猫人之谜》经典服独立模块（客户端 3.8.0.69137）**：作为一个完全独立、现代化的解析模块存在，不再依赖 5.5.0 / 5.5.3 的代码，专门处理中国大陆服务器特有的包格式。
- **处理国服客户端 3.80.2 的包格式差异**：针对国服与国际服同一资料片下数据包结构不同的情况做适配；跳过仓库（VoidStorage）误判段、优化指令编号（opcode）覆盖层。
- **大幅提升整体解析率**：通过打通移动相关数据包、识别国服新增数据包、批量映射「服务器→客户端」与部分「客户端→服务器」此前未被识别的数据包，把整体解析率从 **65.6% 一路提升到 93.4%**。
- **修正具体游戏内容的解析**：
  - 移动、任务、查询等数据块的尾部字节与包体结构差异；
  - 新抓包的解析（签名/请愿、角色升级、拍卖行、掉落等）；
  - **任务数据包做到 100% 解析**（修正任务信息查询回包的目标数据段结构）；
  - **经验获取恢复正常**（修正国服经验包结构）；
  - **玩家名称查询**功能确认可用；
  - **接取任务可被正确识别**（修正客户端→服务器方向的任务指令映射）。
- **同步支持《巫妖王之怒》经典服 3.4.5**（构建 63697+）的解析器更新。

> 一句话：这个 fork 让「国服熊猫人经典服」的抓包从大量乱码变成可完整还原任务、移动、拍卖、掉落、经验、玩家名等游戏内容。

#### [zzlyns/WowPacketParser](https://github.com/zzlyns/WowPacketParser)（12 条超前，2026-03）
- 把《熊猫人之谜》经典服解析器更新到 **5.5.3**（构建 66382、66565），并修正指令编号值的不匹配。属于国际服 5.5.3 的跟进型更新。

#### [ChipLeo/WowPacketParser](https://github.com/ChipLeo/WowPacketParser)（839 条超前，体量最大）+ 同源家族 Megosa / SkyFire / SkyFireArchives / belowzero / PavelDev
这组 fork 源自同一批 2014 年的《熊猫人之谜》5.4.x 工作（提交内容高度一致），它们对 **5.4.1 / 5.4.2 / 5.4.7 / 5.4.8** 做了当时最全面的解析覆盖，功能涵盖：
- **登录与账号**：登录认证、角色登录、世界/传送相关数据包。
- **生物（怪物 / NPC）信息**：可从抓包还原怪物的名称、类型、模型等基础资料。
- **游戏对象（宝箱、门、传送器等）信息**。
- **任务系统**：任务内容、任务 POI（地图坐标点）、任务奖励的完整解析。
- **NPC 对话（gossip）菜单** 与 **NPC 文本**（对话内容）。
- **银行交互**、**拍卖行**（含拍卖行所有者竞价通知）。
- **法术 / 技能初始列表**。
- **场景对象更新与移动**：单位/物体的生成、位置、旋转、攻击目标、生物刷新点；玩家与单位的移动轨迹。
- **背包 / 物品列表**、**聊天**、**伤害 / 治疗日志**、**副本场景**、**专业 / 交易技能**、**地图难度** 等。

其中几个家族成员的增量差异：
- **SkyFire / SkyFireArchives**：在 5.4.8（构建 18414）基础上，额外补充了怪物「最小/最大伤害」的数据库输出、对象销毁包解析，并把生物/物体刷新点解析得更准确；SkyFireArchives 还明确说明「从 ChipLeo 仓库搬运了一些内容」。
- **belowzero**：在 5.4.8 基础上补充了拍卖行、银行、世界传送等更多包。
- **SkyFireTools**：把指令编号命名对齐到 SkyFire 的命名规范，便于与 SkyFire 服务端配套使用。
- **LegacyCorporation / LegacyPacketParser**：补了 4.3.0 的几个指令编号（偏老版本零售）。

> 这组 fork 的实际价值：它们是「熊猫人经典服 5.4.x 时代」解析能力的集大成者，但工作集中在 2014 年，之后基本停止更新。

#### 小结（熊猫人方向）
- 想要**最新、覆盖国服 3.8.0 且解析率最高** → 看 `leewheel`。
- 想要**国际服 5.5.3** → 看 `zzlyns`。
- 想要**5.4.x 时代最全的历史解析** → 看 `ChipLeo` 及其同源家族。

---

### 3.2 巫妖王之怒经典服（3.4.x）

#### [RioMcBoo/WowPacketParser](https://github.com/RioMcBoo/WowPacketParser)（28 条超前，全部真实，2024–2025 活跃，默认分支 `FractalCore`）
这是目前**最活跃、最新**的巫妖王经典服解析 fork，功能包括：
- **支持为特定客户端构建版本单独创建数据包结构**（按版本区分解析逻辑，而不是一刀切）。
- **覆盖 3.4.1 / 3.4.3 / 3.4.4 多个构建**：
  - 日历相关数据包（日历发送、日历事件提醒）；
  - 公会银行查询、公会权限/拒绝邀请；
  - 死亡骑士符文转换；
  - 专业技能展示；
  - **随机副本 / 队伍查找器**（组队加入、查找器列表的申请/邀请/离开、时间同步确认等）——即巫妖王经典服里的「随机本」与「组队工具」功能；
  - 角色移动惯性确认、使用物品、攻击挥击日志、副本倒计时、流媒体错误上报、热修请求等具体交互；
  - 修正了 GUID（全局唯一标识）读取错误。
- 此外其仓库还有 `handlers_refactor`、`sanitizing`、`splitLocale`、`multiple_sql` 等多个分支，说明在做解析器结构的重构与数据库输出增强。

#### 其它巫妖王方向 fork
- **leewheel**：除熊猫人外，也把《巫妖王》经典服 3.4.5（构建 63697+）纳入解析。
- **DavuKnight**（2022）：在巫妖王经典服开服时补了对应的指令与更新字段，并额外把移动数据暴露出来供外部程序调用。
- **lineagedr**（2023）：修正了 3.4.3 下「角色列表结果」包的解析。
- **Vanheden**（2023）：补了 3.4.2.50664 构建。
- **Gultask**（2026）：默认分支只是初始化，真正的 WotLK Classic 工作在其 `wotlkclassic` 分支。
- **killerwife**：默认分支仅 1 条提交，但拥有 `version/WotlkClassic46902` 等巫妖王相关分支。

---

### 3.3 经典旧世 / 探索赛季（对应你写的「1.1.5」）

- **[TheSCREWEDSoftware/WowPacketParser](https://github.com/TheSCREWEDSoftware/WowPacketParser)**（12 条超前，但仅 1 条真实功能）：新增了 **1.15.7.62797** 构建支持——这是《魔兽世界》「探索赛季（Season of Discovery）」的客户端版本。其余提交均为合并上游。其仓库还有 `64857`、`67156` 等以构建号命名的分支。
- **[killerwife/WowPacketParser](https://github.com/killerwife/WowPacketParser)**：默认分支仅「1.14.3 的移植」1 条提交，但其分支极其丰富，包含 `feature/SodVersions`、`feature/1_14_4`、`feature/1143` 等，是一个覆盖探索赛季、经典旧世 1.14.x、TBC、巫妖王经典的多版本「港口型」fork（真正工作大多在分支而非默认分支）。
- **[HelloKitty/WowPacketParser](https://github.com/HelloKitty/WowPacketParser)**：拥有 `1_12_1_updatepacket` 分支，针对最早的经典旧世 1.12.1 做数据包更新。

> 说明：你写的「1.1.5」最接近的是经典旧世 / 探索赛季（1.14.x / 1.15.x）。目前专门做这一方向的 fork 不多，且多数以分支形式存在。

---

### 3.4 大地的裂变经典服（4.4.x）——重要如实说明

**在所有「默认分支超前」的 68 个 fork 中，没有找到专门、持续维护 4.4.x（大地的裂变经典服）的 fork。** 具体情况：
- 版本归类工具把 `RioMcBoo` 误标为含 4.4.x——但其实际提交全是 3.4.x（巫妖王）内容，并无大灾变相关改动。
- `ratkosrb` 的仓库里确实有 `cataclysm`、`classicsom` 等分支保留了大地的裂变 / 经典旧世赛季相关工作，但其**默认分支的 10 条超前提交全部是「合并上游」**，没有任何功能改动。

**结论：4.4.x（大地的裂变经典服）在目前的 fork 生态里基本是空白**，若你需要这一方向的解析，基本只能基于上游或从零补齐。

---

### 3.5 其它「实质超前且有内容」的版本 fork（非你点名，但值得记录）

- **TBC 2.4.3（燃烧的远征旧版）**：`Tremolo4` 做了 2.4.3 模块，修正了移动、对象更新等解析（其描述即「rudimentary TBC 2.4.3 support」）。
- **军团再临 7.3.5**：`The-Legion-Preservation-Project` 修正了军团再临的 DBC 使用、SQL 加载，以及 `creature_template` / `creature_template_addon` / `creature_template_difficulty` 的数据库输出（用于该资料片的数据提取与存档）；`mdX7` 补了 7.3.5.26972 构建。
- **燃烧远征经典 2.5.x**：`WowLegacyCore` 做了 BCC（Burning Crusade Classic）2.5.1~2.5.4 的构建支持、数据库输出，以及任务/法术/拍卖等包修正。
- **零售（BfA / 巨龙时代 / 地心之战，客户端 8/10/11）**：
  - `lyosky`（272 条超前，2018–2019）：对《争霸艾泽拉斯》8.0.1 做了当时最全面的解析，覆盖角色枚举、法术、光环、任务、聊天、怪物移动、副本、公会、场景、神器、PvP、社交等海量包结构修正，是零售 8.0 时代最完整的 fork 之一。
  - `dio85`（2023–2026）：在零售方向补了挑战模式、BfA 的组队查找器、**战团分组（Warbands）**、**商栈 / 月度奖励（PerksProgram）**、**制造订单（CraftingOrder）** 等当前版本功能。
  - `AHigi`（2025）：修正 11.1.0 史诗钥石（大秘境）成员种族变量读取。
  - `Christyan`（2024）：修正 11.0.5 之后的位向量读取。
- **老零售 4.x–7.x（历史资料片）**：`RazorCore`（5.1）、`DrEhsan` / `Malylolek` / `lorac2k14`（7.0.3 / 7.1.0）、`horn`（6.2.4）、`Epicurus4`（6.1.0）、`Subv`（4.2.2 / 4.0.6）、`skypeak` / `LegacyCorporation`（4.3.4 / 4.3.0）、`Supabad`（4.2.0）——多为对当年资料片的零散包补充。
- **不绑定特定版本的通用改进**：`QAston`（把解析结果输出到 SQLite 数据库）、`Mesielecat`（玩家选择系统的 SQL 生成）、`ennioVisco`（XML 输出支持）、`Subv`（自动解析对话菜单与路点）、`Sar777`（动画工具包、6.x 商人）、`chaodhib`（移动包解析增强）、`dufernst`（增加包计数统计）、`maanuel`（Sniff Viewer 输出适配）。

---

## 四、「数字超前但无实质功能」的 fork 清单（避免被 ahead 数误导）

以下 fork 在对比中显示「超前」，但经核查其默认分支并无有意义的功能改动，请勿据此判断其价值：

| Fork | 显示超前数 | 真实功能提交 | 实际情况 |
|---|---|---|---|
| BuloZB | 69 | 0 | 全部为自动化「activity sync」机器人提交 |
| ratkosrb | 10 | 0 | 全部为合并上游；真正工作只在分支 |
| PowerpuffIO | 13 | 1 | 仅修复了含西里尔字母用户名时的构建错误 |
| Gultask | 6 | 0 | 提交为 `init`/`yea` 等；WotLK 工作在分支 |
| dio85 | 71 | 6 | 其余均为合并上游 |
| TheSCREWEDSoftware | 12 | 1 | 仅新增 1.15.7 构建；其余合并 |
| bozoweed / AshamaneProject / Legolast-Manu / Daniel25 / imbavirus / jtongzhi / Fatliner / LuigiElleBalotta / wanglxi1 / stefanursu1234 | 各 1–4 | 0 | 仅合并或 README 改动 |
| CraftedRO / moboqe / cyberium / mdX7 / Kittnz / Vanheden / lineagedr / killerwife / DavuKnight | 各 1–6 | 1–5 | 仅有极少量真实提交 |

---

## 五、附录：全部 68 个「超前」fork 速查表

> RealFeat = 剔除合并/机器人后的真实功能提交数；VersionTags 为启发式版本归类（ASCII 标签，仅作参考）。

| Fork | Ahead | Behind | LastPush | RealFeat | VersionTags |
|------|-------|--------|----------|----------|------------|
| [aedansilver/WowPacketParser_2](https://github.com/aedansilver/WowPacketParser_2) | 5 | 5952 | 2012-07-27 | 3 | Other/Uncat |
| [AHigi/WowPacketParser](https://github.com/AHigi/WowPacketParser) | 2 | 502 | 2025-03-03 | 2 | Retail(8/10/11) |
| [Altairfree/WowPacketParser](https://github.com/Altairfree/WowPacketParser) | 3 | 1606 | 2021-09-13 | 3 | Other/Uncat |
| [Ancient/WowPacketParser](https://github.com/Ancient/WowPacketParser) | 34 | 5348 | 2014-04-23 | 32 | MoP-Classic(5.4/5.5/3.8-CN); Old-Retail(4-7) |
| [AshamaneProject/WowPacketParser](https://github.com/AshamaneProject/WowPacketParser) | 2 | 2311 | 2018-01-04 | 1 | Other/Uncat |
| [belowzero/WowPacketParser](https://github.com/belowzero/WowPacketParser) | 38 | 5348 | 2014-05-30 | 35 | MoP-Classic(5.4/5.5/3.8-CN); SkyFire(5.4.8); Old-Retail(4-7) |
| [bozoweed/WowPacketParser](https://github.com/bozoweed/WowPacketParser) | 2 | 2539 | 2016-09-23 | 0 | Other/Uncat |
| [BuloZB/WowPacketParser](https://github.com/BuloZB/WowPacketParser) | 69 | 11 | 2026-08-03 | 0 | Other/Uncat |
| [chaodhib/WowPacketParser](https://github.com/chaodhib/WowPacketParser) | 3 | 2504 | 2016-12-22 | 3 | Other/Uncat |
| [ChipLeo/WowPacketParser](https://github.com/ChipLeo/WowPacketParser) | 839 | 199 | 2025-11-16 | 458 | MoP-Classic(5.4/5.5/3.8-CN); SkyFire(5.4.8); Old-Retail(4-7) |
| [Christyan/WowPacketParser](https://github.com/Christyan/WowPacketParser) | 2 | 660 | 2024-10-30 | 2 | Retail(8/10/11) |
| [CraftedRO/WowPacketParser](https://github.com/CraftedRO/WowPacketParser) | 1 | 968 | 2025-10-30 | 1 | Other/Uncat |
| [cyberium/WowPacketParser](https://github.com/cyberium/WowPacketParser) | 1 | 1509 | 2026-05-24 | 1 | Other/Uncat |
| [Daniel25/WowPacketParser](https://github.com/Daniel25/WowPacketParser) | 3 | 1712 | 2021-01-26 | 0 | Other/Uncat |
| [danlapps/WowPacketParser](https://github.com/danlapps/WowPacketParser) | 1 | 3116 | 2015-02-23 | 1 | Other/Uncat |
| [DavuKnight/WowPacketParser](https://github.com/DavuKnight/WowPacketParser) | 6 | 1310 | 2022-09-05 | 5 | Other/Uncat |
| [DDuarte/WowPacketParser](https://github.com/DDuarte/WowPacketParser) | 4 | 2468 | 2017-03-26 | 4 | Other/Uncat |
| [dio85/WowPacketParser](https://github.com/dio85/WowPacketParser) | 71 | 10 | 2026-08-11 | 6 | Retail(8/10/11) |
| [DrEhsan/WowPacketParser](https://github.com/DrEhsan/WowPacketParser) | 3 | 2700 | 2016-07-27 | 2 | Old-Retail(4-7) |
| [dufernst/WowPacketParser](https://github.com/dufernst/WowPacketParser) | 1 | 2955 | 2015-05-14 | 1 | Other/Uncat |
| [ennioVisco/WowPacketParser](https://github.com/ennioVisco/WowPacketParser) | 3 | 2474 | 2017-03-11 | 3 | Other/Uncat |
| [Epicurus4/WowPacketParser](https://github.com/Epicurus4/WowPacketParser) | 5 | 3104 | 2015-02-28 | 1 | Old-Retail(4-7) |
| [Fatliner/WowPacketParser](https://github.com/Fatliner/WowPacketParser) | 1 | 2023 | 2019-08-13 | 0 | Other/Uncat |
| [Gooyeth/WowPacketParser](https://github.com/Gooyeth/WowPacketParser) | 5 | 2790 | 2015-10-20 | 1 | Other/Uncat |
| [Gultask/WowPacketParser](https://github.com/Gultask/WowPacketParser) | 6 | 2 | 2026-08-22 | 0 | WotLK-Classic(3.4) |
| [HelloKitty/WowPacketParser](https://github.com/HelloKitty/WowPacketParser) | 1 | 2287 | 2018-06-01 | 1 | Other/Uncat |
| [hexenir/WowPacketParser](https://github.com/hexenir/WowPacketParser) | 2 | 1164 | 2023-01-30 | 2 | Other/Uncat |
| [horn/WowPacketParser](https://github.com/horn/WowPacketParser) | 1 | 2711 | 2016-04-07 | 1 | Old-Retail(4-7) |
| [imbavirus/WowPacketParser](https://github.com/imbavirus/WowPacketParser) | 4 | 2783 | 2015-11-14 | 0 | Other/Uncat |
| [jtongzhi/WowPacketParser](https://github.com/jtongzhi/WowPacketParser) | 3 | 2706 | 2016-04-26 | 0 | Other/Uncat |
| [killerwife/WowPacketParser](https://github.com/killerwife/WowPacketParser) | 1 | 1348 | 2026-08-04 | 1 | WotLK-Classic(3.4); Classic-Era/SoD(1.x); BCC(2.5); Old-Retail(4-7) |
| [Kittnz/WowPacketParser](https://github.com/Kittnz/WowPacketParser) | 1 | 2472 | 2024-08-29 | 1 | Other/Uncat |
| [leewheel/WowPacketParser](https://github.com/leewheel/WowPacketParser) | 26 | 0 | 2026-08-24 | 22 | MoP-Classic(5.4/5.5/3.8-CN); Old-Retail(4-7) |
| [LegacyCorporation/LegacyPacketParser](https://github.com/LegacyCorporation/LegacyPacketParser) | 4 | 6680 | 2012-01-25 | 2 | SkyFire(5.4.8); Old-Retail(4-7) |
| [Legolast-Manu/WowPacketParser](https://github.com/Legolast-Manu/WowPacketParser) | 1 | 1294 | 2022-09-10 | 0 | Other/Uncat |
| [lineagedr/WowPacketParser](https://github.com/lineagedr/WowPacketParser) | 2 | 1034 | 2023-10-16 | 2 | WotLK-Classic(3.4); Old-Retail(4-7) |
| [lorac2k14/WowPacketParser](https://github.com/lorac2k14/WowPacketParser) | 4 | 2506 | 2016-12-17 | 3 | Old-Retail(4-7) |
| [LuigiElleBalotta/WowPacketParser](https://github.com/LuigiElleBalotta/WowPacketParser) | 1 | 2489 | 2017-11-09 | 1 | Other/Uncat |
| [lyosky/WowPacketParser](https://github.com/lyosky/WowPacketParser) | 272 | 2031 | 2019-08-06 | 248 | Retail(8/10/11) |
| [maanuel/WowPacketParser](https://github.com/maanuel/WowPacketParser) | 2 | 6686 | 2012-01-19 | 2 | Other/Uncat |
| [Malylolek/WowPacketParser](https://github.com/Malylolek/WowPacketParser) | 3 | 2598 | 2016-08-19 | 3 | Old-Retail(4-7) |
| [mdX7/WowPacketParser](https://github.com/mdX7/WowPacketParser) | 1 | 2280 | 2026-02-06 | 1 | Legion(7.3.5) |
| [Megosa/WowPacketParser](https://github.com/Megosa/WowPacketParser) | 51 | 5348 | 2014-05-05 | 44 | MoP-Classic(5.4/5.5/3.8-CN) |
| [Mesielecat/WowPacketParser](https://github.com/Mesielecat/WowPacketParser) | 9 | 1964 | 2023-04-04 | 8 | Other/Uncat |
| [mikefernandz/WowPacketParser](https://github.com/mikefernandz/WowPacketParser) | 1 | 1629 | 2021-08-05 | 1 | Other/Uncat |
| [moboqe/WowPacketParser](https://github.com/moboqe/WowPacketParser) | 1 | 674 | 2024-10-11 | 1 | Other/Uncat |
| [PavelDev/WowPacketParser](https://github.com/PavelDev/WowPacketParser) | 33 | 5348 | 2014-03-06 | 32 | MoP-Classic(5.4/5.5/3.8-CN) |
| [PowerpuffIO/WowPacketParser](https://github.com/PowerpuffIO/WowPacketParser) | 13 | 11 | 2026-07-27 | 1 | Other/Uncat |
| [QAston/WowPacketParser](https://github.com/QAston/WowPacketParser) | 33 | 5482 | 2013-07-18 | 30 | Other/Uncat |
| [ratkosrb/WowPacketParser](https://github.com/ratkosrb/WowPacketParser) | 10 | 1266 | 2026-04-17 | 0 | Cata-Classic(4.4) |
| [RazorCore/WowPacketParser](https://github.com/RazorCore/WowPacketParser) | 13 | 5517 | 2013-03-03 | 12 | Old-Retail(4-7) |
| [RioMcBoo/WowPacketParser](https://github.com/RioMcBoo/WowPacketParser) | 28 | 323 | 2025-10-10 | 28 | WotLK-Classic(3.4); Cata-Classic(4.4); Old-Retail(4-7) |
| [Sar777/WowPacketParser](https://github.com/Sar777/WowPacketParser) | 19 | 2703 | 2016-06-05 | 5 | Other/Uncat |
| [SkyFireArchives/WowPacketParser](https://github.com/SkyFireArchives/WowPacketParser) | 47 | 5348 | 2014-07-23 | 44 | MoP-Classic(5.4/5.5/3.8-CN); SkyFire(5.4.8); Old-Retail(4-7) |
| [SkyFireTools/WowPacketParser](https://github.com/SkyFireTools/WowPacketParser) | 5 | 2784 | 2016-10-30 | 5 | SkyFire(5.4.8) |
| [SkyFire/WowPacketParser](https://github.com/SkyFire/WowPacketParser) | 42 | 5348 | 2014-07-21 | 39 | MoP-Classic(5.4/5.5/3.8-CN); SkyFire(5.4.8); Old-Retail(4-7) |
| [skypeak/WowPacketParser_SkyFire](https://github.com/skypeak/WowPacketParser_SkyFire) | 18 | 6330 | 2012-05-27 | 3 | SkyFire(5.4.8); Old-Retail(4-7) |
| [stefanursu1234/WowPacketParser](https://github.com/stefanursu1234/WowPacketParser) | 2 | 4091 | 2014-10-29 | 2 | Other/Uncat |
| [Subv/WowPacketParser](https://github.com/Subv/WowPacketParser) | 36 | 6986 | 2011-12-02 | 11 | Old-Retail(4-7) |
| [Supabad/WowPacketParser](https://github.com/Supabad/WowPacketParser) | 1 | 7073 | 2011-11-20 | 1 | Other/Uncat |
| [The-Legion-Preservation-Project/WowPacketParser](https://github.com/The-Legion-Preservation-Project/WowPacketParser) | 6 | 364 | 2025-06-06 | 6 | Legion(7.3.5) |
| [TheSCREWEDSoftware/WowPacketParser](https://github.com/TheSCREWEDSoftware/WowPacketParser) | 12 | 32 | 2026-05-28 | 1 | Classic-Era/SoD(1.x) |
| [Tremolo4/WowPacketParser](https://github.com/Tremolo4/WowPacketParser) | 6 | 2317 | 2018-01-02 | 6 | TBC(2.4.3); Old-Retail(4-7) |
| [Vanheden/WowPacketParser](https://github.com/Vanheden/WowPacketParser) | 1 | 1068 | 2023-08-01 | 1 | WotLK-Classic(3.4); Old-Retail(4-7) |
| [wanglxi1/WowPacketParser](https://github.com/wanglxi1/WowPacketParser) | 1 | 2782 | 2015-11-30 | 0 | Other/Uncat |
| [WowLegacyCore/WowPacketParser](https://github.com/WowLegacyCore/WowPacketParser) | 19 | 1420 | 2022-03-23 | 15 | MoP-Classic(5.4/5.5/3.8-CN); BCC(2.5); Old-Retail(4-7) |
| [Zakamurite/WowPacketParser](https://github.com/Zakamurite/WowPacketParser) | 1 | 6042 | 2012-08-05 | 1 | Other/Uncat |
| [zzlyns/WowPacketParser](https://github.com/zzlyns/WowPacketParser) | 12 | 83 | 2026-03-25 | 9 | MoP-Classic(5.4/5.5/3.8-CN) |

---

## 六、给你的建议（基于以上发现）

1. **熊猫人之谜方向**：若以国服 3.8.0 为主，`leewheel`（你自己的）已是最优解；若要国际服 5.5.3 跟进，`zzlyns` + 你的 5.5.0/5.5.3 基础可互补；若需要 5.4.x 时代最全的解析沉淀，参考 `ChipLeo` 家族。
2. **巫妖王经典 3.4.x**：`RioMcBoo` 是当前最值得跟进的活跃 fork。
3. **4.4.x（大地的裂变经典）目前是空白**——若你有此需求，这是一片蓝海，需要从上游或 3.4.x/5.4.x 的代码基础上自行补齐。
4. **警惕「虚高超前数」**：评估任何 fork 时，请优先看「真实功能提交数（RealFeat）」而非「Ahead」列，本报告第四节已列出所有需警惕的 fork。

> 过程文件均位于 `TempFiles/`：`forks_all.json`（全部 fork 元数据）、`compare_results.json`（领先/落后对比）、`forks_detail/*.json`（各 fork 分支与独有提交详情）、`digests.txt`（剔除合并后的真实提交摘要）、`category_index.json` 与 `appendix_table.md`（分类与速查表）。

