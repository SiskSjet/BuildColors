using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using Sisk.BuildColors.UI;
using Sisk.Utils.Localization.Extensions;
using System.Collections.Generic;
using System.Text;

namespace Sisk.BuildColors.Services {

    /// <summary>
    /// Renders a paint job as text for the console.
    /// </summary>
    internal static class PaintJobReport {
        private const int INDENT_SPACES = 2;

        public static string BuildJobReport(PaintJob job) {
            job.EnsureRules();

            var options = job.Options ?? new PaintJobOptions();
            var builder = new StringBuilder();

            builder.AppendLine(ModText.BC_Cmd_JobHeader.GetString(job.Name));
            builder.AppendLine(ModText.BC_Cmd_JobOptionsLine.GetString(
                DescribeFlag(options.IncludeSubgrids),
                DescribeFlag(options.IncludeProjectedGrids),
                DescribeFlag(options.IncludePreviewGrids),
                DescribeFlag(options.RespectOwnership)));
            builder.AppendLine();
            builder.AppendLine(ModText.BC_Cmd_ShowJobHint.GetString());

            for (var i = 0; i < job.Rules.Count; i++) {
                var rule = job.Rules[i];

                builder.AppendLine();
                builder.AppendLine(ModText.BC_Cmd_RuleLine.GetString(i + 1, rule.Name));
                builder.AppendLine(ModText.BC_Cmd_WhenLine.GetString());

                foreach (var node in PaintRulePath.Flatten(rule.ConditionGroup)) {
                    builder.AppendLine(BuildNodeLine(node));
                }

                builder.AppendLine(ModText.BC_Cmd_ThenLine.GetString(DescribeAction(rule.Action)));
            }

            return builder.ToString();
        }

        public static string DescribeAction(PaintRuleAction action) {
            if (action == null || !action.ApplyColor && !action.ApplySkin) {
                return ModText.BC_Cmd_ActionNone.GetString();
            }

            var parts = new List<string>();

            if (action.SourceType != PaintSourceType.Solid) {
                parts.Add(PaintSourceText.Describe(action.Source));
                parts.Add(action.ApplyColor && action.ApplySkin
                    ? ModText.BC_Cmd_ActionSourceBoth.GetString()
                    : action.ApplyColor
                        ? ModText.BC_Cmd_ActionSourceColorOnly.GetString()
                        : ModText.BC_Cmd_ActionSourceSkinOnly.GetString());

                return string.Join(", ", parts);
            }

            if (action.ApplyColor) {
                parts.Add(ModText.BC_Cmd_ActionColor.GetString(action.TargetColor.R, action.TargetColor.G, action.TargetColor.B));
            }

            if (action.ApplySkin) {
                parts.Add(string.IsNullOrWhiteSpace(action.TargetSkinId)
                    ? ModText.BC_Cmd_ActionSkinNone.GetString()
                    : ModText.BC_Cmd_ActionSkin.GetString(action.TargetSkinId));
            }

            return string.Join(", ", parts);
        }

        public static string DescribeNode(PaintRuleNode node) {
            if (node == null) {
                return string.Empty;
            }

            return node.IsGroup
                ? ModText.BC_Cmd_GroupText.GetString(PaintRuleConditionText.DescribeOperator(node.Group))
                : PaintRuleConditionText.Describe(node.Condition);
        }

        public static string DescribeFlag(bool value) {
            return value ? ModText.BC_Cmd_On.GetString() : ModText.BC_Cmd_Off.GetString();
        }

        private static string BuildNodeLine(PaintRuleNode node) {
            var indent = new string(' ', (node.Depth + 2) * INDENT_SPACES);

            return string.Format("{0}[{1}] {2}", indent, node.Path, DescribeNode(node));
        }
    }
}
