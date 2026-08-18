using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings.Models;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using Sisk.Utils.Localization.Extensions;

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

        private static string DescribeColor(ColorMask mask) {
            var hsv = (SeHsv)mask;

            return string.Format("({0:0}°, {1:0}%, {2:0}%)", hsv.H, hsv.S, hsv.V);
        }
    }
}
