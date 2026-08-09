using RichHudFramework.UI;
using RichHudFramework.UI.Rendering;
using VRage.Utils;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Dropdown row showing an armor skin icon next to its name. Used as the element type of the skin
    /// dropdowns, which is why it needs a parameterless constructor and exposes a text board.
    /// </summary>
    public class SkinListEntry : HudElementBase, IMinLabelElement {
        private const float ICON_SIZE = 24f;
        private const float ICON_SPACING = 8f;

        private readonly Label _label;
        private readonly TexturedBox _icon;

        public SkinListEntry() : this(null) { }

        public SkinListEntry(HudParentBase parent = null) : base(parent) {
            Height = ICON_SIZE;

            _icon = new TexturedBox() {
                Width = ICON_SIZE,
                Height = ICON_SIZE,
                Color = VRageMath.Color.White,
                Visible = false,
            };

            _label = new Label() {
                AutoResize = false,
                Height = ICON_SIZE,
            };

            // No FitMembersOffAxis here: the list box overrides row height, and stretching the icon to it
            // would distort a square texture.
            var layout = new HudChain(false, this) {
                CollectionContainer = { { _icon, 0f }, { _label, 1f } },
                Spacing = ICON_SPACING,
                DimAlignment = DimAlignments.Size,
                ParentAlignment = ParentAlignments.Inner,
            };
        }

        public ITextBoard TextBoard {
            get { return _label.TextBoard; }
        }

        /// <summary>
        /// Shows the icon of the given transparent material, or hides it when there is none. Skins from
        /// other mods have no icon material shipped with this mod and simply show as text.
        /// </summary>
        public void SetIcon(string materialSubtype) {
            if (string.IsNullOrEmpty(materialSubtype)) {
                _icon.Visible = false;
                return;
            }

            _icon.Material = new Material(MyStringId.GetOrCompute(materialSubtype), new Vector2(ICON_SIZE, ICON_SIZE));
            _icon.Visible = true;
        }
    }
}
