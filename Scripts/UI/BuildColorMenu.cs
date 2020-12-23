using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using VRageMath;

// ReSharper disable ArrangeAccessorOwnerBody

namespace Sisk.BuildColors.UI {
    internal sealed class BuildColorMenu : HudElementBase {
        private readonly TexturedBox _body;
        private readonly BorderBox _border;

        private readonly ColorSets _colorSets;
        private readonly Label _footer;
        private readonly Label _header;
        private readonly HudChain<HudElementBase> _layout;
        private float _backgroundOpacity;

        public BuildColorMenu() : base(HudMain.Root) {
            Visible = false;
            Size = new Vector2(500f, 1080f);
            Offset = new Vector2(220, 0);

            _body = new TexturedBox(this) {
                CaptureCursor = true,
                ShareCursor = true,
                Color = Style.BodyBackgroundColor,
                DimAlignment = DimAlignments.Both,
                ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner,
            };

            _border = new BorderBox(this) {
                DimAlignment = DimAlignments.Both,
                Color = Style.BorderColor,
                Thickness = .75f
            };

            _header = new Label {
                DimAlignment = DimAlignments.Width,
                Format = Style.HeaderText,
                Text = Mod.NAME,
                AutoResize = false,
                Height = 24f,
            };

            _footer = new Label(_body) {
                DimAlignment = DimAlignments.Width,
                ParentAlignment = ParentAlignments.Bottom | ParentAlignments.Inner,
                Format = Style.FooterText,
                Text = "by SISK",
                Height = 24f,
            };

            _colorSets = new ColorSets {
                DimAlignment = DimAlignments.Width | DimAlignments.IgnorePadding,
            };

            var separator = new TexturedBox {
                DimAlignment = DimAlignments.Width,
                Height = .75f,
                Padding = new Vector2(60, 0),
                Color = Style.SeparatorColor,
            };

            var separator2 = new TexturedBox {
                DimAlignment = DimAlignments.Width,
                Height = .75f,
                Padding = new Vector2(60, 0),
                Color = Style.SeparatorColor,
            };

            var load = new LabelBoxButton {
                Format = Style.ButtonText,
                Text = "Load",
                Width = 146,
                Height = 38,
                Color = Style.ButtonBackgroundColor,
                HighlightEnabled = true,
                HighlightColor = Style.ButtonHighlightBackgroundColor
            };

            var loadBorder = new BorderBox(load) {
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
                Color = Style.ButtonBorderColor,
                Thickness = .75f,
            };

            var delete = new LabelBoxButton {
                Format = Style.ButtonText,
                Text = "Delete",
                Width = 146,
                Height = 38,
                Color = Style.ButtonBackgroundColor,
                HighlightEnabled = true,
                HighlightColor = Style.ButtonHighlightBackgroundColor
            };

            var deleteBorder = new BorderBox(delete) {
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
                Color = Style.ButtonBorderColor,
                Thickness = .75f,
            };

            var share = new LabelBoxButton {
                Format = Style.ButtonText,
                Text = "Share",
                Width = 146,
                Height = 38,
                Color = Style.ButtonBackgroundColor,
                HighlightEnabled = true,
                HighlightColor = Style.ButtonHighlightBackgroundColor
            };

            var shareBorder = new BorderBox(share) {
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
                Color = Style.ButtonBorderColor,
                Thickness = .75f,
            };

            var save = new LabelBoxButton {
                DimAlignment = DimAlignments.Width | DimAlignments.IgnorePadding,
                ParentAlignment = ParentAlignments.Center,
                Format = Style.ButtonText,
                Text = "Save current color palette",
                Width = 150,
                Height = 38,
                Color = Style.ButtonBackgroundColor,
                HighlightEnabled = true,
                HighlightColor = Style.ButtonHighlightBackgroundColor
            };

            var saveBorder = new BorderBox(save) {
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
                Color = Style.ButtonBorderColor,
                Thickness = .75f,
            };

            var buttonLayout = new HudChain<LabelBoxButton> {
                ParentAlignment = ParentAlignments.Center,
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
                Spacing = 4,

                ChildContainer = {
                    load,
                    delete,
                    share
                }
            };

            _layout = new HudChain<HudElementBase>(_body) {
                DimAlignment = DimAlignments.Both,
                AlignVertical = true,
                Spacing = 10,
                Padding = new Vector2(50, 0),
                Offset = new Vector2(0, -30),

                ChildContainer = {
                    _header,
                    separator,
                    _colorSets,
                    buttonLayout,
                    separator2,
                    save,
                }
            };

            BackgroundOpacity = MyAPIGateway.Session.Config.UIBkOpacity;

            share.MouseInput.OnCursorEnter += CursorEntered;
            share.MouseInput.OnCursorExit += CursorExited;
            share.MouseInput.OnLeftRelease += OnLeftRelease;
        }

