using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRageMath;

namespace Sisk.BuildColors.Services {

    /// <summary>
    ///     What one block looked like before a job touched it. Only the channels the job actually wrote are
    ///     marked for restoring, so undoing a job that only changed skins leaves later color work alone.
    /// </summary>
    internal struct BlockPaintState {
        public Vector3 Mask;
        public Vector3I Position;
        public bool RestoreColor;
        public bool RestoreSkin;
        public string SkinId;
    }

    /// <summary>
    ///     The blocks of one grid a job changed. Grids are held by id rather than by reference so that a
    ///     history entry cannot keep a deleted grid alive.
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
    ///     One application of a paint job, in the form it takes to put it back.
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
        ///     Whether the job was applied with ownership respected. Undoing must not become a way around a
        ///     check the job itself honoured.
        /// </summary>
        public bool RespectOwnership { get; private set; }

        public string GridName {
            get { return Grids.Count > 0 ? Grids[0].GridName : string.Empty; }
        }
    }

    /// <summary>
    ///     Gathers the previous state of every block a run changes.
    /// </summary>
    internal sealed class PaintSnapshotBuilder {
        private readonly List<GridPaintSnapshot> _grids = new List<GridPaintSnapshot>();

        private int _blockCount;
        private GridPaintSnapshot _current;

        public bool HasChanges {
            get { return _blockCount > 0; }
        }

        public void Record(IMyCubeGrid grid, ref BlockPaintState state) {
            // Grids are painted one after another, so the snapshot being filled is always the last one.
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
    ///     The paint jobs applied this session, newest last, so any of them can be put back. Held in memory
    ///     only: a snapshot is worth a few hundred kilobytes for a large build, and it stops being valid as
    ///     soon as the world is reloaded and blocks have moved on without it.
    /// </summary>
    internal sealed class PaintHistory {

        /// <summary>
        ///     Ceiling on the blocks all entries hold together. Old entries are dropped to stay under it, so a
        ///     couple of jobs over a capital ship cannot quietly grow into tens of megabytes.
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
        ///     Removes and returns an entry, counted from the newest at 1. An entry that has been put back is
        ///     no longer something that can be put back, so it leaves the history either way.
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
