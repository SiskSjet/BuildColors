using Sisk.BuildColors.Settings.Models.PaintJobs;
using System.Linq;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Human readable descriptions of paint rule conditions, shared by the dialogs that list them.
    /// </summary>
    internal static class PaintRuleConditionText {

        public static string Describe(PaintRuleCondition condition) {
            if (condition == null) {
                return "Unconfigured";
            }

            var comparison = condition.Comparison == PaintRuleComparison.NotEquals ? "is not" : "is";

            switch (condition.Type) {
                case PaintRuleConditionType.BlockColor:
                    return string.Format("Color {0} {1}", comparison, DescribeColor(condition.Color));

                case PaintRuleConditionType.BlockDefinition:
                    var typeId = string.IsNullOrWhiteSpace(condition.Definition.TypeId) ? "Any" : condition.Definition.TypeId;
                    var subtypeId = string.IsNullOrWhiteSpace(condition.Definition.SubtypeId) ? "Any" : condition.Definition.SubtypeId;
                    return string.Format("Block {0} {1}/{2}", comparison, typeId, subtypeId);

                case PaintRuleConditionType.BlockSkin:
                    var skinId = string.IsNullOrWhiteSpace(condition.SkinId) ? "No skin" : condition.SkinId;
                    return string.Format("Skin {0} {1}", comparison, skinId);

                case PaintRuleConditionType.BlockCategory:
                    return string.Format("Block {0} {1}", comparison, DescribeCategory(condition.Category));

                case PaintRuleConditionType.GridSize:
                    return string.Format("Grid {0} {1}", comparison,
                        condition.GridSize == PaintRuleGridSize.Small ? "small" : "large");

                case PaintRuleConditionType.BlockIntegrity:
                    return condition.Integrity == PaintRuleIntegrityState.BelowThreshold
                        ? string.Format("Integrity {0} below {1:0.##}%", condition.Comparison == PaintRuleComparison.NotEquals ? "is not" : "is", condition.IntegrityThreshold)
                        : string.Format("Block {0} {1}", comparison, DescribeIntegrity(condition.Integrity));

                case PaintRuleConditionType.AnyBlock:
                    return "Any block";

                default:
                    return "Condition";
            }
        }

        /// <summary>
        /// Short summary of a whole group, used where the tree itself is not shown.
        /// </summary>
        public static string DescribeGroup(PaintRuleConditionGroup group) {
            if (group == null) {
                return "No conditions";
            }

            var conditionCount = CountConditions(group);
            var groupCount = CountGroups(group);

            if (conditionCount == 0) {
                return "No conditions - this rule never matches";
            }

            var summary = string.Format("{0} condition(s), match {1}", conditionCount, DescribeOperator(group));

            if (groupCount > 0) {
                summary += string.Format(", {0} nested group(s)", groupCount);
            }

            return summary;
        }

        /// <summary>
        /// How a group combines its members, including the inversion if it is set.
        /// </summary>
        public static string DescribeOperator(PaintRuleConditionGroup group) {
            if (group == null) {
                return "ALL of";
            }

            var operatorText = group.Operator == PaintRuleLogicalOperator.And ? "ALL of" : "ANY of";

            return group.Negate ? "NOT " + operatorText : operatorText;
        }

        private static string DescribeCategory(PaintRuleBlockCategory category) {
            switch (category) {
                case PaintRuleBlockCategory.LightArmor:
                    return "light armor";
                case PaintRuleBlockCategory.HeavyArmor:
                    return "heavy armor";
                case PaintRuleBlockCategory.Functional:
                    return "a functional block";
                default:
                    return "armor";
            }
        }

        private static string DescribeIntegrity(PaintRuleIntegrityState state) {
            switch (state) {
                case PaintRuleIntegrityState.Damaged:
                    return "damaged";
                case PaintRuleIntegrityState.Incomplete:
                    return "under construction";
                default:
                    return "intact";
            }
        }

        private static int CountConditions(PaintRuleConditionGroup group) {
            if (group == null) {
                return 0;
            }

            var count = group.Conditions?.Count ?? 0;

            if (group.Children != null) {
                count += group.Children.Sum(CountConditions);
            }

            return count;
        }

        private static int CountGroups(PaintRuleConditionGroup group) {
            if (group?.Children == null) {
                return 0;
            }

            return group.Children.Count + group.Children.Sum(CountGroups);
        }

        private static string DescribeColor(ColorModel color) {
            return string.Format("({0}, {1}, {2})", color.R, color.G, color.B);
        }
    }
}
