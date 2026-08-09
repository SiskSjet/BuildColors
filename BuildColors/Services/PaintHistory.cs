using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRageMath;

namespace Sisk.BuildColors.Services {

    /// <summary>
    /// What one block looked like before a job touched it.
    /// </summary>
    internal struct BlockPaintState {
        public Vector3 Mask;
        public Vector3I Position;
        public bool RestoreColor;
        public bool RestoreSkin;
        public string SkinId;
    }

    /// <summary>
    /// The blocks of one grid a job changed.
    /// </summary>
    internal sealed class GridPaintSnapshot {
        public GridPaintSnapshot(long gridId, string gridName) {
            GridId = gridId;
            GridName = gridName;
        }

        public List<BlockPaintState> Blocks { get; } = new List<BlockPaintState>();

        public long GridId { get; private set; }

        public string GridName { get; private set; }
    }

    /// <summary>
    /// One application of a paint job, in the form it takes to put it back.
    /// </summary>
    internal sealed class PaintHistoryEntry {
        public PaintHistoryEntry(string jobName, bool respectOwnership, List<GridPaintSnapshot> grids, int blockCount) {
            JobName = jobName;
            RespectOwnership = respectOwnership;
            Grids = grids;
            BlockCount = blockCount;
        }

        public int BlockCount { get; private set; }

        public List<GridPaintSnapshot> Grids { get; private set; }

        public string JobName { get; private set; }

        /// <summary>
        /// Whether the job was applied with ownership respected.
        /// </summary>
        public bool RespectOwnership { get; private set; }

        public string GridName {
            get { return Grids.Count > 0 ? Grids[0].GridName : string.Empty; }
        }
    }

    /// <summary>
    /// Gathers the previous state of every block a run changes.
    /// </summary>
    internal sealed class PaintSnapshotBuilder {
        private readonly List<GridPaintSnapshot> _grids = new List<GridPaintSnapshot>();

        private int _blockCount;
        private GridPaintSnapshot _current;

        public bool HasChanges {
            get { return _blockCount > 0; }
        }

        public void Record(IMyCubeGrid grid, ref BlockPaintState state) {
            if (_current == null || _current.GridId != grid.EntityId) {
                _current = new GridPaintSnapshot(grid.EntityId, grid.DisplayName);
                _grids.Add(_current);
            }

            _current.Blocks.Add(state);
            _blockCount++;
        }

        public PaintHistoryEntry Build(string jobName, bool respectOwnership) {
            return new PaintHistoryEntry(jobName, respectOwnership, _grids, _blockCount);
        }
    }

    /// <summary>
    /// The paint jobs applied this session, newest last, so any of them can be put back.
    /// </summary>
    internal sealed class PaintHistory {
        /// <summary>
        /// Ceiling on the blocks all entries hold together.
        /// </summary>
        private const int MAX_BLOCKS = 250000;

        private const int MAX_ENTRIES = 10;

        private readonly List<PaintHistoryEntry> _entries = new List<PaintHistoryEntry>();

        private int _blockCount;

        public IReadOnlyList<PaintHistoryEntry> Entries {
            get { return _entries; }
        }

        public void Push(PaintHistoryEntry entry) {
            _entries.Add(entry);
            _blockCount += entry.BlockCount;

            while (_entries.Count > MAX_ENTRIES || (_blockCount > MAX_BLOCKS && _entries.Count > 1)) {
                _blockCount -= _entries[0].BlockCount;
                _entries.RemoveAt(0);
            }
        }

        /// <summary>
        /// Removes and returns an entry, counted from the newest at 1.
        /// </summary>
        public PaintHistoryEntry Take(int position) {
            var index = _entries.Count - position;
            if (index < 0 || index >= _entries.Count) {
                return null;
            }

            var entry = _entries[index];

            _entries.RemoveAt(index);
            _blockCount -= entry.BlockCount;

            return entry;
        }

        public void Clear() {
            _entries.Clear();
            _blockCount = 0;
        }
    }
}
