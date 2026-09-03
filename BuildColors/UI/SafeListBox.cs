using RichHudFramework.UI;
using VRageMath;

namespace Sisk.BuildColors.UI {

    internal class SafeListBox<TValue> : ListBox<TValue> {
        public SafeListBox(HudParentBase parent = null) : base(parent) { }

        public new void ClearEntries() {
            base.ClearEntries();
            ClampVisibleRange();
        }

        public new bool Remove(ListBoxEntry<TValue> entry) {
            var removed = base.Remove(entry);
            ClampVisibleRange();

            return removed;
        }

        public new void RemoveAt(int index) {
            base.RemoveAt(index);
            ClampVisibleRange();
        }

        public new void RemoveRange(int index, int count) {
            base.RemoveRange(index, count);
            ClampVisibleRange();
        }

        protected override void Layout() {
            base.Layout();
            ClampVisibleRange();
        }

        private void ClampVisibleRange() {
            var last = Count - 1;
            var range = listInput.ListRange;

            listInput.ListRange = new Vector2I(
                MathHelper.Clamp(range.X, 0, last),
                MathHelper.Clamp(range.Y, 0, last));

            EntryChain.End = MathHelper.Clamp(EntryChain.End, 0, last);
            EntryChain.Start = MathHelper.Clamp(EntryChain.Start, 0, last);
        }
    }
}
