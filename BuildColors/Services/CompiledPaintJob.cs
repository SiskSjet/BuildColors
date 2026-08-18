using Sandbox.Definitions;
using Sisk.BuildColors.Settings.Models;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace Sisk.BuildColors.Services {

    /// <summary>
    /// A paint job turned into the form it is actually run in.
    /// </summary>
    internal sealed class CompiledPaintJob {
        private readonly List<CompiledRule> _rules = new List<CompiledRule>();

        private CompiledPaintJob() { }

        /// <summary>
        /// True when no rule of the job can ever match, which lets the caller skip the grid walk entirely.
        /// </summary>
        public bool IsEmpty {
            get { return _rules.Count == 0; }
        }

        public static CompiledPaintJob Compile(PaintJob job) {
            var compiled = new CompiledPaintJob();

            if (job == null || job.Rules == null) {
                return compiled;
            }

            foreach (var rule in job.Rules) {
                if (rule == null || rule.Action == null || !rule.Action.ApplyColor && !rule.Action.ApplySkin) {
                    continue;
                }

                var group = CompiledConditionGroup.Compile(rule.ConditionGroup);
                if (group == null) {
                    continue;
                }

                compiled._rules.Add(new CompiledRule(group, rule.Action));
            }

            return compiled;
        }

        /// <summary>
        /// Index of the first rule matching the block, or -1 when none does.
        /// </summary>
        public int FindRule(IMySlimBlock block, ref GridPaintContext grid, out BlockFacts facts) {
            facts = BlockFacts.Read(block, ref grid);

            for (var i = 0; i < _rules.Count; i++) {
                if (_rules[i].Group.Matches(ref facts)) {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Opens the measuring pass over a grid.
        /// </summary>
        public void BeginGrid() {
            for (var i = 0; i < _rules.Count; i++) {
                _rules[i].Source.BeginGrid();
            }
        }

        public void Observe(int ruleIndex, ref BlockFacts facts) {
            _rules[ruleIndex].Source.Observe(ref facts);
        }

        public void EndGrid() {
            for (var i = 0; i < _rules.Count; i++) {
                _rules[i].Source.EndGrid();
            }
        }

        /// <summary>
        /// Works out what a rule paints a block with.
        /// </summary>
        public void Resolve(int ruleIndex, ref BlockFacts facts, out PaintResolution resolution) {
            var rule = _rules[ruleIndex];

            PaintEntry entry;
            rule.Source.Evaluate(ref facts, out entry);

            resolution = new PaintResolution {
                ApplyColor = rule.ApplyColor,
                ApplySkin = rule.ApplySkin,
                Mask = entry.Mask,
                SkinId = entry.SkinId ?? string.Empty
            };
        }

        private sealed class CompiledRule {
            public CompiledRule(CompiledConditionGroup group, PaintRuleAction action) {
                Group = group;
                ApplyColor = action.ApplyColor;
                ApplySkin = action.ApplySkin;
                Source = CompiledPaintSource.Compile(action);
            }

            public bool ApplyColor { get; private set; }

            public bool ApplySkin { get; private set; }

            public CompiledConditionGroup Group { get; private set; }

            public CompiledPaintSource Source { get; private set; }
        }
    }

    /// <summary>
    /// What a matched block should end up as.
    /// </summary>
    internal struct PaintResolution {
        public bool ApplyColor;
        public bool ApplySkin;
        public Vector3 Mask;
        public string SkinId;

        /// <summary>
        /// True when two blocks can be painted by the same pair of calls.
        /// </summary>
        public bool Matches(ref PaintResolution other) {
            return ApplyColor == other.ApplyColor
                && ApplySkin == other.ApplySkin
                && Mask.Equals(other.Mask)
                && string.Equals(SkinId, other.SkinId, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Everything a condition may look at, read once per block.
    /// </summary>
    internal struct BlockFacts {
        public float BuildRatio;
        public Vector3 ColorMask;
        public MyCubeBlockDefinition Definition;
        public MyDefinitionId DefinitionId;
        public MyCubeSize GridSize;
        public bool HasDamage;
        public float IntegrityRatio;
        public MyStringHash SkinId;

        /// <summary>
        /// Center of the block in grid coordinates.
        /// </summary>
        public Vector3 LocalPosition;

        /// <summary>
        /// Where the block sits between the two ends of the grid on each axis, from 0 to 1.
        /// </summary>
        public Vector3 NormalizedPosition;

        /// <summary>
        /// The component of NormalizedPosition belonging to the longest axis of the grid.
        /// </summary>
        public float NormalizedLongest;

        /// <summary>
        /// Whole block coordinate along the longest axis of the grid, for patterns counting in blocks.
        /// </summary>
        public int LongestPosition;

        public float NormalizedRadial;
        public float NormalizedUp;

        /// <summary>
        /// Lowest cell of the block, which is also the coordinate the game addresses it by.
        /// </summary>
        public Vector3I Position;

        /// <summary>
        /// Distance from the grid center in whole blocks, along the up axis and outwards.
        /// </summary>
        public float RadialCoordinate;

        public float UpCoordinate;

        public static BlockFacts Read(IMySlimBlock block, ref GridPaintContext grid) {
            var definition = block.BlockDefinition;
            var maxIntegrity = block.MaxIntegrity;

            var center = (new Vector3(block.Min) + new Vector3(block.Max)) * .5f;
            var offset = center - grid.Center;
            var upCoordinate = Vector3.Dot(offset, grid.UpAxis);
            var radialCoordinate = offset.Length();
            var normalized = Vector3.Clamp((center - grid.Min) / grid.Size, Vector3.Zero, Vector3.One);

            return new BlockFacts {
                BuildRatio = block.BuildLevelRatio,
                ColorMask = block.GetColorMask(),
                Definition = definition as MyCubeBlockDefinition,
                DefinitionId = definition != null ? definition.Id : default(MyDefinitionId),
                GridSize = block.CubeGrid != null ? block.CubeGrid.GridSizeEnum : MyCubeSize.Large,
                HasDamage = block.CurrentDamage > 0f,
                IntegrityRatio = maxIntegrity > 0f ? block.Integrity / maxIntegrity : 1f,
                SkinId = block.SkinSubtypeId,
                Position = block.Min,
                LocalPosition = center,
                NormalizedPosition = normalized,
                NormalizedLongest = grid.LongestAxis == 0 ? normalized.X : grid.LongestAxis == 1 ? normalized.Y : normalized.Z,
                LongestPosition = grid.LongestAxis == 0 ? block.Min.X : grid.LongestAxis == 1 ? block.Min.Y : block.Min.Z,
                UpCoordinate = upCoordinate,
                NormalizedUp = MathHelper.Clamp(.5f + upCoordinate / (grid.UpExtent * 2f), 0f, 1f),
                RadialCoordinate = radialCoordinate,
                NormalizedRadial = MathHelper.Clamp(radialCoordinate / grid.Radius, 0f, 1f)
            };
        }
    }

    internal interface IBlockCondition {
        bool Matches(ref BlockFacts facts);
    }

    /// <summary>
    /// A condition tree node.
    /// </summary>
    internal sealed class CompiledConditionGroup {
        private readonly CompiledConditionGroup[] _children;
        private readonly IBlockCondition[] _conditions;
        private readonly bool _negate;
        private readonly bool _requireAll;

        private CompiledConditionGroup(bool requireAll, bool negate, IBlockCondition[] conditions, CompiledConditionGroup[] children) {
            _requireAll = requireAll;
            _negate = negate;
            _conditions = conditions;
            _children = children;
        }

        /// <summary>
        /// Compiles a group, returning null when it holds nothing that could match.
        /// </summary>
        public static CompiledConditionGroup Compile(PaintRuleConditionGroup group) {
            if (group == null) {
                return null;
            }

            var conditions = new List<IBlockCondition>();
            var children = new List<CompiledConditionGroup>();

            if (group.Conditions != null) {
                foreach (var condition in group.Conditions) {
                    var compiled = CompileCondition(condition);
                    if (compiled != null) {
                        conditions.Add(compiled);
                    }
                }
            }

            if (group.Children != null) {
                foreach (var child in group.Children) {
                    var compiled = Compile(child);
                    if (compiled != null) {
                        children.Add(compiled);
                    }
                }
            }

            if (conditions.Count == 0 && children.Count == 0) {
                return null;
            }

            return new CompiledConditionGroup(
                group.Operator == PaintRuleLogicalOperator.And,
                group.Negate,
                conditions.ToArray(),
                children.ToArray());
        }

        public bool Matches(ref BlockFacts facts) {
            var matched = Evaluate(ref facts);

            return _negate ? !matched : matched;
        }

        /// <summary>
        /// Builds the test for a single condition, or null when the condition cannot match anything.
        /// </summary>
        private static IBlockCondition CompileCondition(PaintRuleCondition condition) {
            if (condition == null) {
                return null;
            }

            IBlockCondition compiled;

            switch (condition.Type) {
                case PaintRuleConditionType.BlockColor:
                    compiled = new ColorCondition(condition.Color);
                    break;

                case PaintRuleConditionType.BlockDefinition:
                    if (string.IsNullOrWhiteSpace(condition.Definition.TypeId) && string.IsNullOrWhiteSpace(condition.Definition.SubtypeId)) {
                        return null;
                    }

                    compiled = new DefinitionCondition(condition.Definition);
                    break;

                case PaintRuleConditionType.BlockSkin:
                    compiled = new SkinCondition(condition.SkinId);
                    break;

                case PaintRuleConditionType.BlockCategory:
                    compiled = new CategoryCondition(condition.Category);
                    break;

                case PaintRuleConditionType.GridSize:
                    compiled = new GridSizeCondition(condition.GridSize);
                    break;

                case PaintRuleConditionType.BlockIntegrity:
                    compiled = new IntegrityCondition(condition.Integrity, condition.IntegrityThreshold);
                    break;

                case PaintRuleConditionType.AnyBlock:
                    compiled = AnyBlockCondition.Instance;
                    break;

                default:
                    return null;
            }

            return condition.Comparison == PaintRuleComparison.NotEquals ? new NegatedCondition(compiled) : compiled;
        }

        private bool Evaluate(ref BlockFacts facts) {
            for (var i = 0; i < _conditions.Length; i++) {
                var matched = _conditions[i].Matches(ref facts);

                if (matched != _requireAll) {
                    return matched;
                }
            }

            for (var i = 0; i < _children.Length; i++) {
                var matched = _children[i].Matches(ref facts);

                if (matched != _requireAll) {
                    return matched;
                }
            }

            return _requireAll;
        }
    }

    internal sealed class NegatedCondition : IBlockCondition {
        private readonly IBlockCondition _condition;

        public NegatedCondition(IBlockCondition condition) {
            _condition = condition;
        }

        public bool Matches(ref BlockFacts facts) {
            return !_condition.Matches(ref facts);
        }
    }

    internal sealed class AnyBlockCondition : IBlockCondition {
        public static readonly AnyBlockCondition Instance = new AnyBlockCondition();

        private AnyBlockCondition() { }

        public bool Matches(ref BlockFacts facts) {
            return true;
        }
    }

    internal sealed class ColorCondition : IBlockCondition {
        private readonly Vector3 _mask;

        public ColorCondition(ColorMask mask) {
            _mask = mask;
        }

        public bool Matches(ref BlockFacts facts) {
            return PaintColorMath.MaskEquals(facts.ColorMask, _mask);
        }
    }

    internal sealed class DefinitionCondition : IBlockCondition {
        private readonly Dictionary<MyDefinitionId, bool> _resultsByDefinition = new Dictionary<MyDefinitionId, bool>(MyDefinitionId.Comparer);

        private readonly string _subtypePattern;
        private readonly string _typePattern;

        public DefinitionCondition(PaintRuleDefinitionValue value) {
            _typePattern = value.TypeId ?? string.Empty;
            _subtypePattern = value.SubtypeId ?? string.Empty;
        }

        public bool Matches(ref BlockFacts facts) {
            bool result;
            if (_resultsByDefinition.TryGetValue(facts.DefinitionId, out result)) {
                return result;
            }

            result = WildcardPattern.Matches(_typePattern, facts.DefinitionId.TypeId.ToString())
                && WildcardPattern.Matches(_subtypePattern, facts.DefinitionId.SubtypeName);

            _resultsByDefinition[facts.DefinitionId] = result;

            return result;
        }
    }

    internal sealed class SkinCondition : IBlockCondition {
        private readonly MyStringHash _skin;
        private readonly string _skinText;

        public SkinCondition(string skinId) {
            _skinText = skinId ?? string.Empty;
            _skin = MyStringHash.GetOrCompute(_skinText);
        }

        public bool Matches(ref BlockFacts facts) {
            if (facts.SkinId == _skin) {
                return true;
            }

            return string.Equals(facts.SkinId.String ?? string.Empty, _skinText, StringComparison.OrdinalIgnoreCase);
        }
    }

    internal sealed class CategoryCondition : IBlockCondition {
        private static readonly MyObjectBuilderType ArmorTypeId = typeof(MyObjectBuilder_CubeBlock);

        private readonly PaintRuleBlockCategory _category;

        public CategoryCondition(PaintRuleBlockCategory category) {
            _category = category;
        }

        public bool Matches(ref BlockFacts facts) {
            var isArmor = facts.DefinitionId.TypeId == ArmorTypeId;

            switch (_category) {
                case PaintRuleBlockCategory.Armor:
                    return isArmor;

                case PaintRuleBlockCategory.LightArmor:
                    return isArmor && !IsHeavy(facts.Definition);

                case PaintRuleBlockCategory.HeavyArmor:
                    return isArmor && IsHeavy(facts.Definition);

                case PaintRuleBlockCategory.Functional:
                    return !isArmor;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Armor weight, taken from the edge type the game picks the edge model by.
        /// </summary>
        private static bool IsHeavy(MyCubeBlockDefinition definition) {
            return definition != null && string.Equals(definition.EdgeType, "Heavy", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Tests how far a block is from being whole.
    /// </summary>
    internal sealed class IntegrityCondition : IBlockCondition {
        private readonly PaintRuleIntegrityState _state;
        private readonly float _threshold;

        public IntegrityCondition(PaintRuleIntegrityState state, float thresholdPercent) {
            _state = state;
            _threshold = MathHelper.Clamp(thresholdPercent, 0f, 100f) * .01f;
        }

        public bool Matches(ref BlockFacts facts) {
            switch (_state) {
                case PaintRuleIntegrityState.Intact:
                    return facts.BuildRatio >= 1f && !facts.HasDamage;

                case PaintRuleIntegrityState.Damaged:
                    return facts.HasDamage;

                case PaintRuleIntegrityState.Incomplete:
                    return facts.BuildRatio < 1f;

                case PaintRuleIntegrityState.BelowThreshold:
                    return facts.IntegrityRatio < _threshold;

                default:
                    return false;
            }
        }
    }

    internal sealed class GridSizeCondition : IBlockCondition {
        private readonly MyCubeSize _size;

        public GridSizeCondition(PaintRuleGridSize size) {
            _size = size == PaintRuleGridSize.Small ? MyCubeSize.Small : MyCubeSize.Large;
        }

        public bool Matches(ref BlockFacts facts) {
            return facts.GridSize == _size;
        }
    }
}
