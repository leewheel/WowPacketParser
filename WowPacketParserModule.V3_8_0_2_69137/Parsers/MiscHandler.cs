using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V3_8_0_69137.Parsers
{
    /// <summary>
    /// 国服经典服 3.80.2 (build 69137) 专用解析器。
    ///
    /// 【本模块的定位】
    /// 本模块是 3.80.2 的"主模块"（VersionDefiningBuild = V3_8_0_69137），
    /// V5_5_0_61735 模块作为 fallback 加载。分发规则（见 Handler.Parse）：
    ///   1. 先查本模块 (V3_8_0_69137, opcode) 的 [Parser] 方法 —— 命中即用；
    ///   2. 未命中再查 fallback 模块 (V5_5_0_61735, opcode)；
    ///   3. 最后查主 exe 的默认 handler。
    /// 因此：与 V5_5_3 结构相同的包无需任何代码（自动走 fallback），
    /// 只有 3.80.2 结构不同 / 需要解密 / 需要修正的包才在这里写 handler 覆盖。
    ///
    /// 【handler 编写约定】
    ///   - 类必须是 public static（加载器要求 IsAbstract && IsPublic）；
    ///   - 方法必须是 public static，第一个参数类型为 Packet；
    ///   - 用 [Parser(Opcode.XXX)] 标注；Opcode 枚举名与 V5_5_3 相同
    ///     （opcode 数值偏移 0x0C0000 已在 Opcodes_3_8_0 表中统一处理，这里不用管）。
    ///
    /// 【示例】（去掉注释即可用，确认结构无误后再启用）
    /// </summary>
    public static class MiscHandler
    {
        // [Parser(Opcode.SMSG_PONG)]
        // public static void HandlePong(Packet packet)
        // {
        //     // 3.80.2 的 SMSG_PONG 结构与 V5_5_3 不同（示例：少了 Int32 字段），
        //     // 在这里按实际结构读取即可覆盖 fallback 的实现。
        //     packet.ReadInt32("Serial");
        // }

        // 如需调用 V5_5_0 模块的基线实现（项目已引用该模块）：
        // WowPacketParserModule.V5_5_0_61735.Parsers.XxxHandler.HandleXxx(packet);
    }
}
