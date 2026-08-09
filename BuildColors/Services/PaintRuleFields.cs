using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using Sisk.BuildColors.UI;
using Sisk.Utils.Localization.Extensions;
using System;
using System.Collections.Generic;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.Services {

    /// <summary>
    /// Reads the field=value pairs the paint job commands take.
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
            var entriesGiven = false;
            var entrySkinGiven = false;

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

                    case "source":
                        PaintSourceType sourceType;
                        if (!TryParseSourceType(value, out sourceType)) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        EnsureSource(action).Type = sourceType;
                        break;

                    case "axis":
                        PaintSourceAxis axis;
                        if (!TryParseSourceAxis(value, out axis)) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        EnsureSource(action).Axis = axis;
                        break;

                    case "blend":
                        PaintBlendSpace blend;
                        if (!TryParseBlendSpace(value, out blend)) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        EnsureSource(action).Blend = blend;
                        break;

                    case "steps":
                        int steps;
                        if (!CommandArguments.TryParseInteger(value, out steps) || steps < 0 || steps > PaintColorSource.MAX_STEPS) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        EnsureSource(action).Steps = steps;
                        break;

                    case "scale":
                        float scale;
                        if (!CommandArguments.TryParseNumber(value, out scale) || scale < PaintColorSource.MIN_SCALE || scale > PaintColorSource.MAX_SCALE) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        EnsureSource(action).Scale = scale;
                        break;

                    case "period":
                        int period;
                        if (!CommandArguments.TryParseInteger(value, out period) || period < 1) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        EnsureSource(action).Period = period;
                        break;

                    case "shape":
                        PaintPatternShape shape;
                        if (!TryParsePatternShape(value, out shape)) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        EnsureSource(action).Shape = shape;
                        break;

                    case "seed":
                        int seed;
                        if (!CommandArguments.TryParseInteger(value, out seed)) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        EnsureSource(action).Seed = seed;
                        break;

                    case "fit":
                        PaintGradientFit fit;
                        if (!TryParseGradientFit(value, out fit)) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        EnsureSource(action).Fit = fit;
                        break;

                    case "reverse":
                        var source = EnsureSource(action);
                        bool reverse;
                        if (!CommandArguments.TryParseFlag(value, source.Reverse, out reverse)) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        source.Reverse = reverse;
                        break;

                    case "stop":
                    case "stops":
                        List<PaintColorStop> stops;
                        if (!TryParseStops(value, out stops)) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        var stopSource = EnsureSource(action);
                        stopSource.Stops = stops;
                        entriesGiven = true;
                        entrySkinGiven |= HasSkin(stops);

                        if (stopSource.Type == PaintSourceType.Solid) {
                            stopSource.Type = PaintSourceType.Gradient;
                        }

                        break;

                    case "palette":
                        List<PaintPaletteEntry> palette;
                        if (!TryParsePalette(value, out palette)) {
                            error = ModText.BC_Cmd_InvalidValue.GetString(value, field);
                            return false;
                        }

                        var paletteSource = EnsureSource(action);
                        paletteSource.Palette = palette;
                        entriesGiven = true;
                        entrySkinGiven |= HasSkin(palette);

                        if (paletteSource.Type == PaintSourceType.Solid || paletteSource.Type == PaintSourceType.Gradient) {
                            paletteSource.Type = PaintSourceType.Camo;
                        }

                        break;

                    default:
                        error = ModText.BC_Cmd_UnknownField.GetString(field);
                        return false;
                }
            }

            if ((colorGiven || entriesGiven) && !applyColorGiven) {
                action.ApplyColor = true;
            }

            if ((skinGiven || entrySkinGiven) && !applySkinGiven) {
                action.ApplySkin = true;
            }

            return true;
        }

        /// <summary>
        /// Reads a skin id, accepting both the id itself and the display name shown in the skin list.
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

        /// <summary>
        /// Returns the source of an action, creating a plain one on first use.
        /// </summary>
        private static PaintColorSource EnsureSource(PaintRuleAction action) {
            if (action.Source == null) {
                action.Source = new PaintColorSource();
            }

            return action.Source;
        }

        /// <summary>
        /// Reads a list of gradient stops written as color[@position][:skin] and separated by semicolons.
        /// </summary>
        private static bool TryParseStops(string value, out List<PaintColorStop> stops) {
            stops = new List<PaintColorStop>();

            if (string.IsNullOrWhiteSpace(value)) {
                return false;
            }

            var tokens = value.Split(';');
            var positions = new List<float>();

            foreach (var token in tokens) {
                if (string.IsNullOrWhiteSpace(token)) {
                    continue;
                }

                string colorText;
                string skinText;
                float position;
                var hasPosition = TrySplitEntry(token, '@', out colorText, out position, out skinText);

                ColorModel color;
                string skinId;
                if (!CommandArguments.TryParseColor(colorText, out color) || !TryResolveSkin(skinText, out skinId)) {
                    return false;
                }

                if (hasPosition && (position < 0f || position > 1f)) {
                    return false;
                }

                stops.Add(new PaintColorStop(color, position) { SkinId = skinId });
                positions.Add(hasPosition ? position : -1f);
            }

            if (stops.Count == 0) {
                return false;
            }

            for (var i = 0; i < stops.Count; i++) {
                if (positions[i] < 0f) {
                    stops[i].Position = stops.Count > 1 ? i / (float)(stops.Count - 1) : 0f;
                }
            }

            return true;
        }

        /// <summary>
        /// Reads a palette written as color[*weight][:skin] and separated by semicolons.
        /// </summary>
        private static bool TryParsePalette(string value, out List<PaintPaletteEntry> palette) {
            palette = new List<PaintPaletteEntry>();

            if (string.IsNullOrWhiteSpace(value)) {
                return false;
            }

            foreach (var token in value.Split(';')) {
                if (string.IsNullOrWhiteSpace(token)) {
                    continue;
                }

                string colorText;
                string skinText;
                float weight;
                var hasWeight = TrySplitEntry(token, '*', out colorText, out weight, out skinText);

                ColorModel color;
                string skinId;
                if (!CommandArguments.TryParseColor(colorText, out color) || !TryResolveSkin(skinText, out skinId)) {
                    return false;
                }

                if (hasWeight && weight < 0f) {
                    return false;
                }

                palette.Add(new PaintPaletteEntry(color) { SkinId = skinId, Weight = hasWeight ? weight : 1f });
            }

            return palette.Count > 0;
        }

        /// <summary>
        /// Peels the optional skin and the optional number off a list entry, leaving the color.
        /// </summary>
        private static bool TrySplitEntry(string token, char modifier, out string colorText, out float number, out string skinId) {
            var text = token.Trim();

            number = 0f;
            skinId = null;

            var skinSeparator = text.IndexOf(':');
            if (skinSeparator >= 0) {
                skinId = text.Substring(skinSeparator + 1).Trim();
                text = text.Substring(0, skinSeparator);
            }

            var numberSeparator = text.IndexOf(modifier);
            var hasNumber = false;

            if (numberSeparator >= 0) {
                hasNumber = CommandArguments.TryParseNumber(text.Substring(numberSeparator + 1), out number);
                text = text.Substring(0, numberSeparator);
            }

            colorText = text.Trim();

            return hasNumber;
        }

        private static bool HasSkin(List<PaintColorStop> stops) {
            foreach (var stop in stops) {
                if (!string.IsNullOrWhiteSpace(stop.SkinId)) {
                    return true;
                }
            }

            return false;
        }

        private static bool HasSkin(List<PaintPaletteEntry> palette) {
            foreach (var entry in palette) {
                if (!string.IsNullOrWhiteSpace(entry.SkinId)) {
                    return true;
                }
            }

            return false;
        }

        private static bool TryParseSourceType(string value, out PaintSourceType result) {
            result = PaintSourceType.Solid;

            if (string.IsNullOrWhiteSpace(value)) {
                return false;
            }

            switch (value.Trim().ToLowerInvariant()) {
                case "solid":
                case "plain":
                case "none":
                    result = PaintSourceType.Solid;
                    return true;

                case "gradient":
                case "ramp":
                    result = PaintSourceType.Gradient;
                    return true;

                case "camo":
                case "camouflage":
                    result = PaintSourceType.Camo;
                    return true;

                case "scatter":
                case "speckle":
                    result = PaintSourceType.Scatter;
                    return true;

                case "pattern":
                    result = PaintSourceType.Pattern;
                    return true;

                default:
                    return false;
            }
        }

        private static bool TryParseSourceAxis(string value, out PaintSourceAxis result) {
            result = PaintSourceAxis.GridY;

            if (string.IsNullOrWhiteSpace(value)) {
                return false;
            }

            switch (value.Trim().ToLowerInvariant()) {
                case "x":
                case "gridx":
                    result = PaintSourceAxis.GridX;
                    return true;

                case "y":
                case "gridy":
                    result = PaintSourceAxis.GridY;
                    return true;

                case "z":
                case "gridz":
                    result = PaintSourceAxis.GridZ;
                    return true;

                case "up":
                case "worldup":
                case "gravity":
                    result = PaintSourceAxis.WorldUp;
                    return true;

                case "radial":
                case "center":
                    result = PaintSourceAxis.Radial;
                    return true;

                case "long":
                case "longest":
                    result = PaintSourceAxis.Longest;
                    return true;

                default:
                    return false;
            }
        }

        private static bool TryParseGradientFit(string value, out PaintGradientFit result) {
            result = PaintGradientFit.MatchedBlocks;

            if (string.IsNullOrWhiteSpace(value)) {
                return false;
            }

            switch (value.Trim().ToLowerInvariant()) {
                case "blocks":
                case "matched":
                case "painted":
                    result = PaintGradientFit.MatchedBlocks;
                    return true;

                case "grid":
                case "bounds":
                case "gridbounds":
                    result = PaintGradientFit.GridBounds;
                    return true;

                default:
                    return false;
            }
        }

        private static bool TryParseBlendSpace(string value, out PaintBlendSpace result) {
            result = PaintBlendSpace.Lab;

            if (string.IsNullOrWhiteSpace(value)) {
                return false;
            }

            switch (value.Trim().ToLowerInvariant()) {
                case "hsv":
                    result = PaintBlendSpace.Hsv;
                    return true;

                case "lab":
                    result = PaintBlendSpace.Lab;
                    return true;

                case "rgb":
                    result = PaintBlendSpace.Rgb;
                    return true;

                default:
                    return false;
            }
        }

        private static bool TryParsePatternShape(string value, out PaintPatternShape result) {
            result = PaintPatternShape.Stripes;

            if (string.IsNullOrWhiteSpace(value)) {
                return false;
            }

            switch (value.Trim().ToLowerInvariant()) {
                case "stripes":
                case "bands":
                    result = PaintPatternShape.Stripes;
                    return true;

                case "checker":
                case "checkers":
                    result = PaintPatternShape.Checker;
                    return true;

                default:
                    return false;
            }
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
        /// Reads TypeId/SubtypeId.
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
        /// Sets the type a value field belongs to while a condition is being created.
        /// </summary>
        private static bool ApplyInferredType(PaintRuleCondition condition, PaintRuleConditionType type, bool isNew, bool typeGiven, bool valueFieldGiven) {
            if (isNew && !typeGiven && !valueFieldGiven) {
                condition.Type = type;
            }

            return true;
        }
    }
}
