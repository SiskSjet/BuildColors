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
                    if (!condition.Color.Enabled) {
                        return "Color (not configured)";
                    }

                    return string.Format("Color {0} {1}", comparison, DescribeColor(condition.Color.Value));

                case PaintRuleConditionType.BlockDefinition:
                    if (!condition.Definition.Enabled) {
                        return "Block (not configured)";
                    }

                    var typeId = string.IsNullOrWhiteSpace(condition.Definition.TypeId) ? "Any" : condition.Definition.TypeId;
                    var subtypeId = string.IsNullOrWhiteSpace(condition.Definition.SubtypeId) ? "Any" : condition.Definition.SubtypeId;
                    return string.Format("Block {0} {1}/{2}", comparison, typeId, subtypeId);

                case PaintRuleConditionType.BlockSkin:
                    if (!condition.Skin.Enabled) {
                        return "Skin (not configured)";
                    }

                    var skinId = string.IsNullOrWhiteSpace(condition.Skin.SkinId) ? "No skin" : condition.Skin.SkinId;
                    return string.Format("Skin {0} {1}", comparison, skinId);

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

            var operatorText = group.Operator == PaintRuleLogicalOperator.And ? "ALL" : "ANY";
            var summary = string.Format("{0} condition(s), match {1}", conditionCount, operatorText);

            if (groupCount > 0) {
                summary += string.Format(", {0} nested group(s)", groupCount);
            }

            return summary;
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
