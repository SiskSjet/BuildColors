using RichHudFramework.UI;
using System;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// The one place a control is built.
    /// </summary>
    internal static class ControlFactory {
        /// <summary>
        /// Widest a caption beside a control may get.
        /// </summary>
        public const float MAX_CAPTION_WIDTH = 190f;

        /// <summary>
        /// Width the vertical scrollbar occupies, taken out of a column as padding.
        /// </summary>
        public const float SCROLLBAR_WIDTH = 43f;

        /// <summary>
        /// A section heading.
        /// </summary>
        public static Label CreateHeading(string text, float width) {
            return new Label() {
                Text = text,
                Format = Style.HeadingText,
                AutoResize = false,
                Width = width,
                Height = LayoutMetrics.HEADING_HEIGHT,
            };
        }

        public static Label CreateLabel(string text, float width, float height = LayoutMetrics.LABEL_HEIGHT) {
            return new Label() {
                Text = text,
                Format = Style.BodyText,
                AutoResize = false,
                Width = width,
                Height = height,
            };
        }

        /// <summary>
        /// Muted text that explains a control rather than being one.
        /// </summary>
        public static Label CreateCaption(string text, float width, float height = LayoutMetrics.LABEL_HEIGHT) {
            return new Label() {
                Text = text,
                Format = Style.CaptionText,
                AutoResize = false,
                Width = width,
                Height = height,
            };
        }

        public static TexturedBox CreateSeparator(float width) {
            return new TexturedBox() {
                Width = width,
                Height = LayoutMetrics.SEPARATOR_HEIGHT,
                Color = Style.SeparatorColor,
            };
        }

        public static ActionButton CreateButton(string text, float width = 0f, ButtonRole role = ButtonRole.Default) {
            var button = new ActionButton(role) { Text = text };

            if (width > 0f) {
                button.Width = width;
            }

            return button;
        }

        public static GameInputBlockingTextField CreateTextField(float width, string initialText = null) {
            var field = new GameInputBlockingTextField() {
                Width = width,
                Height = LayoutMetrics.CONTROL_HEIGHT,
            };

            StyleTextField(field);
            field.Text = initialText ?? string.Empty;

            return field;
        }

        public static RangeClampedListBox<TValue> CreateList<TValue>() {
            var list = new RangeClampedListBox<TValue>();

            StyleList(list);
            list.MouseInput.CursorEntered += OnCursorEntered;

            return list;
        }

        public static RangeClampedListBox<TValue> CreateList<TValue>(float width, float height) {
            var list = CreateList<TValue>();

            list.Width = width;
            list.Height = height;

            return list;
        }

        public static Dropdown<TValue> CreateDropdown<TValue>(float width) {
            var dropdown = new Dropdown<TValue>() { Width = width, Height = LayoutMetrics.CONTROL_HEIGHT };

            StyleDropdown(dropdown);

            return dropdown;
        }

        public static Label CreateFullWidthLabel(string text) {
            return new Label() {
                Text = text,
                Format = Style.BodyText,
                AutoResize = false,
                DimAlignment = DimAlignments.Width,
                Height = LayoutMetrics.LABEL_HEIGHT,
            };
        }

        public static Label CreateFullWidthCaption(string text, float height = LayoutMetrics.LABEL_HEIGHT) {
            return new Label() {
                Text = text,
                Format = Style.CaptionText,
                AutoResize = false,
                DimAlignment = DimAlignments.Width,
                Height = height,
            };
        }

        public static TexturedBox CreateFullWidthSeparator() {
            return new TexturedBox() {
                DimAlignment = DimAlignments.Width,
                Height = LayoutMetrics.SEPARATOR_HEIGHT,
                Color = Style.SeparatorColor,
            };
        }

        public static GameInputBlockingTextField CreateFullWidthTextField() {
            var field = new GameInputBlockingTextField() {
                DimAlignment = DimAlignments.Width,
                Height = LayoutMetrics.CONTROL_HEIGHT,
            };

            StyleTextField(field);

            return field;
        }

        public static Dropdown<TValue> CreateFullWidthDropdown<TValue>() {
            var dropdown = new Dropdown<TValue>() {
                DimAlignment = DimAlignments.Width,
                Height = LayoutMetrics.CONTROL_HEIGHT,
            };

            StyleDropdown(dropdown);

            return dropdown;
        }

        public static BorderedCheckBox CreateCheckbox() {
            var checkbox = new BorderedCheckBox() {
                Color = Style.SunkenBackgroundColor,
                BorderColor = Style.ButtonBorderColor,
                TickBoxColor = Style.AccentColor,
                TickBoxHighlightColor = Style.AccentColor,
                TickBoxFocusColor = Style.AccentColor,
                FocusColor = Style.SunkenBackgroundColor,
                UseFocusFormatting = false,
            };

            checkbox.MouseInput.CursorEntered += OnCursorEntered;
            checkbox.MouseInput.LeftClicked += OnClicked;

            return checkbox;
        }

        /// <summary>
        /// Applies the field palette and clears the placeholder the framework starts every field with.
        /// </summary>
        public static void StyleTextField(TextField field) {
            field.Text = string.Empty;
            field.Format = Style.BodyText;
            field.Color = Style.SunkenBackgroundColor;
            field.BorderColor = Style.ButtonBorderColor;
            field.HighlightColor = Style.HoverBackgroundColor;
            field.FocusColor = Style.SunkenBackgroundColor;
            field.FocusTextColor = Style.BodyTextColor;
            field.UseFocusFormatting = false;
            field.TextPadding = new Vector2(10f, 0f);

            field.MouseInput.CursorEntered += OnCursorEntered;
        }

        public static void StyleDropdown<TContainer, TElement, TValue>(Dropdown<TContainer, TElement, TValue> dropdown)
            where TContainer : class, IListBoxEntry<TElement, TValue>, new()
            where TElement : HudElementBase, IMinLabelElement {
            dropdown.Format = Style.BodyText;
            dropdown.Color = Style.SunkenBackgroundColor;
            dropdown.HighlightColor = Style.HoverBackgroundColor;
            dropdown.TabColor = Style.AccentColor;

            dropdown.BarColor = Style.ScrollBarColor;
            dropdown.BarHighlight = Style.ScrollSliderHighlightColor;
            dropdown.SliderColor = Style.ScrollSliderColor;
            dropdown.SliderHighlight = Style.ScrollSliderHighlightColor;

            dropdown.MouseInput.CursorEntered += OnCursorEntered;
        }

        /// <summary>
        /// Applies the list palette.
        /// </summary>
        public static void StyleList<TContainer, TElement, TValue>(ListBox<TContainer, TElement, TValue> list)
            where TContainer : class, IListBoxEntry<TElement, TValue>, new()
            where TElement : HudElementBase, IMinLabelElement {
            list.Color = Style.SunkenBackgroundColor;
            list.Format = Style.BodyText;
            list.HighlightColor = Style.HoverBackgroundColor;
            list.FocusColor = Style.SelectionBackgroundColor;

            list.FocusTextColor = Style.BodyTextColor;
            list.TabColor = Style.AccentColor;

            list.BarColor = Style.ScrollBarColor;
            list.BarHighlight = Style.ScrollSliderHighlightColor;
            list.SliderColor = Style.ScrollSliderColor;
            list.SliderHighlight = Style.ScrollSliderHighlightColor;
        }

        /// <summary>
        /// Width of the caption in a row of the given width.
        /// </summary>
        public static float CaptionWidth(float rowWidth) {
            return Math.Min(MAX_CAPTION_WIDTH, rowWidth * .45f);
        }

        /// <summary>
        /// Width left for the control beside a caption.
        /// </summary>
        public static float ControlWidth(float rowWidth) {
            return rowWidth - CaptionWidth(rowWidth) - LayoutMetrics.ROW_SPACING;
        }

        public static HudChain CreateColumn(float width, float height, float spacing = LayoutMetrics.SECTION_SPACING) {
            return new HudChain(true) {
                Spacing = spacing,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = width,
                Height = height,
            };
        }

        /// <summary>
        /// A column that clips and offers a scrollbar instead of overlapping its own members.
        /// </summary>
        public static ScrollBox CreateScrollColumn(float width, float height, float spacing = LayoutMetrics.ROW_SPACING) {
            return new ScrollBox(true) {
                Padding = new Vector2(SCROLLBAR_WIDTH, 0f),
                Width = width,
                Height = height,
                Spacing = spacing,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                EnableScrolling = true,
            };
        }

        /// <summary>
        /// Width left for controls in a scrolling column of the given total width.
        /// </summary>
        public static float ScrollContentWidth(float columnWidth) {
            return columnWidth - SCROLLBAR_WIDTH;
        }

        public static HudChain CreateRow(float width, float height, float spacing = LayoutMetrics.ROW_SPACING) {
            return new HudChain(false) {
                Spacing = spacing,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = width,
                Height = height,
            };
        }

        /// <summary>
        /// A caption beside a control, which takes whatever width the caption leaves.
        /// </summary>
        public static HudChain CreateControlRow(string caption, HudElementBase control, float width) {
            var captionLabel = CreateCaption(caption, CaptionWidth(width), LayoutMetrics.CONTROL_HEIGHT);

            var row = CreateRow(width, LayoutMetrics.CONTROL_HEIGHT);
            row.Add(captionLabel, 0f);
            row.Add(control, 1f);

            return row;
        }

        public static HudChain CreateCheckboxRow(BorderedCheckBox checkbox, string text, float width) {
            var label = CreateLabel(text, width - LayoutMetrics.CHECKBOX_SIZE - LayoutMetrics.ROW_SPACING, LayoutMetrics.CHECKBOX_SIZE);

            var row = CreateRow(width, LayoutMetrics.CHECKBOX_SIZE);
            row.Add(checkbox, 0f);
            row.Add(label, 1f);

            return row;
        }

        /// <summary>
        /// A row of buttons, each taking an equal share of the width.
        /// </summary>
        public static HudChain CreateButtonRow(float width, params BorderedButton[] buttons) {
            var row = CreateRow(width, LayoutMetrics.BUTTON_HEIGHT);

            foreach (var button in buttons) {
                row.Add(button, 1f);
            }

            return row;
        }

        private static void OnCursorEntered(object sender, EventArgs args) {
            HudSoundUtils.PlaySound("HudMouseOver");
        }

        private static void OnClicked(object sender, EventArgs args) {
            HudSoundUtils.PlaySound("HudMouseClick");
        }
    }
}
