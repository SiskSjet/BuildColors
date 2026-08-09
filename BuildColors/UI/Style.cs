using RichHudFramework.UI;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// The mod's design tokens: one palette and one type scale that every control is built from.
    /// </summary>
    internal static class Style {
        /// <summary>
        /// The panel and dialog body.
        /// </summary>
        public static readonly Color BodyBackgroundColor = new Color(32, 40, 48);

        /// <summary>
        /// Title bars and the navigation rail, a step darker than the body so the chrome reads as chrome.
        /// </summary>
        public static readonly Color ChromeBackgroundColor = new Color(25, 32, 39);

        /// <summary>
        /// Lists, text fields and anything else the eye should read as a hole in the surface.
        /// </summary>
        public static readonly Color SunkenBackgroundColor = new Color(22, 28, 34);

        /// <summary>
        /// Grouping cards, a step lighter than the body.
        /// </summary>
        public static readonly Color CardBackgroundColor = new Color(40, 50, 59);

        public static readonly Color BorderColor = new Color(58, 72, 85);
        public static readonly Color SubtleBorderColor = new Color(45, 56, 66);
        public static readonly Color SeparatorColor = new Color(52, 65, 77);

        /// <summary>
        /// The single accent.
        /// </summary>
        public static readonly Color AccentColor = new Color(96, 180, 222);
        public static readonly Color AccentBackgroundColor = new Color(38, 84, 110);
        public static readonly Color AccentHighlightColor = new Color(52, 110, 142);

        public static readonly Color DangerColor = new Color(198, 86, 86);
        public static readonly Color DangerHighlightColor = new Color(108, 52, 56);

        public static readonly Color BodyTextColor = new Color(232, 238, 242);
        public static readonly Color MutedTextColor = new Color(150, 166, 178);
        public static readonly Color DisabledTextColor = new Color(104, 118, 130);

        public static readonly Color ButtonBackgroundColor = new Color(44, 55, 65);
        public static readonly Color ButtonBorderColor = new Color(70, 86, 100);
        public static readonly Color ButtonHighlightBackgroundColor = new Color(60, 76, 90);
        public static readonly Color ButtonTextColor = new Color(214, 226, 234);

        public static readonly Color SelectionBackgroundColor = new Color(38, 84, 110);
        public static readonly Color HoverBackgroundColor = new Color(48, 60, 72);

        public static readonly Color ScrollBarColor = new Color(30, 38, 45);
        public static readonly Color ScrollSliderColor = new Color(64, 79, 92);
        public static readonly Color ScrollSliderHighlightColor = new Color(88, 108, 124);

        /// <summary>
        /// The panel title.
        /// </summary>
        public static readonly GlyphFormat TitleText = new GlyphFormat(BodyTextColor, TextAlignment.Left, 1.25f);

        /// <summary>
        /// Section headings inside a view.
        /// </summary>
        public static readonly GlyphFormat HeadingText = new GlyphFormat(BodyTextColor, TextAlignment.Left, 1f);

        public static readonly GlyphFormat BodyText = new GlyphFormat(BodyTextColor, TextAlignment.Left, .885f);

        /// <summary>
        /// Field captions, hints and anything that explains a control rather than being one.
        /// </summary>
        public static readonly GlyphFormat CaptionText = new GlyphFormat(MutedTextColor, TextAlignment.Left, .82f);

        public static readonly GlyphFormat ButtonText = new GlyphFormat(ButtonTextColor, TextAlignment.Center, .95f);

        /// <summary>
        /// Dialog title bars, which are centered where the panel title is not.
        /// </summary>
        public static readonly GlyphFormat HeaderText = TitleText.WithAlignment(TextAlignment.Center);
    }
}
