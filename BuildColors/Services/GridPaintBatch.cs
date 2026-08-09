using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRageMath;

namespace Sisk.BuildColors.Services {

    /// <summary>
    /// Collects the blocks a job changes on one grid and paints them in as few calls as it can.
    /// </summary>
    internal sealed class GridPaintBatch {
        private readonly List<PaintGroup> _groups = new List<PaintGroup>();
        private readonly List<Vector3I> _run = new List<Vector3I>();

        private IMyCubeGrid _grid;

        public void Begin(IMyCubeGrid grid) {
            _grid = grid;
            _groups.Clear();
        }

        /// <summary>
        /// Queues a block.
        /// </summary>
        public bool Add(IMySlimBlock block, ref PaintResolution resolution, out BlockPaintState previous) {
            var currentMask = block.GetColorMask();
            var currentSkin = block.SkinSubtypeId.String ?? string.Empty;

            var needsColor = resolution.ApplyColor && !PaintColorMath.MaskEquals(currentMask, resolution.Mask);
            var needsSkin = resolution.ApplySkin
                && !string.Equals(currentSkin, resolution.SkinId ?? string.Empty, StringComparison.OrdinalIgnoreCase);

            previous = new BlockPaintState {
                Position = block.Min,
                Mask = currentMask,
                SkinId = currentSkin,
                RestoreColor = needsColor,
                RestoreSkin = needsSkin
            };

            if (!needsColor && !needsSkin) {
                return false;
            }

            var applied = new PaintResolution {
                ApplyColor = needsColor,
                ApplySkin = needsSkin,
                Mask = resolution.Mask,
                SkinId = resolution.SkinId ?? string.Empty
            };

            GetGroup(ref applied).Add(block);

            return true;
        }

        public void Flush() {
            if (_grid == null) {
                return;
            }

            foreach (var group in _groups) {
                PaintGroupBlocks(group);
            }

            _groups.Clear();
            _grid = null;
        }

        private PaintGroup GetGroup(ref PaintResolution resolution) {
            for (var i = 0; i < _groups.Count; i++) {
                if (_groups[i].Resolution.Matches(ref resolution)) {
                    return _groups[i];
                }
            }

            var group = new PaintGroup(resolution);
            _groups.Add(group);

            return group;
        }

        private void PaintGroupBlocks(PaintGroup group) {
            foreach (var block in group.MultiCellBlocks) {
                Paint(block.Min, block.Max, group.Resolution);
            }

            var cells = group.Cells;
            if (cells.Count == 0) {
                return;
            }

            cells.Sort(CompareCells);

            _run.Clear();

            foreach (var cell in cells) {
                if (_run.Count > 0 && !ContinuesRun(_run[_run.Count - 1], cell)) {
                    PaintRun(group.Resolution);
                }

                _run.Add(cell);
            }

            PaintRun(group.Resolution);
        }

        private void PaintRun(PaintResolution resolution) {
            if (_run.Count == 0) {
                return;
            }

            Paint(_run[0], _run[_run.Count - 1], resolution);
            _run.Clear();
        }

        private void Paint(Vector3I min, Vector3I max, PaintResolution resolution) {
            if (resolution.ApplySkin) {
                _grid.SkinBlocks(min, max, resolution.ApplyColor ? resolution.Mask : (Vector3?)null, resolution.SkinId ?? string.Empty);
                return;
            }

            if (resolution.ApplyColor) {
                _grid.ColorBlocks(min, max, resolution.Mask);
            }
        }

        private static bool ContinuesRun(Vector3I previous, Vector3I next) {
            return previous.Y == next.Y && previous.Z == next.Z && next.X == previous.X + 1;
        }

        private static int CompareCells(Vector3I left, Vector3I right) {
            if (left.Y != right.Y) {
                return left.Y.CompareTo(right.Y);
            }

            return left.Z != right.Z ? left.Z.CompareTo(right.Z) : left.X.CompareTo(right.X);
        }

        /// <summary>
        /// Blocks that end up with the same paint.
        /// </summary>
        private sealed class PaintGroup {
            public PaintGroup(PaintResolution resolution) {
                Resolution = resolution;
            }

            public List<Vector3I> Cells { get; } = new List<Vector3I>();

            public List<IMySlimBlock> MultiCellBlocks { get; } = new List<IMySlimBlock>();

            public PaintResolution Resolution;

            public void Add(IMySlimBlock block) {
                if (block.Min == block.Max) {
                    Cells.Add(block.Min);
                } else {
                    MultiCellBlocks.Add(block);
                }
            }
        }
    }
}
