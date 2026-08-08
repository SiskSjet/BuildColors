using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using Sisk.Utils.Localization.Extensions;
using System.Linq;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Human readable descriptions of paint rule conditions, shared by the dialogs that list them.
    /// </summary>
    internal static class PaintRuleConditionText {

        public static string Describe(PaintRuleCondition condition) {
            if (condition == null) {
                return ModText.BC_UI_Desc_Unconfigured.GetString();
            }

            var comparison = condition.Comparison == PaintRuleComparison.NotEquals ? ModText.BC_UI_Desc_IsNot.GetString() : ModText.BC_UI_Desc_Is.GetString();

            switch (condition.Type) {
                case PaintRuleConditionType.BlockColor:
                    return ModText.BC_UI_Desc_Color.GetString(comparison, DescribeColor(condition.Color));

                case PaintRuleConditionType.BlockDefinition:
                    var typeId = string.IsNullOrWhiteSpace(condition.Definition.TypeId) ? ModText.BC_UI_Desc_Any.GetString() : condition.Definition.TypeId;
                    var subtypeId = string.IsNullOrWhiteSpace(condition.Definition.SubtypeId) ? ModText.BC_UI_Desc_Any.GetString() : condition.Definition.SubtypeId;
                    return ModText.BC_UI_Desc_BlockDefinition.GetString(comparison, typeId, subtypeId);

                case PaintRuleConditionType.BlockSkin:
                    var skinId = string.IsNullOrWhiteSpace(condition.SkinId) ? ModText.BC_UI_Desc_NoSkin.GetString() : condition.SkinId;
                    return ModText.BC_UI_Desc_Skin.GetString(comparison, skinId);

                case PaintRuleConditionType.BlockCategory:
                    return ModText.BC_UI_Desc_BlockCategory.GetString(comparison, DescribeCategory(condition.Category));

                case PaintRuleConditionType.GridSize:
                    return ModText.BC_UI_Desc_GridSize.GetString(comparison, condition.GridSize == PaintRuleGridSize.Small ? ModText.BC_UI_Desc_GridSmall.GetString() : ModText.BC_UI_Desc_GridLarge.GetString());

                case PaintRuleConditionType.BlockIntegrity:
                    return condition.Integrity == PaintRuleIntegrityState.BelowThreshold
                        ? ModText.BC_UI_Desc_IntegrityThreshold.GetString(comparison, condition.IntegrityThreshold)
                        : ModText.BC_UI_Desc_BlockState.GetString(comparison, DescribeIntegrity(condition.Integrity));

                case PaintRuleConditionType.AnyBlock:
                    return ModText.BC_UI_Desc_AnyBlock.GetString();

                default:
                    return ModText.BC_UI_Desc_Condition.GetString();
            }
        }

        /// <summary>
        /// Short summary of a whole group, used where the tree itself is not shown.
        /// </summary>
        public static string DescribeGroup(PaintRuleConditionGroup group) {
            if (group == null) {
                return ModText.BC_UI_Desc_NoConditions.GetString();
            }

            var conditionCount = CountConditions(group);
            var groupCount = CountGroups(group);

            if (conditionCount == 0) {
                return ModText.BC_UI_Desc_NoConditionsNeverMatches.GetString();
            }

            var summary = ModText.BC_UI_Desc_GroupSummary.GetString(conditionCount, DescribeOperator(group));

            if (groupCount > 0) {
                summary += ModText.BC_UI_Desc_GroupSummaryNested.GetString(groupCount);
            }

            return summary;
        }

        /// <summary>
        /// How a group combines its members, including the inversion if it is set.
        /// </summary>
        public static string DescribeOperator(PaintRuleConditionGroup group) {
            if (group == null) {
                return ModText.BC_UI_Desc_AllOf.GetString();
            }

            var operatorText = group.Operator == PaintRuleLogicalOperator.And ? ModText.BC_UI_Desc_AllOf.GetString() : ModText.BC_UI_Desc_AnyOf.GetString();

            return group.Negate ? ModText.BC_UI_Desc_Not.GetString(operatorText) : operatorText;
        }

        private static string DescribeCategory(PaintRuleBlockCategory category) {
            switch (category) {
                case PaintRuleBlockCategory.LightArmor:
                    return ModText.BC_UI_Desc_LightArmor.GetString();
                case PaintRuleBlockCategory.HeavyArmor:
                    return ModText.BC_UI_Desc_HeavyArmor.GetString();
                case PaintRuleBlockCategory.Functional:
                    return ModText.BC_UI_Desc_Functional.GetString();
                default:
                    return ModText.BC_UI_Desc_Armor.GetString();
            }
        }

        private static string DescribeIntegrity(PaintRuleIntegrityState state) {
            switch (state) {
                case PaintRuleIntegrityState.Damaged:
                    return ModText.BC_UI_Desc_Damaged.GetString();
                case PaintRuleIntegrityState.Incomplete:
                    return ModText.BC_UI_Desc_Underconstruction.GetString();
                default:
                    return ModText.BC_UI_Desc_Intact.GetString();
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
