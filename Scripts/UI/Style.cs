using RichHudFramework.UI;
using VRageMath;

namespace Sisk.BuildColors.UI {
    internal static class Style {
        public static readonly Color BodyBackgroundColor = new Color(37, 46, 53);
        public static readonly Color BodyTextColor = new Color(242, 242, 242);
        public static readonly Color SelectionBackgroundColor = new Color(60, 76, 82);
        public static readonly Color SeparatorColor = new Color(77, 99, 113);
        public static readonly Color BorderColor = new Color(55, 66, 83);

        public static readonly Color ButtonBackgroundColor = new Color(41, 54, 62);
        public static readonly Color ButtonTextColor = new Color(201, 223, 230);
        public static readonly Color ButtonBorderColor = new Color(62, 71, 79);
        public static readonly Color ButtonHighlightBorderColor = new Color(112, 117, 122);
        public static readonly Color ButtonHighlightBackgroundColor = new Color(60, 76, 82);

        public static readonly GlyphFormat HeaderText = new GlyphFormat(BodyTextColor, TextAlignment.Center, 1.25f);
        public static readonly GlyphFormat BodyText = HeaderText.WithAlignment(TextAlignment.Left).WithSize(.885f);
        public static readonly GlyphFormat FooterText = BodyText.WithAlignment(TextAlignment.Center);
        public static readonly GlyphFormat ButtonText = new GlyphFormat(ButtonTextColor, TextAlignment.Center, 1.25f);
    }
}