using RichHudFramework.UI;
using System;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Named HSV slider picker styled after the SE terminal color picker.
    /// </summary>
    public class ColorPickerHSV2 : HudElementBase {
        public readonly SliderBox[] sliders;

        private readonly HudChain colorNameColumn;
        private readonly HudChain colorSliderColumn;
        private readonly HudChain colorValueColumn;
        private readonly TexturedBox display;

        private readonly HudChain headerChain;

        private readonly HudChain mainChain, colorChain;

        private readonly Label name;

        private readonly Label[] sliderText;

        private readonly TextField[] sliderTextBox;

        private Vector3 _color;

        public ColorPickerHSV2(HudParentBase parent) : base(parent) {
            name = new Label() {
                Format = Style.CaptionText,
                Text = "NewColorPicker",
                AutoResize = false,
                Size = new Vector2(88f, 22f)
            };

            display = new TexturedBox() {
                Width = 231f,
                Color = VRageMath.Color.Black
            };

            new BorderBox(display) {
                Color = Style.BorderColor,
                Thickness = 1f,
                DimAlignment = DimAlignments.Both,
            };

            headerChain = new HudChain(false) {
                SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.FitChainBoth,
                Height = 22f,
                Spacing = 0f,
                CollectionContainer = { name, display }
            };

            sliderText = new Label[]
            {
                new Label() { AutoResize = false, Format = Style.CaptionText, Height = 47f, Text = "H: " },
                new Label() { AutoResize = false, Format = Style.CaptionText, Height = 47f, Text = "S: " },
                new Label() { AutoResize = false, Format = Style.CaptionText, Height = 47f, Text = "V: " }
            };

            sliderTextBox = new TextField[] {
                CreateChannelField(),
                CreateChannelField(),
                CreateChannelField()
            };

            colorNameColumn = new HudChain(true) {
                SizingMode = HudChainSizingModes.FitMembersBoth | HudChainSizingModes.FitChainBoth,
                Width = 17f,
                Spacing = 5f,
                CollectionContainer = { sliderText[0], sliderText[1], sliderText[2] }
            };

            colorValueColumn = new HudChain(true) {
                SizingMode = HudChainSizingModes.FitMembersBoth | HudChainSizingModes.FitChainBoth,
                Width = 77f,
                Spacing = 5f,
                CollectionContainer = { sliderTextBox[0], sliderTextBox[1], sliderTextBox[2] }
            };

            sliders = new SliderBox[]
            {
                new SliderBox() { Min = 0f, Max = 360f, Height = 47f },
                new SliderBox() { Min = 0f, Max = 100f, Height = 47f },
                new SliderBox() { Min = 0f, Max = 100f, Height = 47f }
            };

            colorSliderColumn = new HudChain(true) {
                SizingMode = HudChainSizingModes.FitMembersBoth | HudChainSizingModes.FitChainBoth,
                Width = 201f,
                Spacing = 5f,
                CollectionContainer = { sliders[0], sliders[1], sliders[2] }
            };

            colorChain = new HudChain(false) {
                SizingMode = HudChainSizingModes.FitChainBoth,
                Spacing = 5f,
                CollectionContainer =
                {
                    colorNameColumn,
                    colorValueColumn,
                    colorSliderColumn,
                }
            };

            mainChain = new HudChain(true, this) {
                SizingMode = HudChainSizingModes.FitChainBoth,
                Spacing = 5f,
                CollectionContainer =
                {
                    headerChain,
                    colorChain,
                }
            };

            Size = new Vector2(318f, 163f);

            UseCursor = true;
            ShareCursor = true;

            foreach (var slider in sliders) {
                slider.MouseInput.LeftReleased += OnSliderLeftReleased;
            }

            foreach (var textBox in sliderTextBox) {
                textBox.ValueChanged += OnTextChanged;
            }
        }

        public ColorPickerHSV2() : this(null) { }

        /// <summary>
        /// One of the three channel entry boxes, styled like every other text field in the mod.
        /// </summary>
        private static TextField CreateChannelField() {
            return new GameInputBlockingTextField() {
                Text = string.Empty,
                AutoResize = false,
                Height = 47f,
                Format = Style.BodyText,
                Color = Style.SunkenBackgroundColor,
                BorderColor = Style.ButtonBorderColor,
                HighlightColor = Style.HoverBackgroundColor,
                FocusColor = Style.SunkenBackgroundColor,
                FocusTextColor = Style.BodyTextColor,
                UseFocusFormatting = false,
            };
        }

        public event RichHudFramework.EventHandler ColorChanged;

        /// <summary>
        /// Color currently specified by the color picker.
        /// </summary>
        public Vector3 Color {
            get { return _color; }
            set {
                _color = value;
                SetColorsToSliders();
                SetColorToTextBoxes();
                display.Color = (_color / new Vector3(360f, 100f, 100f)).HSVtoColor();
            }
        }

        /// <summary>
        /// Text rendered by the label
        /// </summary>
        public RichText Name { get { return name.TextBoard.GetText(); } set { name.TextBoard.SetText(value); } }

        protected override void HandleInput(Vector2 cursorPos) {
            for (var i = 0; i < sliders.Length; i++) {
                if (sliders[i].FocusHandler.HasFocus) {
                    if (SharedBinds.UpArrow.IsNewPressed) {
                        i = MathHelper.Clamp(i - 1, 0, sliders.Length - 1);
                        sliders[i].FocusHandler.GetInputFocus();
                    } else if (SharedBinds.DownArrow.IsNewPressed) {
                        i = MathHelper.Clamp(i + 1, 0, sliders.Length - 1);
                        sliders[i].FocusHandler.GetInputFocus();
                    }

                    if (SharedBinds.LeftArrow.IsPressed) {
                        SetColorFromSlider();
                        SetColorToTextBoxes();
                    } else if (SharedBinds.RightArrow.IsPressed) {
                        SetColorFromSlider();
                        SetColorToTextBoxes();
                    }

                    if (SharedBinds.LeftButton.IsPressed) {
                        SetColorFromSlider();
                        SetColorToTextBoxes();
                    }

                    break;
                }
            }
        }

        private void OnSliderLeftReleased(object sender, EventArgs e) {
            SetColorFromSlider();
            SetColorToTextBoxes();
        }

        private void OnTextChanged(object sender, EventArgs e) {
            if (!sliderTextBox[0].InputOpen && !sliderTextBox[1].InputOpen && !sliderTextBox[2].InputOpen) {
                return;
            }

            float x;
            float y;
            float z;

            if (!float.TryParse(sliderTextBox[0].TextBoard.GetText().ToString(), out x)) {
                sliderTextBox[0].Text = $"{Math.Round(_color.X, 1)}";
            }

            if (!float.TryParse(sliderTextBox[1].TextBoard.GetText().ToString(), out y)) {
                sliderTextBox[1].Text = $"{Math.Round(_color.Y, 1)}";
            }

            if (!float.TryParse(sliderTextBox[2].TextBoard.GetText().ToString(), out z)) {
                sliderTextBox[2].Text = $"{Math.Round(_color.Z, 1)}";
            }

            if (sliderTextBox[0].InputOpen) {
                x = MathHelper.Clamp(x, 0f, 360f);
                sliderTextBox[0].Text = $"{Math.Round(x, 1)}";
            }

            if (sliderTextBox[1].InputOpen) {
                y = MathHelper.Clamp(y, 0f, 100f);
                sliderTextBox[1].Text = $"{Math.Round(y, 1)}";
            }

            if (sliderTextBox[2].InputOpen) {
                z = MathHelper.Clamp(z, 0f, 100f);
                sliderTextBox[2].Text = $"{Math.Round(z, 1)}";
            }

            SetColorFromSliderTextBox();
            SetColorsToSliders();
        }

        private void SetColorFromSlider() {
            _color = new Vector3() {
                X = sliders[0].Value,
                Y = sliders[1].Value,
                Z = sliders[2].Value,
            };

            display.Color = (_color / new Vector3(360f, 100f, 100f)).HSVtoColor();
            ColorChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SetColorFromSliderTextBox() {
            float x;
            float y;
            float z;

            if (float.TryParse(sliderTextBox[0].TextBoard.GetText().ToString(), out x)) {
                if (float.TryParse(sliderTextBox[1].TextBoard.GetText().ToString(), out y)) {
                    if (float.TryParse(sliderTextBox[2].TextBoard.GetText().ToString(), out z)) {
                        x = MathHelper.Clamp(x, 0f, 360f);
                        y = MathHelper.Clamp(y, 0f, 100f);
                        z = MathHelper.Clamp(z, 0f, 100f);

                        _color = new Vector3() {
                            X = x,
                            Y = y,
                            Z = z,
                        };

                        display.Color = (_color / new Vector3(360f, 100f, 100f)).HSVtoColor();
                        ColorChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        private void SetColorsToSliders() {
            sliders[0].Value = _color.X;
            sliders[1].Value = _color.Y;
            sliders[2].Value = _color.Z;
        }

        private void SetColorToTextBoxes() {
            sliderTextBox[0].Text = $"{Math.Round(_color.X, 1)}";
            sliderTextBox[1].Text = $"{Math.Round(_color.Y, 1)}";
            sliderTextBox[2].Text = $"{Math.Round(_color.Z, 1)}";
        }
    }
}