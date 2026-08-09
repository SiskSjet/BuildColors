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
        BlockSkin = 2,
        [ProtoEnum]
        BlockCategory = 3,
        [ProtoEnum]
        GridSize = 4,
        [ProtoEnum]
        AnyBlock = 5,
        [ProtoEnum]
        BlockIntegrity = 6
    }

    [ProtoContract]
    public enum PaintRuleIntegrityState {
        [ProtoEnum]
        Intact = 0,
        [ProtoEnum]
        Damaged = 1,
        [ProtoEnum]
        Incomplete = 2,
        [ProtoEnum]
        BelowThreshold = 3
    }

    [ProtoContract]
    public enum PaintRuleComparison {
        [ProtoEnum]
        Equals = 0,
        [ProtoEnum]
        NotEquals = 1
    }

    [ProtoContract]
    public enum PaintRuleBlockCategory {
        [ProtoEnum]
        Armor = 0,
        [ProtoEnum]
        LightArmor = 1,
        [ProtoEnum]
        HeavyArmor = 2,
        [ProtoEnum]
        Functional = 3
    }

    [ProtoContract]
    public enum PaintRuleGridSize {
        [ProtoEnum]
        Large = 0,
        [ProtoEnum]
        Small = 1
    }
}
