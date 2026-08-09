using Sandbox.ModAPI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using Sisk.BuildColors.UI;
using Sisk.Utils.CommandHandler;
using Sisk.Utils.Localization.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sisk.BuildColors.Services {

    /// <summary>
    /// Chat commands covering the paint job feature.
    /// </summary>
    internal static class PaintJobCommands {
        public static void Register(CommandHandler handler) {
            handler.Register(new Command { Name = "Jobs", Description = ModText.BC_Description_Jobs.GetString(), Execute = ListJobs });
            handler.Register(new Command { Name = "ShowJob", Description = ModText.BC_Description_ShowJob.GetString(), Execute = ShowJob });
            handler.Register(new Command { Name = "ApplyJob", Description = ModText.BC_Description_ApplyJob.GetString(), Execute = ApplyJob });
            handler.Register(new Command { Name = "UndoJob", Description = ModText.BC_Description_UndoJob.GetString(), Execute = UndoJob });
            handler.Register(new Command { Name = "PaintHistory", Description = ModText.BC_Description_PaintHistory.GetString(), Execute = ListHistory });
            handler.Register(new Command { Name = "NewJob", Description = ModText.BC_Description_NewJob.GetString(), Execute = NewJob });
            handler.Register(new Command { Name = "RemoveJob", Description = ModText.BC_Description_RemoveJob.GetString(), Execute = RemoveJob });
            handler.Register(new Command { Name = "RenameJob", Description = ModText.BC_Description_RenameJob.GetString(), Execute = RenameJob });
            handler.Register(new Command { Name = "CopyJob", Description = ModText.BC_Description_CopyJob.GetString(), Execute = CopyJob });
            handler.Register(new Command { Name = "ShareJob", Description = ModText.BC_Description_ShareJob.GetString(), Execute = ShareJob });
            handler.Register(new Command { Name = "JobOption", Description = ModText.BC_Description_JobOption.GetString(), Execute = SetJobOption });
            handler.Register(new Command { Name = "AddRule", Description = ModText.BC_Description_AddRule.GetString(), Execute = AddRule });
            handler.Register(new Command { Name = "RemoveRule", Description = ModText.BC_Description_RemoveRule.GetString(), Execute = RemoveRule });
            handler.Register(new Command { Name = "RenameRule", Description = ModText.BC_Description_RenameRule.GetString(), Execute = RenameRule });
            handler.Register(new Command { Name = "MoveRule", Description = ModText.BC_Description_MoveRule.GetString(), Execute = MoveRule });
            handler.Register(new Command { Name = "RuleAction", Description = ModText.BC_Description_RuleAction.GetString(), Execute = SetRuleAction });
            handler.Register(new Command { Name = "AddCondition", Description = ModText.BC_Description_AddCondition.GetString(), Execute = AddCondition });
            handler.Register(new Command { Name = "SetCondition", Description = ModText.BC_Description_SetCondition.GetString(), Execute = SetCondition });
            handler.Register(new Command { Name = "RemoveCondition", Description = ModText.BC_Description_RemoveCondition.GetString(), Execute = RemoveNode });
            handler.Register(new Command { Name = "MoveCondition", Description = ModText.BC_Description_MoveCondition.GetString(), Execute = MoveNode });
            handler.Register(new Command { Name = "AddGroup", Description = ModText.BC_Description_AddGroup.GetString(), Execute = AddGroup });
            handler.Register(new Command { Name = "SetGroup", Description = ModText.BC_Description_SetGroup.GetString(), Execute = SetGroup });
            handler.Register(new Command { Name = "Skins", Description = ModText.BC_Description_Skins.GetString(), Execute = ListSkins });
        }

        private static void ListJobs(string arguments) {
            var jobs = Mod.Static?.PaintJobs;

            if (jobs == null || jobs.Count == 0) {
                Show(ModText.BC_NoPaintJobsAvailable.GetString());
                return;
            }

            var names = PaintJobService.InNameOrder(jobs).Select((job, index) => string.Format("#{0} {1}", index + 1, job.Name));
            Show(string.Join(", ", names));
        }

        private static void ShowJob(string arguments) {
            var tokens = CommandArguments.Split(arguments);

            if (tokens.Count != 1) {
                Show(ModText.BC_Cmd_Usage_ShowJob.GetString(Mod.Acronym));
                return;
            }

            var job = ResolveJob(tokens[0]);
            if (job == null) {
                return;
            }

            MyAPIGateway.Utilities.ShowMissionScreen(Mod.NAME, string.Empty, job.Name, PaintJobReport.BuildJobReport(job), okButtonCaption: ModText.BC_UI_Done.GetString());
        }

        /// <summary>
        /// Sends a paint job to one player, or to everyone online when no player is named.
        /// </summary>
        private static void ShareJob(string arguments) {
            var tokens = CommandArguments.Split(arguments);

            if (tokens.Count < 1 || tokens.Count > 2) {
                Show(ModText.BC_Cmd_Usage_ShareJob.GetString(Mod.Acronym));
                return;
            }

            var job = ResolveJob(tokens[0]);
            if (job == null) {
                return;
            }

            ShareService.Share(new SharePacket { Kind = ShareKind.PaintJob, PaintJob = job.Clone() }, tokens.Count == 2 ? tokens[1] : null);
        }

        private static void ApplyJob(string arguments) {
            var tokens = CommandArguments.Split(arguments);

            if (tokens.Count != 1) {
                Show(ModText.BC_PaintJob_ApplyJobUsage.GetString(Mod.Acronym));
                return;
            }

            var job = ResolveJob(tokens[0]);
            if (job == null) {
                return;
            }

            Mod.Static?.PaintJobService?.ApplyJobToSelection(job);
        }

        /// <summary>
        /// Puts back an application of a paint job.
        /// </summary>
        private static void UndoJob(string arguments) {
            var tokens = CommandArguments.Split(arguments);
            var position = 1;

            if (tokens.Count > 1 || (tokens.Count == 1 && !TryParsePosition(tokens[0], out position)) || position < 1) {
                Show(ModText.BC_Cmd_Usage_UndoJob.GetString(Mod.Acronym));
                return;
            }

            Mod.Static?.PaintJobService?.UndoPaintJob(position);
        }

        private static void ListHistory(string arguments) {
            var service = Mod.Static?.PaintJobService;
            var entries = service?.History.Entries;

            if (entries == null || entries.Count == 0) {
                Show(ModText.BC_PaintJob_NothingToUndo.GetString());
                return;
            }

            var lines = new List<string>();

            for (var i = entries.Count - 1; i >= 0; i--) {
                var entry = entries[i];
                lines.Add(ModText.BC_Cmd_HistoryLine.GetString(entries.Count - i, entry.JobName, entry.BlockCount, entry.GridName));
            }

            Show(string.Join(", ", lines));
        }

        /// <summary>
        /// Reads a position written either as #2 or as a bare number.
        /// </summary>
        private static bool TryParsePosition(string token, out int position) {
            return CommandArguments.TryParseSelector(token, out position) || CommandArguments.TryParseInteger(token, out position);
        }

        private static void NewJob(string arguments) {
            var tokens = CommandArguments.Split(arguments);
            var service = Mod.Static?.PaintJobService;

            if (tokens.Count != 1 || string.IsNullOrWhiteSpace(tokens[0]) || service == null) {
                Show(ModText.BC_Cmd_Usage_NewJob.GetString(Mod.Acronym));
                return;
            }

            if (FindJob(tokens[0]) != null) {
                Show(ModText.BC_Cmd_JobExists.GetString(tokens[0]));
                return;
            }

            var job = service.CreateJob(tokens[0]);
            RefreshUI(job);
            Show(ModText.BC_Cmd_JobCreated.GetString(job.Name));
        }

        private static void RemoveJob(string arguments) {
            var tokens = CommandArguments.Split(arguments);

            if (tokens.Count != 1) {
                Show(ModText.BC_Cmd_Usage_RemoveJob.GetString(Mod.Acronym));
                return;
            }

            var job = ResolveJob(tokens[0]);
            if (job == null) {
                return;
            }

            if (Mod.Static?.PaintJobService?.RemoveJob(job) != true) {
                return;
            }

            RefreshUI(null);
            Show(ModText.BC_Cmd_JobRemoved.GetString(job.Name));
        }

        private static void RenameJob(string arguments) {
            var tokens = CommandArguments.Split(arguments);

            if (tokens.Count != 2 || string.IsNullOrWhiteSpace(tokens[1])) {
                Show(ModText.BC_Cmd_Usage_RenameJob.GetString(Mod.Acronym));
                return;
            }

            var job = ResolveJob(tokens[0]);
            if (job == null) {
                return;
            }

            var existing = FindJob(tokens[1]);
            if (existing != null && existing.Id != job.Id) {
                Show(ModText.BC_Cmd_JobExists.GetString(tokens[1]));
                return;
            }

            var previousName = job.Name;
            job.Name = tokens[1].Trim();

            Persist(job);
            Show(ModText.BC_Cmd_JobRenamed.GetString(previousName, job.Name));
        }

        private static void CopyJob(string arguments) {
            var tokens = CommandArguments.Split(arguments);

            if (tokens.Count != 2 || string.IsNullOrWhiteSpace(tokens[1])) {
                Show(ModText.BC_Cmd_Usage_CopyJob.GetString(Mod.Acronym));
                return;
            }

            var job = ResolveJob(tokens[0]);
            if (job == null) {
                return;
            }

            if (FindJob(tokens[1]) != null) {
                Show(ModText.BC_Cmd_JobExists.GetString(tokens[1]));
                return;
            }

            var copy = job.Clone();
            copy.Id = Guid.NewGuid();
            copy.Name = tokens[1].Trim();

            Persist(copy);
            Show(ModText.BC_Cmd_JobCopied.GetString(job.Name, copy.Name));
        }

        private static void SetJobOption(string arguments) {
            var tokens = CommandArguments.Split(arguments);

            if (tokens.Count < 2 || tokens.Count > 3) {
                Show(ModText.BC_Cmd_Usage_JobOption.GetString(Mod.Acronym));
                return;
            }

            var job = ResolveJob(tokens[0]);
            if (job == null) {
                return;
            }

            if (job.Options == null) {
                job.Options = new PaintJobOptions();
            }

            var option = tokens[1].Trim().ToLowerInvariant();
            var current = ReadOption(job.Options, option);

            if (!current.HasValue) {
                Show(ModText.BC_Cmd_UnknownOption.GetString(tokens[1], "subgrids, projected, preview, ownership"));
                return;
            }

            var requested = tokens.Count == 3 ? tokens[2] : "toggle";

            bool value;
            if (!CommandArguments.TryParseFlag(requested, current.Value, out value)) {
                Show(ModText.BC_Cmd_InvalidValue.GetString(requested, tokens[1]));
                return;
            }

            WriteOption(job.Options, option, value);

            Persist(job);
            Show(ModText.BC_Cmd_OptionSet.GetString(option, job.Name, PaintJobReport.DescribeFlag(value)));
        }

        private static void AddRule(string arguments) {
            var tokens = CommandArguments.Split(arguments);

            if (tokens.Count < 1 || tokens.Count > 2) {
                Show(ModText.BC_Cmd_Usage_AddRule.GetString(Mod.Acronym));
                return;
            }

            var job = ResolveJob(tokens[0]);
            if (job == null) {
                return;
            }

            job.EnsureRules();

            var name = tokens.Count == 2 ? tokens[1].Trim() : NextRuleName(job);
            if (string.IsNullOrWhiteSpace(name)) {
                Show(ModText.BC_Cmd_Usage_AddRule.GetString(Mod.Acronym));
                return;
            }

            var rule = PaintRule.CreateDefault(name);
            job.Rules.Add(rule);

            Persist(job);
            Show(ModText.BC_Cmd_RuleAdded.GetString(rule.Name, job.Name, job.Rules.Count));
        }

        private static void RemoveRule(string arguments) {
            var tokens = CommandArguments.Split(arguments);

            if (tokens.Count != 2) {
                Show(ModText.BC_Cmd_Usage_RemoveRule.GetString(Mod.Acronym));
                return;
            }

            PaintJob job;
            PaintRule rule;
            if (!TryResolveRule(tokens[0], tokens[1], out job, out rule)) {
                return;
            }

            if (job.Rules.Count <= 1) {
                Show(ModText.BC_Cmd_RuleRequired.GetString());
                return;
            }

            job.Rules.Remove(rule);

            Persist(job);
            Show(ModText.BC_Cmd_RuleRemoved.GetString(rule.Name, job.Name));
        }

        private static void RenameRule(string arguments) {
            var tokens = CommandArguments.Split(arguments);

            if (tokens.Count != 3 || string.IsNullOrWhiteSpace(tokens[2])) {
                Show(ModText.BC_Cmd_Usage_RenameRule.GetString(Mod.Acronym));
                return;
            }

            PaintJob job;
            PaintRule rule;
            if (!TryResolveRule(tokens[0], tokens[1], out job, out rule)) {
                return;
            }

            var previousName = rule.Name;
            rule.Name = tokens[2].Trim();

            Persist(job);
            Show(ModText.BC_Cmd_RuleRenamed.GetString(previousName, rule.Name));
        }

        private static void MoveRule(string arguments) {
            var tokens = CommandArguments.Split(arguments);
            int position;

            if (tokens.Count != 3 || !CommandArguments.TryParseInteger(tokens[2], out position)) {
                Show(ModText.BC_Cmd_Usage_MoveRule.GetString(Mod.Acronym));
                return;
            }

            PaintJob job;
            PaintRule rule;
            if (!TryResolveRule(tokens[0], tokens[1], out job, out rule)) {
                return;
            }

            if (position < 1 || position > job.Rules.Count) {
                Show(ModText.BC_Cmd_InvalidPosition.GetString(position, job.Rules.Count));
                return;
            }

            job.Rules.Remove(rule);
            job.Rules.Insert(position - 1, rule);

            Persist(job);
            Show(ModText.BC_Cmd_RuleMoved.GetString(rule.Name, position));
        }

        private static void SetRuleAction(string arguments) {
            var tokens = CommandArguments.Split(arguments);

            if (tokens.Count < 3) {
                Show(ModText.BC_Cmd_Usage_RuleAction.GetString(Mod.Acronym));
                return;
            }

            PaintJob job;
            PaintRule rule;
            if (!TryResolveRule(tokens[0], tokens[1], out job, out rule)) {
                return;
            }

            if (rule.Action == null) {
                rule.Action = new PaintRuleAction();
            }

            string error;
            if (!PaintRuleFields.TryApplyToAction(rule.Action, tokens.GetRange(2, tokens.Count - 2), out error)) {
                Show(error);
                return;
            }

            Persist(job);
            Show(ModText.BC_Cmd_ActionUpdated.GetString(rule.Name, PaintJobReport.DescribeAction(rule.Action)));
        }

        private static void AddCondition(string arguments) {
            var tokens = CommandArguments.Split(arguments);

            if (tokens.Count < 3) {
                Show(ModText.BC_Cmd_Usage_AddCondition.GetString(Mod.Acronym));
                return;
            }

            PaintJob job;
            PaintRule rule;
            if (!TryResolveRule(tokens[0], tokens[1], out job, out rule)) {
                return;
            }

            var index = 2;
            var path = PaintRulePath.ROOT;

            if (!tokens[index].Contains("=")) {
                path = tokens[index];
                index++;
            }

            if (index >= tokens.Count) {
                Show(ModText.BC_Cmd_Usage_AddCondition.GetString(Mod.Acronym));
                return;
            }

            var group = ResolveGroup(rule, path);
            if (group == null) {
                return;
            }

            var condition = new PaintRuleCondition();
            string error;

            if (!PaintRuleFields.TryApplyToCondition(condition, tokens.GetRange(index, tokens.Count - index), true, out error)) {
                Show(error);
                return;
            }

            if (group.Conditions == null) {
                group.Conditions = new List<PaintRuleCondition>();
            }

            group.Conditions.Add(condition);

            Persist(job);
            Show(ModText.BC_Cmd_ConditionAdded.GetString(FindPath(rule, condition), PaintRuleConditionText.Describe(condition)));
        }

        private static void SetCondition(string arguments) {
            var tokens = CommandArguments.Split(arguments);

            if (tokens.Count < 4) {
                Show(ModText.BC_Cmd_Usage_SetCondition.GetString(Mod.Acronym));
                return;
            }

            PaintJob job;
            PaintRule rule;
            if (!TryResolveRule(tokens[0], tokens[1], out job, out rule)) {
                return;
            }

            var node = ResolveNode(rule, tokens[2]);
            if (node == null) {
                return;
            }

            if (node.IsGroup) {
                Show(ModText.BC_Cmd_PathIsGroup.GetString(node.Path, Mod.Acronym));
                return;
            }

            string error;
            if (!PaintRuleFields.TryApplyToCondition(node.Condition, tokens.GetRange(3, tokens.Count - 3), false, out error)) {
                Show(error);
                return;
            }

            Persist(job);
            Show(ModText.BC_Cmd_ConditionUpdated.GetString(node.Path, PaintRuleConditionText.Describe(node.Condition)));
        }

        private static void RemoveNode(string arguments) {
            var tokens = CommandArguments.Split(arguments);

            if (tokens.Count != 3) {
                Show(ModText.BC_Cmd_Usage_RemoveCondition.GetString(Mod.Acronym));
                return;
            }

            PaintJob job;
            PaintRule rule;
            if (!TryResolveRule(tokens[0], tokens[1], out job, out rule)) {
                return;
            }

            var node = ResolveNode(rule, tokens[2]);
            if (node == null) {
                return;
            }

            if (node.Parent == null) {
                Show(ModText.BC_Cmd_RootProtected.GetString());
                return;
            }

            var description = PaintJobReport.DescribeNode(node);

            if (node.IsGroup) {
                node.Parent.Children.RemoveAt(node.Index);
            } else {
                node.Parent.Conditions.RemoveAt(node.Index);
            }

            Persist(job);
            Show(ModText.BC_Cmd_NodeRemoved.GetString(node.Path, description));
        }

        private static void MoveNode(string arguments) {
            var tokens = CommandArguments.Split(arguments);

            if (tokens.Count < 4 || tokens.Count > 5) {
                Show(ModText.BC_Cmd_Usage_MoveCondition.GetString(Mod.Acronym));
                return;
            }

            PaintJob job;
            PaintRule rule;
            if (!TryResolveRule(tokens[0], tokens[1], out job, out rule)) {
                return;
            }

            var node = ResolveNode(rule, tokens[2]);
            if (node == null) {
                return;
            }

            if (node.Parent == null) {
                Show(ModText.BC_Cmd_RootProtected.GetString());
                return;
            }

            var target = ResolveGroup(rule, tokens[3]);
            if (target == null) {
                return;
            }

            if (node.IsGroup && PaintRulePath.Contains(node.Group, target)) {
                Show(ModText.BC_Cmd_MoveIntoSelf.GetString());
                return;
            }

            if (node.IsGroup) {
                if (target.Children == null) {
                    target.Children = new List<PaintRuleConditionGroup>();
                }

                node.Parent.Children.RemoveAt(node.Index);

                int position;
                if (!TryReadPosition(tokens, target.Children.Count, out position)) {
                    node.Parent.Children.Insert(node.Index, node.Group);
                    return;
                }

                target.Children.Insert(position, node.Group);
            } else {
                if (target.Conditions == null) {
                    target.Conditions = new List<PaintRuleCondition>();
                }

                node.Parent.Conditions.RemoveAt(node.Index);

                int position;
                if (!TryReadPosition(tokens, target.Conditions.Count, out position)) {
                    node.Parent.Conditions.Insert(node.Index, node.Condition);
                    return;
                }

                target.Conditions.Insert(position, node.Condition);
            }

            Persist(job);

            var movedTo = node.IsGroup ? FindPath(rule, node.Group) : FindPath(rule, node.Condition);
            Show(ModText.BC_Cmd_NodeMoved.GetString(PaintJobReport.DescribeNode(node), movedTo));
        }

        private static void AddGroup(string arguments) {
            var tokens = CommandArguments.Split(arguments);

            if (tokens.Count < 2) {
                Show(ModText.BC_Cmd_Usage_AddGroup.GetString(Mod.Acronym));
                return;
            }

            PaintJob job;
            PaintRule rule;
            if (!TryResolveRule(tokens[0], tokens[1], out job, out rule)) {
                return;
            }

            var path = PaintRulePath.ROOT;
            var group = new PaintRuleConditionGroup();

            for (var i = 2; i < tokens.Count; i++) {
                PaintRuleLogicalOperator logicalOperator;

                if (PaintRuleFields.TryParseOperator(tokens[i], out logicalOperator)) {
                    group.Operator = logicalOperator;
                    continue;
                }

                if (IsNegation(tokens[i])) {
                    group.Negate = true;
                    continue;
                }

                path = tokens[i];
            }

            var parent = ResolveGroup(rule, path);
            if (parent == null) {
                return;
            }

            if (parent.Children == null) {
                parent.Children = new List<PaintRuleConditionGroup>();
            }

            parent.Children.Add(group);

            Persist(job);
            Show(ModText.BC_Cmd_GroupAdded.GetString(FindPath(rule, group), PaintRuleConditionText.DescribeOperator(group)));
        }

        private static void SetGroup(string arguments) {
            var tokens = CommandArguments.Split(arguments);

            if (tokens.Count < 4) {
                Show(ModText.BC_Cmd_Usage_SetGroup.GetString(Mod.Acronym));
                return;
            }

            PaintJob job;
            PaintRule rule;
            if (!TryResolveRule(tokens[0], tokens[1], out job, out rule)) {
                return;
            }

            var node = ResolveNode(rule, tokens[2]);
            if (node == null) {
                return;
            }

            if (!node.IsGroup) {
                Show(ModText.BC_Cmd_PathIsCondition.GetString(node.Path, Mod.Acronym));
                return;
            }

            for (var i = 3; i < tokens.Count; i++) {
                PaintRuleLogicalOperator logicalOperator;

                if (PaintRuleFields.TryParseOperator(tokens[i], out logicalOperator)) {
                    node.Group.Operator = logicalOperator;
                    continue;
                }

                if (IsNegation(tokens[i])) {
                    node.Group.Negate = true;
                    continue;
                }

                string field;
                string value;
                bool negate;

                if (CommandArguments.TrySplitAssignment(tokens[i], out field, out value)
                    && (field == "not" || field == "negate")
                    && CommandArguments.TryParseFlag(value, node.Group.Negate, out negate)) {
                    node.Group.Negate = negate;
                    continue;
                }

                Show(ModText.BC_Cmd_InvalidValue.GetString(tokens[i], "and|or|not|not=on|not=off"));
                return;
            }

            Persist(job);
            Show(ModText.BC_Cmd_GroupUpdated.GetString(node.Path, PaintRuleConditionText.DescribeOperator(node.Group)));
        }

        private static void ListSkins(string arguments) {
            var filter = arguments != null ? arguments.Trim() : string.Empty;

            var matches = DefinitionCatalog.Skins
                .Where(skin => !string.IsNullOrEmpty(skin.SkinId))
                .Where(skin => filter.Length == 0 || skin.SkinId.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0)
                .Select(skin => skin.SkinId)
                .ToArray();

            if (matches.Length == 0) {
                Show(ModText.BC_Cmd_NoSkinsMatched.GetString(filter));
                return;
            }

            Show(ModText.BC_Cmd_SkinList.GetString(string.Join(", ", matches)));
        }

        private static void Show(string message) {
            MyAPIGateway.Utilities.ShowMessage(Mod.NAME, message);
        }

        private static PaintJob FindJob(string reference) {
            var jobs = Mod.Static?.PaintJobs;

            if (jobs == null || jobs.Count == 0 || string.IsNullOrWhiteSpace(reference)) {
                return null;
            }

            int position;
            if (CommandArguments.TryParseSelector(reference, out position)) {
                var ordered = PaintJobService.InNameOrder(jobs).ToArray();

                return position >= 1 && position <= ordered.Length ? ordered[position - 1] : null;
            }

            return jobs.FirstOrDefault(job => string.Equals(job.Name, reference.Trim(), StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Looks a job up and reports it when there is none, so callers only have to check for null.
        /// </summary>
        private static PaintJob ResolveJob(string reference) {
            var job = FindJob(reference);

            if (job == null) {
                Show(ModText.BC_NoPaintJobFound.GetString(reference));
                return null;
            }

            job.EnsureRules();

            return job;
        }

        private static bool TryResolveRule(string jobReference, string ruleReference, out PaintJob job, out PaintRule rule) {
            rule = null;
            job = ResolveJob(jobReference);

            if (job == null) {
                return false;
            }

            int position;
            if (CommandArguments.TryParseSelector(ruleReference, out position)) {
                if (position >= 1 && position <= job.Rules.Count) {
                    rule = job.Rules[position - 1];
                }
            } else {
                rule = job.Rules.FirstOrDefault(x => string.Equals(x.Name, ruleReference.Trim(), StringComparison.InvariantCultureIgnoreCase));
            }

            if (rule == null) {
                Show(ModText.BC_Cmd_RuleNotFound.GetString(ruleReference, job.Name));
                return false;
            }

            if (rule.ConditionGroup == null) {
                rule.ConditionGroup = PaintRuleConditionGroup.CreateDefault();
            }

            return true;
        }

        private static PaintRuleNode ResolveNode(PaintRule rule, string path) {
            var node = PaintRulePath.Resolve(rule.ConditionGroup, path);

            if (node == null) {
                Show(ModText.BC_Cmd_PathNotFound.GetString(path, Mod.Acronym));
            }

            return node;
        }

        private static PaintRuleConditionGroup ResolveGroup(PaintRule rule, string path) {
            var node = ResolveNode(rule, path);

            if (node == null) {
                return null;
            }

            if (!node.IsGroup) {
                Show(ModText.BC_Cmd_PathNotGroup.GetString(node.Path));
                return null;
            }

            return node.Group;
        }

        /// <summary>
        /// Reads the optional target position of a move.
        /// </summary>
        private static bool TryReadPosition(List<string> tokens, int count, out int position) {
            position = count;

            if (tokens.Count < 5) {
                return true;
            }

            int requested;
            if (!CommandArguments.TryParseInteger(tokens[4], out requested) || requested < 1 || requested > count + 1) {
                Show(ModText.BC_Cmd_InvalidPosition.GetString(tokens[4], count + 1));
                return false;
            }

            position = requested - 1;

            return true;
        }

        private static string FindPath(PaintRule rule, object node) {
            foreach (var candidate in PaintRulePath.Flatten(rule.ConditionGroup)) {
                if (ReferenceEquals(candidate.Group, node) || ReferenceEquals(candidate.Condition, node)) {
                    return candidate.Path;
                }
            }

            return PaintRulePath.ROOT;
        }

        private static bool IsNegation(string token) {
            var text = token.Trim().ToLowerInvariant();

            return text == "not" || text == "negate";
        }

        private static string NextRuleName(PaintJob job) {
            var index = job.Rules.Count + 1;
            var name = ModText.BC_UI_DefaultRuleName.GetString(index);

            while (job.Rules.Exists(rule => string.Equals(rule.Name, name, StringComparison.InvariantCultureIgnoreCase))) {
                index++;
                name = ModText.BC_UI_DefaultRuleName.GetString(index);
            }

            return name;
        }

        private static bool? ReadOption(PaintJobOptions options, string option) {
            switch (option) {
                case "subgrids":
                    return options.IncludeSubgrids;

                case "projected":
                    return options.IncludeProjectedGrids;

                case "preview":
                    return options.IncludePreviewGrids;

                case "ownership":
                    return options.RespectOwnership;

                default:
                    return null;
            }
        }

        private static void WriteOption(PaintJobOptions options, string option, bool value) {
            switch (option) {
                case "subgrids":
                    options.IncludeSubgrids = value;
                    break;

                case "projected":
                    options.IncludeProjectedGrids = value;
                    break;

                case "preview":
                    options.IncludePreviewGrids = value;
                    break;

                case "ownership":
                    options.RespectOwnership = value;
                    break;
            }
        }

        private static void Persist(PaintJob job) {
            Mod.Static?.PaintJobService?.SaveJob(job);
            RefreshUI(job);
        }

        private static void RefreshUI(PaintJob job) {
            Mod.Static?.RefreshPaintJobs(job);
        }
    }
}
