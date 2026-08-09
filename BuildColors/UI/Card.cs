using RichHudFramework.UI;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// A titled surface that holds one group of controls.
    /// </summary>
    internal class Card : HudElementBase {
        /// <summary>
        /// The chain the card's controls go into.
        /// </summary>
        public readonly HudChain Content;

        private readonly Label _title;

        public Card(string title, float width, float height, HudParentBase parent = null) : base(parent) {
            Size = new Vector2(width, height);

            var background = new TexturedBox(this) {
                DimAlignment = DimAlignments.Both,
                Color = Style.CardBackgroundColor,
            };

            new BorderBox(background) {
                DimAlignment = DimAlignments.Both,
                Color = Style.SubtleBorderColor,
                Thickness = 1f,
            };

            var innerWidth = ContentWidth(width);
            var innerHeight = height - LayoutMetrics.CARD_PADDING * 2f;

            var layout = new HudChain(true, this) {
                ParentAlignment = ParentAlignments.Inner,
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = innerWidth,
                Height = innerHeight,
            };

            if (title != null) {
                _title = ControlFactory.CreateHeading(title, innerWidth);

                layout.Add(_title, 0f);
                innerHeight -= LayoutMetrics.HEADING_HEIGHT + LayoutMetrics.ROW_SPACING;
            }

            Content = ControlFactory.CreateColumn(innerWidth, innerHeight, LayoutMetrics.ROW_SPACING);
            layout.Add(Content, 0f);
        }

        /// <summary>
        /// The card's heading.
        /// </summary>
        public string Title {
            set {
                if (_title != null) {
                    _title.Text = value ?? string.Empty;
                }
            }
        }

        /// <summary>
        /// Width a card of the given total width leaves for its contents.
        /// </summary>
        public static float ContentWidth(float cardWidth) {
            return cardWidth - LayoutMetrics.CARD_PADDING * 2f;
        }

        /// <summary>
        /// Height a card needs to hold contents of the given height, title included.
        /// </summary>
        public static float HeightFor(float contentHeight, bool hasTitle = true) {
            var chrome = LayoutMetrics.CARD_PADDING * 2f;

            if (hasTitle) {
                chrome += LayoutMetrics.HEADING_HEIGHT + LayoutMetrics.ROW_SPACING;
            }

            return contentHeight + chrome;
        }

        /// <summary>
        /// Height the contents of a card of the given total height may take.
        /// </summary>
        public static float ContentHeight(float cardHeight, bool hasTitle = true) {
            return cardHeight - (HeightFor(0f, hasTitle));
        }
    }
}
