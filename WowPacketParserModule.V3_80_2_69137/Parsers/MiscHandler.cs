using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V3_80_2_69137.Parsers
{
    /// <summary>
    /// 国服经典服 3.80.2 (build 69137) 专用解析器。
    ///
    /// 【本模块的定位】
    /// 本模块是 3.80.2 的完全独立主模块（VersionDefiningBuild = V3_80_2_69137），
    /// 不引用任何其它版本（V5_5_0/V5_5_3 等）的 handler。分发规则：
    ///   1. 查本模块 (V3_80_2_69137, opcode) 的 [Parser] 方法；
    ///   2. 未命中则走主 exe 的默认 (Zero) handler。
    ///
    /// 本模块的全部解析能力来自 Parsers/ 下自带的业务 handler 与本模块自己的
    /// UpdateFields 副本（UpdateFields/V3_80_2_69137，命名空间含 V3_80_2_69137 是
    /// WPP 加载机制所需，与 GetUpdateFieldDictionaryBuild 返回值一致）。
    ///
    /// 【国服独有的 opcode 覆盖】
    /// 国服 3.8 的 opcode 低段 = V5_5_3 低段 +0x0C0000，高段独立重排。
    /// 已在 Opcodes_3_8_0.cs 中覆盖：任务段 0x6400xx、对象更新 0x5C0000、光环 0x660011。
    ///
    /// 【handler 编写约定】
    ///   - 类必须 public static（加载器要求 IsAbstract && IsPublic）；
    ///   - 方法必须 public static，第一个参数类型为 Packet；
    ///   - 用 [Parser(Opcode.XXX)] 标注，Opcode 枚举名与 V5_5_3 相同。
    ///
    /// 【示例】为 3.80.2 结构不同的包写覆盖 handler：
    /// </summary>
    public static class MiscHandler
    {
        // [Parser(Opcode.SMSG_PONG)]
        // public static void HandlePong(Packet packet)
        // {
        //     // 3.80.2 的 SMSG_PONG 结构与标准不同，按实际结构读取。
        //     packet.ReadInt32("Serial");
        // }
    }
}
