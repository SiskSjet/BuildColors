using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using Sisk.BuildColors.UI;
using Sisk.Utils.Localization.Extensions;
using System;
using System.Collections.Generic;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.Services {

    /// <summary>
    ///     Reads the <c>field=value</c> pairs the paint job commands take and writes them onto a condition or
    ///     a rule action. A condition is fully described by its type, so a new condition takes its type from
    ///     the first value field it is given unless the type is stated outright.
    /// </summary>
    internal static class PaintRuleFields {

        public static bool TryApplyToCondition(PaintRuleCondition condition, IList<string> assignments, bool isNew, out string error) {
            error = null;

            var typeGiven = false;
            var integrityGiven = false;
            var thresholdGiven = false;
            var valueFieldGiven = false;

            foreach (var token in assignments) {
                string field;
                string value;

                if (!CommandArguments.TrySplitAssignment(token, out field, out value)) {
                    error = ModText.BC_Cmd_UnknownField.GetString(token);
                    return false;
                }

                switch (field) {
                    case "type":
                        PaintRuleConditionType conditionType;
                        if (!TryParseConditionType(value, out conditionType)) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        condition.Type = conditionType;
                        typeGiven = true;
                        break;

                    case "is":
                    case "cmp":
                    case "comparison":
                        PaintRuleComparison comparison;
                        if (!TryParseComparison(value, out comparison)) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        condition.Comparison = comparison;
                        break;

                    case "color":
                        ColorModel color;
                        if (!CommandArguments.TryParseColor(value, out color)) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        condition.Color = color;
                        valueFieldGiven = ApplyInferredType(condition, PaintRuleConditionType.BlockColor, isNew, typeGiven, valueFieldGiven);
                        break;

                    case "def":
                    case "definition":
                        condition.Definition = ParseDefinition(value);
                        valueFieldGiven = ApplyInferredType(condition, PaintRuleConditionType.BlockDefinition, isNew, typeGiven, valueFieldGiven);
                        break;

                    case "deftype":
                        var definitionType = condition.Definition;
                        definitionType.TypeId = value;
                        condition.Definition = definitionType;
                        valueFieldGiven = ApplyInferredType(condition, PaintRuleConditionType.BlockDefinition, isNew, typeGiven, valueFieldGiven);
                        break;

                    case "subtype":
                        var definitionSubtype = condition.Definition;
                        definitionSubtype.SubtypeId = value;
                        condition.Definition = definitionSubtype;
                        valueFieldGiven = ApplyInferredType(condition, PaintRuleConditionType.BlockDefinition, isNew, typeGiven, valueFieldGiven);
                        break;

                    case "skin":
                        string skinId;
                        if (!TryResolveSkin(value, out skinId)) {
                            error = ModText.BC_Cmd_UnknownSkin.GetString(value, Mod.Acronym);
                            return false;
                        }

                        condition.SkinId = skinId;
                        valueFieldGiven = ApplyInferredType(condition, PaintRuleConditionType.BlockSkin, isNew, typeGiven, valueFieldGiven);
                        break;

                    case "cat":
                    case "category":
                        PaintRuleBlockCategory category;
                        if (!TryParseCategory(value, out category)) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        condition.Category = category;
                        valueFieldGiven = ApplyInferredType(condition, PaintRuleConditionType.BlockCategory, isNew, typeGiven, valueFieldGiven);
                        break;

                    case "size":
                    case "gridsize":
                        PaintRuleGridSize gridSize;
                        if (!TryParseGridSize(value, out gridSize)) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        condition.GridSize = gridSize;
                        valueFieldGiven = ApplyInferredType(condition, PaintRuleConditionType.GridSize, isNew, typeGiven, valueFieldGiven);
                        break;

                    case "state":
                    case "integrity":
                        PaintRuleIntegrityState integrity;
                        if (!TryParseIntegrity(value, out integrity)) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        condition.Integrity = integrity;
                        integrityGiven = true;
                        valueFieldGiven = ApplyInferredType(condition, PaintRuleConditionType.BlockIntegrity, isNew, typeGiven, valueFieldGiven);
                        break;

                    case "threshold":
                        float threshold;
                        if (!CommandArguments.TryParsePercentage(value, out threshold) || threshold < 0f || threshold > 100f) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        condition.IntegrityThreshold = threshold;
                        thresholdGiven = true;
                        valueFieldGiven = ApplyInferredType(condition, PaintRuleConditionType.BlockIntegrity, isNew, typeGiven, valueFieldGiven);
                        break;

                    default:
                        error = ModText.BC_Cmd_UnknownField.GetString(field);
                        return false;
                }
            }

            // A threshold on its own only means something together with the state it belongs to.
            if (thresholdGiven && !integrityGiven && condition.Type == PaintRuleConditionType.BlockIntegrity && isNew) {
                condition.Integrity = PaintRuleIntegrityState.BelowThreshold;
            }

            return true;
        }

        public static bool TryApplyToAction(PaintRuleAction action, IList<string> assignments, out string error) {
            error = null;

            var applyColorGiven = false;
            var applySkinGiven = false;
            var colorGiven = false;
            var skinGiven = false;

            foreach (var token in assignments) {
                string field;
                string value;

                if (!CommandArguments.TrySplitAssignment(token, out field, out value)) {
                    error = ModText.BC_Cmd_UnknownField.GetString(token);
                    return false;
                }

                switch (field) {
                    case "color":
                        ColorModel color;
                        if (!CommandArguments.TryParseColor(value, out color)) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        action.TargetColor = color;
                        colorGiven = true;
                        break;

                    case "applycolor":
                        bool applyColor;
                        if (!CommandArguments.TryParseFlag(value, action.ApplyColor, out applyColor)) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        action.ApplyColor = applyColor;
                        applyColorGiven = true;
                        break;

                    case "skin":
                        string skinId;
                        if (!TryResolveSkin(value, out skinId)) {
                            error = ModText.BC_Cmd_UnknownSkin.GetString(value, Mod.Acronym);
                            return false;
                        }

                        action.TargetSkinId = skinId;
                        skinGiven = true;
                        break;

                    case "applyskin":
                        bool applySkin;
                        if (!CommandArguments.TryParseFlag(value, action.ApplySkin, out applySkin)) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        action.ApplySkin = applySkin;
                        applySkinGiven = true;
                        break;

                    default:
                        error = ModText.BC_Cmd_UnknownField.GetString(field);
                        return false;
                }
            }

            // Naming a color or a skin without saying whether it is applied is only ever meant one way.
            if (colorGiven && !applyColorGiven) {
                action.ApplyColor = true;
            }

            if (skinGiven && !applySkinGiven) {
                action.ApplySkin = true;
            }

            return true;
        }

        /// <summary>
        ///     Reads a skin id, accepting both the id itself and the display name shown in the skin list. An
        ///     empty value is the default armor look, which is a valid target.
        /// </summary>
        public static bool TryResolveSkin(string value, out string skinId) {
            skinId = string.Empty;

            if (string.IsNullOrWhiteSpace(value) || string.Equals(value, "none", StringComparison.InvariantCultureIgnoreCase)) {
                return true;
            }

            var text = value.Trim();

            foreach (var skin in DefinitionCatalog.Skins) {
                if (string.Equals(skin.SkinId, text, StringComparison.InvariantCultureIgnoreCase)
                    || string.Equals(skin.DisplayName, text, StringComparison.InvariantCultureIgnoreCase)) {
                    skinId = skin.SkinId;
                    return true;
                }
            }

            return false;
        }

        public static bool TryParseOperator(string value, out PaintRuleLogicalOperator result) {
            result = PaintRuleLogicalOperator.And;

            if (string.IsNullOrWhiteSpace(value)) {
                return false;
            }

            switch (value.Trim().ToLowerInvariant()) {
                case "and":
                case "all":
                    result = PaintRuleLogicalOperator.And;
                    return true;

                case "or":
                case "any":
                    result = PaintRuleLogicalOperator.Or;
                    return true;

                default:
                    return false;
            }
        }

        private static bool TryParseConditionType(string value, out PaintRuleConditionType result) {
            result = PaintRuleConditionType.BlockColor;

            if (string.IsNullOrWhiteSpace(value)) {
                return false;
            }

            switch (value.Trim().ToLowerInvariant()) {
                case "color":
                case "blockcolor":
                    result = PaintRuleConditionType.BlockColor;
                    return true;

                case "def":
                case "definition":
                case "blockdefinition":
                    result = PaintRuleConditionType.BlockDefinition;
                    return true;

                case "skin":
                case "blockskin":
                    result = PaintRuleConditionType.BlockSkin;
                    return true;

                case "cat":
                case "category":
                case "blockcategory":
                    result = PaintRuleConditionType.BlockCategory;
                    return true;

                case "size":
                case "gridsize":
                    result = PaintRuleConditionType.GridSize;
                    return true;

                case "any":
                case "anyblock":
                    result = PaintRuleConditionType.AnyBlock;
                    return true;

                case "state":
                case "integrity":
                case "blockintegrity":
                    result = PaintRuleConditionType.BlockIntegrity;
                    return true;

                default:
                    return false;
            }
        }

        private static bool TryParseComparison(string value, out PaintRuleComparison result) {
            result = PaintRuleComparison.Equals;

            if (string.IsNullOrWhiteSpace(value)) {
                return false;
            }

            switch (value.Trim().ToLowerInvariant()) {
                case "is":
                case "equals":
                case "matches":
                    result = PaintRuleComparison.Equals;
                    return true;

                case "isnot":
                case "not":
                case "notequals":
                    result = PaintRuleComparison.NotEquals;
                    return true;

                default:
                    return false;
            }
        }

        private static bool TryParseCategory(string value, out PaintRuleBlockCategory result) {
            result = PaintRuleBlockCategory.Armor;

            if (string.IsNullOrWhiteSpace(value)) {
                return false;
            }

            switch (value.Trim().ToLowerInvariant()) {
                case "armor":
                    result = PaintRuleBlockCategory.Armor;
                    return true;

                case "light":
                case "lightarmor":
                    result = PaintRuleBlockCategory.LightArmor;
                    return true;

                case "heavy":
                case "heavyarmor":
                    result = PaintRuleBlockCategory.HeavyArmor;
                    return true;

                case "functional":
                case "block":
                    result = PaintRuleBlockCategory.Functional;
                    return true;

                default:
                    return false;
            }
        }

        private static bool TryParseGridSize(string value, out PaintRuleGridSize result) {
            result = PaintRuleGridSize.Large;

            if (string.IsNullOrWhiteSpace(value)) {
                return false;
            }

            switch (value.Trim().ToLowerInvariant()) {
                case "large":
                case "big":
                    result = PaintRuleGridSize.Large;
                    return true;

                case "small":
                    result = PaintRuleGridSize.Small;
                    return true;

                default:
                    return false;
            }
        }

        private static bool TryParseIntegrity(string value, out PaintRuleIntegrityState result) {
            result = PaintRuleIntegrityState.Damaged;

            if (string.IsNullOrWhiteSpace(value)) {
                return false;
            }

            switch (value.Trim().ToLowerInvariant()) {
                case "intact":
                    result = PaintRuleIntegrityState.Intact;
                    return true;

                case "damaged":
                    result = PaintRuleIntegrityState.Damaged;
                    return true;

                case "incomplete":
                case "underconstruction":
                    result = PaintRuleIntegrityState.Incomplete;
                    return true;

                case "below":
                case "belowthreshold":
                    result = PaintRuleIntegrityState.BelowThreshold;
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Reads <c>TypeId/SubtypeId</c>. Without a slash the value is a subtype pattern, which is the
        ///     side that tells blocks apart in practice.
        /// </summary>
        private static PaintRuleDefinitionValue ParseDefinition(string value) {
            var definition = new PaintRuleDefinitionValue();

            if (string.IsNullOrWhiteSpace(value)) {
                return definition;
            }

            var text = value.Trim();
            var separator = text.IndexOf('/');

            if (separator < 0) {
                definition.SubtypeId = text;
                return definition;
            }

            definition.TypeId = text.Substring(0, separator).Trim();
            definition.SubtypeId = text.Substring(separator + 1).Trim();

            return definition;
        }

        /// <summary>
        ///     Sets the type a value field belongs to while a condition is being created. Later value fields
        ///     leave the type alone, so <c>color=… skin=…</c> does not silently end up as a skin condition.
        /// </summary>
        private static bool ApplyInferredType(PaintRuleCondition condition, PaintRuleConditionType type, bool isNew, bool typeGiven, bool valueFieldGiven) {
            if (isNew && !typeGiven && !valueFieldGiven) {
                condition.Type = type;
            }

            return true;
        }
    }
}
