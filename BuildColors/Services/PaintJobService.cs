using Sandbox.ModAPI;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.Services {

    /// <summary>
    ///     Runtime helper that encapsulates persistence and execution of paint jobs.
    /// </summary>
    public class PaintJobService {

        /// <summary>
        ///     Per channel tolerance used when comparing a block color against a job color. The game stores colors as a
        ///     color mask, so a round trip back to RGB is not always bit exact.
        /// </summary>
        private const int COLOR_CHANNEL_TOLERANCE = 2;

        private const float RAY_LENGTH = 120f;

        private readonly Mod _mod;

        public PaintJobService(Mod mod) {
            _mod = mod;
        }

        public IReadOnlyCollection<PaintJob> GetJobs() {
            if (_mod.PaintJobs == null) {
                return new PaintJob[0];
            }

            foreach (var paintRule in _mod.PaintJobs) {
                paintRule.EnsureRules();
            }

            return _mod.PaintJobs;
        }

        public PaintJob CreateJob(string name = null) {
            var job = new PaintJob();
            if (!string.IsNullOrWhiteSpace(name)) {
                job.Name = name;
            } else {
                var index = (_mod.PaintJobs?.Count ?? 0) + 1;
                while (_mod.PaintJobs.Any(r => string.Equals(r.Name, $"Paint Job {index}", StringComparison.InvariantCultureIgnoreCase))) {
                    index++;
                }

                job.Name = $"Paint Job {index}";
            }

            job.EnsureRules();
            _mod.PaintJobs.Add(job);
            _mod.SavePaintJobs();

            return job;
        }

        public bool RemoveJob(PaintJob job) {
            if (job == null) {
                return false;
            }

            var removed = _mod.PaintJobs.RemoveWhere(x => x.Id == job.Id) > 0;
            if (removed) {
                _mod.SavePaintJobs();
            }

            return removed;
        }

        public void SaveJob(PaintJob job) {
            if (job == null) {
                return;
            }

            if (_mod.PaintJobs.Contains(job)) {
                _mod.PaintJobs.Remove(job);
            }

            _mod.PaintJobs.Add(job);
            _mod.SavePaintJobs();
        }

        /// <summary>
        ///     Applies a paint job to the grid the player is currently aiming at.
        /// </summary>
        public void ApplyJobToSelection(PaintJob job) {
            if (job == null) {
                return;
            }

            var player = MyAPIGateway.Session?.LocalHumanPlayer;
            if (player == null) {
                MyAPIGateway.Utilities.ShowMessage(Mod.NAME, "Unable to resolve local player for applying a paint job.");
                return;
            }

            var targetedGrid = GetTargetedGrid(player);
            if (targetedGrid == null) {
                MyAPIGateway.Utilities.ShowMessage(Mod.NAME, "Aim at a grid before applying a paint job.");
                return;
            }

            job.EnsureRules();
            var options = job.Options ?? new PaintJobOptions();

            var grids = CollectGrids(targetedGrid, options);
            var skippedGrids = 0;
            var changedBlocks = 0;
            var matchedBlocks = 0;
            var blocks = new List<IMySlimBlock>();

            foreach (var grid in grids) {
                if (options.RespectOwnership && !IsModifiableBy(player, grid)) {
                    skippedGrids++;
                    continue;
                }

                blocks.Clear();
                grid.GetBlocks(blocks);

                foreach (var block in blocks) {
                    if (block == null || block.IsDestroyed) {
                        continue;
                    }

                    var rule = FindMatchingRule(job, block);
                    if (rule == null) {
                        continue;
                    }

                    matchedBlocks++;
                    if (ApplyAction(block, rule.Action)) {
                        changedBlocks++;
                    }
                }
            }

            ReportResult(job, targetedGrid, matchedBlocks, changedBlocks, skippedGrids);
        }

        /// <summary>
        ///     Returns the first rule of the job whose conditions match the given block, or null when none match.
        /// </summary>
        private static PaintRule FindMatchingRule(PaintJob job, IMySlimBlock block) {
            if (job.Rules == null) {
                return null;
            }

            foreach (var rule in job.Rules) {
                if (rule?.ConditionGroup == null || rule.Action == null) {
                    continue;
                }

                if (GroupMatches(rule.ConditionGroup, block)) {
                    return rule;
                }
            }

            return null;
        }

        /// <summary>
        ///     Evaluates a condition group against a block. Conditions that were never configured are ignored, and a group
        ///     without any configured condition never matches so an untouched job cannot repaint a whole grid.
        /// </summary>
        private static bool GroupMatches(PaintRuleConditionGroup group, IMySlimBlock block) {
            var requireAll = group.Operator == PaintRuleLogicalOperator.And;
            var evaluated = 0;
            var matched = 0;

            if (group.Conditions != null) {
                foreach (var condition in group.Conditions) {
                    if (!IsConfigured(condition)) {
                        continue;
                    }

                    evaluated++;

                    if (ConditionMatches(condition, block)) {
                        matched++;
                    } else if (requireAll) {
                        return false;
                    }
                }
            }

            if (group.Children != null) {
                foreach (var child in group.Children) {
                    // An empty nested group carries no meaning; counting it would make an AND group
                    // impossible to satisfy.
                    if (!HasConfiguredContent(child)) {
                        continue;
                    }

                    evaluated++;

                    if (GroupMatches(child, block)) {
                        matched++;
                    } else if (requireAll) {
                        return false;
                    }
                }
            }

            if (evaluated == 0) {
                return false;
            }

            return requireAll ? matched == evaluated : matched > 0;
        }

        /// <summary>
        ///     True when the group, or any group nested inside it, holds at least one configured condition.
        /// </summary>
        private static bool HasConfiguredContent(PaintRuleConditionGroup group) {
            if (group == null) {
                return false;
            }

            if (group.Conditions != null && group.Conditions.Any(IsConfigured)) {
                return true;
            }

            return group.Children != null && group.Children.Any(HasConfiguredContent);
        }

        private static bool IsConfigured(PaintRuleCondition condition) {
            if (condition == null) {
                return false;
            }

            switch (condition.Type) {
                case PaintRuleConditionType.BlockColor:
                    return condition.Color.Enabled;
                case PaintRuleConditionType.BlockDefinition:
                    return condition.Definition.Enabled;
                case PaintRuleConditionType.BlockSkin:
                    return condition.Skin.Enabled;
                default:
                    return false;
            }
        }

        private static bool ConditionMatches(PaintRuleCondition condition, IMySlimBlock block) {
            bool matches;

            switch (condition.Type) {
                case PaintRuleConditionType.BlockColor:
                    matches = ColorMatches(condition.Color.Value, block);
                    break;
                case PaintRuleConditionType.BlockDefinition:
                    matches = DefinitionMatches(condition.Definition, block);
                    break;
                case PaintRuleConditionType.BlockSkin:
                    matches = SkinMatches(condition.Skin, block);
                    break;
                default:
                    return false;
            }

            return condition.Comparison == PaintRuleComparison.NotEquals ? !matches : matches;
        }

        private static bool ColorMatches(ColorModel expected, IMySlimBlock block) {
            ColorModel actual = block.GetColorMask();

            return Math.Abs(actual.R - expected.R) <= COLOR_CHANNEL_TOLERANCE
                && Math.Abs(actual.G - expected.G) <= COLOR_CHANNEL_TOLERANCE
                && Math.Abs(actual.B - expected.B) <= COLOR_CHANNEL_TOLERANCE;
        }

        private static bool DefinitionMatches(PaintRuleDefinitionValue expected, IMySlimBlock block) {
            if (block.BlockDefinition == null) {
                return false;
            }

            var definitionId = block.BlockDefinition.Id;

            var typeMatches = string.IsNullOrWhiteSpace(expected.TypeId)
                || string.Equals(definitionId.TypeId.ToString(), expected.TypeId, StringComparison.OrdinalIgnoreCase);

            var subtypeMatches = string.IsNullOrWhiteSpace(expected.SubtypeId)
                || string.Equals(definitionId.SubtypeName, expected.SubtypeId, StringComparison.OrdinalIgnoreCase);

            return typeMatches && subtypeMatches;
        }

        private static bool SkinMatches(PaintRuleSkinValue expected, IMySlimBlock block) {
            return string.Equals(GetSkinId(block), expected.SkinId ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetSkinId(IMySlimBlock block) {
            return block.SkinSubtypeId.String ?? string.Empty;
        }

        /// <summary>
        ///     Applies the action of a matching rule to a block. Returns true when the block actually changed.
        /// </summary>
        private static bool ApplyAction(IMySlimBlock block, PaintRuleAction action) {
            if (action == null || (!action.ApplyColor && !action.ApplySkin)) {
                return false;
            }

            var grid = block.CubeGrid;
            if (grid == null) {
                return false;
            }

            var changed = false;

            if (action.ApplyColor) {
                Vector3 targetMask = action.TargetColor;
                if (!ColorMatches(action.TargetColor, block)) {
                    grid.ColorBlocks(block.Min, block.Max, targetMask);
                    changed = true;
                }
            }

            if (action.ApplySkin) {
                var targetSkin = action.TargetSkin.SkinId ?? string.Empty;
                if (!string.Equals(GetSkinId(block), targetSkin, StringComparison.OrdinalIgnoreCase)) {
                    grid.SkinBlocks(block.Min, block.Max, null, targetSkin);
                    changed = true;
                }
            }

            return changed;
        }

        /// <summary>
        ///     Builds the list of grids a job should be applied to, based on its options.
        /// </summary>
        private static List<IMyCubeGrid> CollectGrids(IMyCubeGrid target, PaintJobOptions options) {
            var candidates = new List<IMyCubeGrid>();

            if (options.IncludeSubgrids) {
                var group = target.GetGridGroup(GridLinkTypeEnum.Mechanical);
                group?.GetGrids(candidates);
            }

            if (candidates.Count == 0) {
                candidates.Add(target);
            }

            var result = new List<IMyCubeGrid>();
            foreach (var grid in candidates) {
                // Grids without physics are projections or placement previews.
                if (grid == null || (grid.Physics == null && !options.IncludePreviewGrids)) {
                    continue;
                }

                result.Add(grid);
            }

            if (options.IncludeProjectedGrids) {
                foreach (var grid in result.ToArray()) {
                    AddProjectedGrids(grid, result);
                }
            }

            return result;
        }

        private static void AddProjectedGrids(IMyCubeGrid grid, List<IMyCubeGrid> result) {
            var projectors = new List<IMySlimBlock>();
            grid.GetBlocks(projectors, block => block.FatBlock is IMyProjector);

            foreach (var slimBlock in projectors) {
                var projector = slimBlock.FatBlock as IMyProjector;
                var projectedGrid = projector?.ProjectedGrid;

                if (projectedGrid != null && !result.Contains(projectedGrid)) {
                    result.Add(projectedGrid);
                }
            }
        }

        /// <summary>
        ///     Returns true when the player owns the grid, shares a faction with its owner, or the grid is unowned.
        /// </summary>
        private static bool IsModifiableBy(IMyPlayer player, IMyCubeGrid grid) {
            var owners = grid.BigOwners;
            if (owners == null || owners.Count == 0) {
                return true;
            }

            foreach (var owner in owners) {
                var relation = player.GetRelationTo(owner);
                if (relation == MyRelationsBetweenPlayerAndBlock.Owner || relation == MyRelationsBetweenPlayerAndBlock.FactionShare) {
                    return true;
                }
            }

            return false;
        }

        private static void ReportResult(PaintJob job, IMyCubeGrid grid, int matchedBlocks, int changedBlocks, int skippedGrids) {
            string message;

            if (matchedBlocks == 0) {
                message = $"Paint job '{job.Name}' matched no blocks on '{grid.DisplayName}'.";
            } else if (changedBlocks == 0) {
                message = $"Paint job '{job.Name}' matched {matchedBlocks} block(s) on '{grid.DisplayName}', all already up to date.";
            } else {
                message = $"Paint job '{job.Name}' updated {changedBlocks} of {matchedBlocks} matching block(s) on '{grid.DisplayName}'.";
            }

            if (skippedGrids > 0) {
                message += $" {skippedGrids} grid(s) skipped due to ownership.";
            }

            MyAPIGateway.Utilities.ShowMessage(Mod.NAME, message);
        }

        private IMyCubeGrid GetTargetedGrid(IMyPlayer player) {
            var character = player.Character;
            if (character?.Physics == null) {
                return null;
            }

            var headMatrix = character.GetHeadMatrix(true, true, false, false);
            var start = headMatrix.Translation;
            var end = start + headMatrix.Forward * RAY_LENGTH;

            var hits = new List<IHitInfo>();
            MyAPIGateway.Physics.CastRay(start, end, hits);

            if (hits.Count == 0) {
                return null;
            }

            var gridHit = hits
                .Select(hit => hit.HitEntity?.GetTopMostParent())
                .OfType<IMyCubeGrid>()
                .FirstOrDefault();

            return gridHit;
        }
    }
}
