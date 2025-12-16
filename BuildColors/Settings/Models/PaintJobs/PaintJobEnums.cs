using ProtoBuf;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    [ProtoContract]
    public enum PaintRuleLogicalOperator {
        [ProtoEnum]
        And = 0,
        [ProtoEnum]
        Or = 1
    }

    [ProtoContract]
    public enum PaintRuleConditionType {
        [ProtoEnum]
        BlockColor = 0,
        [ProtoEnum]
        BlockDefinition = 1,
        [ProtoEnum]
        BlockSkin = 2
    }

    [ProtoContract]
    public enum PaintRuleComparison {
        [ProtoEnum]
        Equals = 0,
        [ProtoEnum]
        NotEquals = 1
    }
}