        public Color BackgroundColor {
            get { return _body.Color; }
            set { _body.Color = value; }
        }

        /// <summary>
        ///     Opacity between 0 and 1
        /// </summary>
        public float BackgroundOpacity {
            get { return _backgroundOpacity; }
            set {
                _backgroundOpacity = value;
                _body.Color = _body.Color.SetAlphaPct(_backgroundOpacity);
            }
        }

        public Color BorderColor {
            get { return _border.Color; }
            set { _border.Color = value; }
        }

        //public override float Height {
        //    get { return _border.Height; }
        //    set { _border.Height = value; }
        //}

        //public override float Width {
        //    get { return _border.Width; }
        //    set { _border.Width = value; }
        //}

        protected override void Draw() {
            BackgroundOpacity = MyAPIGateway.Session.Config.UIBkOpacity;
        }

        protected override void Layout() {
            _layout.Width = Width;
        }

        private void CursorEntered() {
            MyAPIGateway.Utilities.ShowNotification("cursor enter");
            MyAPIGateway.Utilities.ShowMessage("Me", "cursor enter");
        }

        ///// <summary>
        ///// Determines whether or not the cursor is within the bounds of the hud element.
        ///// </summary>
        //private bool IsMouseInBounds() {
        //    var cursorPos = HudMain.Cursor.Origin;
        //    //var cursorPos = GetCursorPosition();
        //    var width = Math.Max(8, cachedSize.X);
        //    var height = Math.Max(8, cachedSize.Y);
        //    var leftBound = cachedPosition.X - width / 2f;
        //    var rightBound = cachedPosition.X + width / 2f;
        //    var upperBound = cachedPosition.Y + height / 2f;
        //    var lowerBound = cachedPosition.Y - height / 2f;

        //    return (cursorPos.X >= leftBound && cursorPos.X < rightBound) && (cursorPos.Y >= lowerBound && cursorPos.Y < upperBound);
        //}

        //protected override void HandleInput() {
        //    if (MyAPIGateway.Gui.IsCursorVisible) {

        //        if (IsMouseInBounds()) {
        //            MyAPIGateway.Utilities.ShowMessage("Me", $"Over: {true}");

        //        } else {
        //            MyAPIGateway.Utilities.ShowMessage("Me", $"Over: {false}");
        //        }
        //    }
        //}

        //private bool isMouseOver;
        ///// <summary>
        ///// Indicates whether or not the element is capturing the cursor.
        ///// </summary>
        //public override bool IsMousedOver => Visible && isMouseOver;

        //public Vector2 GetCursorPosition() {
        //    var desktopSize = MyAPIGateway.Input.GetMouseAreaSize();
        //    var invMousePosScale = new Vector2 {
        //        X = HudMain.ScreenWidth / desktopSize.X,
        //        Y = HudMain.ScreenHeight / desktopSize.Y,
        //    };
        //    var pos = MyAPIGateway.Input.GetMousePosition() * invMousePosScale;
        //    return new Vector2(-HudMain.ScreenWidth / 2f, HudMain.ScreenHeight / 2f) + new Vector2(pos.X, -pos.Y) + (new Vector2(12f, -12f) * HudMain.ResScale);
        //}

        //public override void BeforeInput(HudLayers layer) {
        //    if (Visible) {
        //        for (int n = children.Count - 1; n >= 0; n--) {
        //            if (children[n].Visible) {
        //                children[n].BeforeInput(layer);
        //            }
        //        }

        //        if (_zOffset == layer) {
        //            if (CaptureCursor && HudMain.Cursor.Visible && !HudMain.Cursor.IsCaptured) {
        //                isMouseOver = IsMouseInBounds();
        //                HandleInput();

        //                if (!ShareCursor && isMouseOver) {
        //                    HudMain.Cursor.Capture(ID);
        //                }
        //            } else {
        //                isMouseOver = false;
        //                HandleInput();
        //            }
        //        }
        //    } else {
        //        isMouseOver = false;
        //    }
        //}

        private void CursorExited() {
            MyAPIGateway.Utilities.ShowNotification("cursor exit");
            MyAPIGateway.Utilities.ShowMessage("Me", "cursor exit");
        }

        private void OnLeftRelease() {
            // todo: open share with player dialog.
            MyAPIGateway.Utilities.ShowMessage("ME", "Share clicked");
        }
    }
}