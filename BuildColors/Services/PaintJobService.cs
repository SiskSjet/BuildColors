using Sandbox.ModAPI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using Sisk.Utils.Localization.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace Sisk.BuildColors.Services {

    /// <summary>
    ///     Runtime helper that encapsulates persistence and execution of paint jobs.
    /// </summary>
    public class PaintJobService {

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
                while (_mod.PaintJobs.Any(r => string.Equals(r.Name, ModText.BC_UI_DefaultPaintJobName.GetString(index), StringComparison.InvariantCultureIgnoreCase))) {
                    index++;
                }

                job.Name = ModText.BC_UI_DefaultPaintJobName.GetString(index);
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
                MyAPIGateway.Utilities.ShowMessage(Mod.NAME, ModText.BC_PaintJob_NoLocalPlayer.GetString());
                return;
            }

            var targetedGrid = GetTargetedGrid(player);
            if (targetedGrid == null) {
                MyAPIGateway.Utilities.ShowMessage(Mod.NAME, ModText.BC_PaintJob_NoTarget.GetString());
                return;
            }

            job.EnsureRules();
            var options = job.Options ?? new PaintJobOptions();

            // Compiled once for the whole run: the rule tree does not change while it is applied, so every
            // pattern, color and skin id is resolved here instead of per block.
            var compiledJob = CompiledPaintJob.Compile(job);
            if (compiledJob.IsEmpty) {
                MyAPIGateway.Utilities.ShowMessage(Mod.NAME, ModText.BC_PaintJob_NoMatchingRule.GetString(job.Name));
                return;
            }

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

                    var action = compiledJob.FindAction(block);
                    if (action == null) {
                        continue;
                    }

                    matchedBlocks++;
                    if (ApplyAction(block, action)) {
                        changedBlocks++;
                    }
                }
            }

            ReportResult(job, targetedGrid, matchedBlocks, changedBlocks, skippedGrids);
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
                if (!PaintColorMath.MaskEquals(block.GetColorMask(), targetMask)) {
                    grid.ColorBlocks(block.Min, block.Max, targetMask);
                    changed = true;
                }
            }

            if (action.ApplySkin) {
                var targetSkin = action.TargetSkinId ?? string.Empty;
                if (!string.Equals(block.SkinSubtypeId.String ?? string.Empty, targetSkin, StringComparison.OrdinalIgnoreCase)) {
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
                message = ModText.BC_PaintJob_NoBlocksMatched.GetString(job.Name, grid.DisplayName);
            } else if (changedBlocks == 0) {
                message = ModText.BC_PaintJob_AlreadyUpToDate.GetString(job.Name, matchedBlocks, grid.DisplayName);
            } else {
                message = ModText.BC_PaintJob_Updated.GetString(job.Name, changedBlocks, matchedBlocks, grid.DisplayName);
            }

            if (skippedGrids > 0) {
                message += ModText.BC_PaintJob_GridsSkipped.GetString(skippedGrids);
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
