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
    /// Runtime helper that encapsulates persistence and execution of paint jobs.
    /// </summary>
    public class PaintJobService {
        private const float RAY_LENGTH = 120f;

        private readonly PaintHistory _history = new PaintHistory();
        private readonly Mod _mod;

        public PaintJobService(Mod mod) {
            _mod = mod;
        }

        internal PaintHistory History {
            get { return _history; }
        }

        /// <summary>
        /// Number of applications that can still be put back.
        /// </summary>
        public int UndoCount {
            get { return _history.Entries.Count; }
        }

        /// <summary>
        /// The job the hotkeys act on, or null when there are none.
        /// </summary>
        public PaintJob ActiveJob {
            get {
                var jobs = OrderedJobs();
                if (jobs.Length == 0) {
                    return null;
                }

                var id = _mod.PaintJobs.ActiveJobId;
                foreach (var job in jobs) {
                    if (job.Id == id) {
                        return job;
                    }
                }

                return jobs[0];
            }
        }

        public void SetActiveJob(PaintJob job) {
            var id = job != null ? job.Id : Guid.Empty;
            if (_mod.PaintJobs == null || _mod.PaintJobs.ActiveJobId == id) {
                return;
            }

            _mod.PaintJobs.ActiveJobId = id;
            _mod.SavePaintJobs();
        }

        /// <summary>
        /// Moves the active job on by one, wrapping around, and returns what it landed on.
        /// </summary>
        public PaintJob CycleActiveJob(int offset) {
            var jobs = OrderedJobs();
            if (jobs.Length == 0) {
                return null;
            }

            var current = ActiveJob;
            var index = Array.IndexOf(jobs, current);
            var next = jobs[((index + offset) % jobs.Length + jobs.Length) % jobs.Length];

            SetActiveJob(next);

            return next;
        }

        /// <summary>
        /// Applies the active job.
        /// </summary>
        public void ApplyActiveJob() {
            var job = ActiveJob;

            if (job == null) {
                MyAPIGateway.Utilities.ShowMessage(Mod.NAME, ModText.BC_NoPaintJobsAvailable.GetString());
                return;
            }

            ApplyJobToSelection(job);
        }

        /// <summary>
        /// The order jobs are listed in everywhere, so a position means the same thing in the panel and in chat.
        /// </summary>
        public static IEnumerable<PaintJob> InNameOrder(IEnumerable<PaintJob> jobs) {
            return jobs.OrderBy(job => job.Name, StringComparer.InvariantCultureIgnoreCase);
        }

        private PaintJob[] OrderedJobs() {
            if (_mod.PaintJobs == null || _mod.PaintJobs.Count == 0) {
                return new PaintJob[0];
            }

            return InNameOrder(_mod.PaintJobs).ToArray();
        }

        /// <summary>
        /// The given name when it is free, otherwise the first numbered copy of it that is.
        /// </summary>
        public string UniqueJobName(string name) {
            if (string.IsNullOrEmpty(name)) {
                name = ModText.BC_UI_NewPaintJobName.GetString();
            }

            var jobs = GetJobs();

            if (!jobs.Any(job => string.Equals(job.Name, name, StringComparison.InvariantCultureIgnoreCase))) {
                return name;
            }

            var candidate = ModText.BC_UI_CopyOfName.GetString(name);
            var index = 2;

            while (jobs.Any(job => string.Equals(job.Name, candidate, StringComparison.InvariantCultureIgnoreCase))) {
                candidate = string.Format("{0} {1}", ModText.BC_UI_CopyOfName.GetString(name), index);
                index++;
            }

            return candidate;
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

        /// <summary>
        /// Writes the jobs out as they stand.
        /// </summary>
        public void Save() {
            _mod.SavePaintJobs();
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
        /// The colors on the grid the player is aiming at, most used first.
        /// </summary>
        public Settings.Models.ColorMask[] GetTargetedGridColors(int count) {
            var player = MyAPIGateway.Session?.LocalHumanPlayer;
            var grid = player != null ? GetTargetedGrid(player) : null;

            if (grid == null) {
                return null;
            }

            var blocks = new List<IMySlimBlock>();
            grid.GetBlocks(blocks);

            var tally = new Dictionary<Vector3I, int>();
            var samples = new Dictionary<Vector3I, Vector3>();

            foreach (var block in blocks) {
                var mask = block.ColorMaskHSV;

                var key = new Vector3I((int)Math.Round(mask.X * 64f), (int)Math.Round(mask.Y * 32f), (int)Math.Round(mask.Z * 32f));

                int used;
                tally[key] = tally.TryGetValue(key, out used) ? used + 1 : 1;
                samples[key] = mask;
            }

            return tally
                .OrderByDescending(entry => entry.Value)
                .Take(count)
                .Select(entry => (Settings.Models.ColorMask)samples[entry.Key])
                .ToArray();
        }

        /// <summary>
        /// Applies a paint job to the grid the player is currently aiming at.
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
            var matches = new List<MatchedBlock>();
            var batch = new GridPaintBatch();
            var snapshot = new PaintSnapshotBuilder();

            foreach (var grid in grids) {
                if (!IsModifiableBy(player, grid)) {
                    skippedGrids++;
                    continue;
                }

                blocks.Clear();
                grid.GetBlocks(blocks);

                var context = GridPaintContext.Build(grid);

                matches.Clear();
                compiledJob.BeginGrid();

                foreach (var block in blocks) {
                    if (block == null || block.IsDestroyed) {
                        continue;
                    }

                    BlockFacts facts;
                    var ruleIndex = compiledJob.FindRule(block, ref context, out facts);
                    if (ruleIndex < 0) {
                        continue;
                    }

                    compiledJob.Observe(ruleIndex, ref facts);
                    matches.Add(new MatchedBlock { Block = block, RuleIndex = ruleIndex });
                }

                compiledJob.EndGrid();

                matchedBlocks += matches.Count;
                batch.Begin(grid);

                foreach (var match in matches) {
                    var facts = BlockFacts.Read(match.Block, ref context);

                    PaintResolution resolution;
                    compiledJob.Resolve(match.RuleIndex, ref facts, out resolution);

                    BlockPaintState previous;
                    if (!batch.Add(match.Block, ref resolution, out previous)) {
                        continue;
                    }

                    changedBlocks++;
                    snapshot.Record(grid, ref previous);
                }

                batch.Flush();
            }

            if (snapshot.HasChanges) {
                _history.Push(snapshot.Build(job.Name));
            }

            ReportResult(job, targetedGrid, matchedBlocks, changedBlocks, skippedGrids);
        }

        /// <summary>
        /// Puts back one application of a paint job, counted from the most recent at 1.
        /// </summary>
        public void UndoPaintJob(int position = 1) {
            var player = MyAPIGateway.Session?.LocalHumanPlayer;
            if (player == null) {
                MyAPIGateway.Utilities.ShowMessage(Mod.NAME, ModText.BC_PaintJob_NoLocalPlayer.GetString());
                return;
            }

            var entry = _history.Take(position);
            if (entry == null) {
                MyAPIGateway.Utilities.ShowMessage(Mod.NAME, ModText.BC_PaintJob_NothingToUndo.GetString());
                return;
            }

            var batch = new GridPaintBatch();
            var restoredBlocks = 0;
            var missingGrids = 0;
            var skippedGrids = 0;

            foreach (var snapshot in entry.Grids) {
                var grid = MyAPIGateway.Entities.GetEntityById(snapshot.GridId) as IMyCubeGrid;
                if (grid == null || grid.MarkedForClose) {
                    missingGrids++;
                    continue;
                }

                if (!IsModifiableBy(player, grid)) {
                    skippedGrids++;
                    continue;
                }

                batch.Begin(grid);

                foreach (var state in snapshot.Blocks) {
                    var block = grid.GetCubeBlock(state.Position);
                    if (block == null || block.IsDestroyed) {
                        continue;
                    }

                    var resolution = new PaintResolution {
                        ApplyColor = state.RestoreColor,
                        ApplySkin = state.RestoreSkin,
                        Mask = state.Mask,
                        SkinId = state.SkinId ?? string.Empty
                    };

                    BlockPaintState overwritten;
                    if (batch.Add(block, ref resolution, out overwritten)) {
                        restoredBlocks++;
                    }
                }

                batch.Flush();
            }

            var message = restoredBlocks > 0
                ? ModText.BC_PaintJob_Undone.GetString(entry.JobName, restoredBlocks)
                : ModText.BC_PaintJob_UndoNothingLeft.GetString(entry.JobName);

            if (missingGrids > 0) {
                message += ModText.BC_PaintJob_UndoGridsGone.GetString(missingGrids);
            }

            if (skippedGrids > 0) {
                message += ModText.BC_PaintJob_GridsSkipped.GetString(skippedGrids);
            }

            MyAPIGateway.Utilities.ShowMessage(Mod.NAME, message);
        }

        /// <summary>
        /// Builds the list of grids a job should be applied to, based on its options.
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
        /// Returns true when the player has creative rights, owns the grid, shares a faction with its owner, or the grid is unowned.
        /// </summary>
        private static bool IsModifiableBy(IMyPlayer player, IMyCubeGrid grid) {
            if (MyAPIGateway.Session != null && MyAPIGateway.Session.HasCreativeRights) {
                return true;
            }

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

        /// <summary>
        /// A block and the rule that claimed it, carried from the matching pass to the painting pass.
        /// </summary>
        private struct MatchedBlock {
            public IMySlimBlock Block;
            public int RuleIndex;
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
