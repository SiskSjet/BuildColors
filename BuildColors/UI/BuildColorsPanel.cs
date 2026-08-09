using RichHudFramework;
using RichHudFramework.UI;
using Sandbox.ModAPI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using Sisk.Utils.Localization.Extensions;
using System;
using System.Collections.Generic;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// The mod's interface: one panel in the room left of the vanilla controls.
    /// </summary>
    internal class BuildColorsPanel : HudElementBase {
        private const float HEADER_HEIGHT = 54f;

        /// <summary>
        /// Width of the navigation rail, and the panel width below which it gives some of that back.
        /// </summary>
        private const float RAIL_WIDTH = 190f;

        private const float RAIL_WIDTH_NARROW = 150f;
        private const float NARROW_PANEL_WIDTH = 1100f;

        private readonly TexturedBox _body;
        private readonly TexturedBox _border;
        private readonly List<NavButton> _navButtons = new List<NavButton>();
        private readonly List<PanelView> _views = new List<PanelView>();

        private int _selectedView;

        public BuildColorsPanel(HudParentBase parent = null) : base(parent) {
            var size = PickerScreenLayout.PanelSize;
            Size = size;

            _border = new TexturedBox(this) {
                DimAlignment = DimAlignments.Both,
                Color = Style.BorderColor,
            };

            _body = new TexturedBox(this) {
                DimAlignment = DimAlignments.Both,
                Color = Style.BodyBackgroundColor,
                Padding = new Vector2(2f),
            };

            var contentWidth = size.X - LayoutMetrics.CONTENT_PADDING_X * 2f;
            var contentHeight = size.Y
                - LayoutMetrics.CONTENT_PADDING_Y * 2f
                - HEADER_HEIGHT
                - LayoutMetrics.SECTION_SPACING;

            var railWidth = size.X >= NARROW_PANEL_WIDTH ? RAIL_WIDTH : RAIL_WIDTH_NARROW;
            var viewWidth = contentWidth - railWidth - LayoutMetrics.BLOCK_SPACING;

            var title = new Label() {
                Text = ModText.BC_UI_PanelTitle.GetString(),
                Format = Style.TitleText,
                AutoResize = false,
                Width = contentWidth,
                Height = HEADER_HEIGHT - LayoutMetrics.SEPARATOR_HEIGHT - LayoutMetrics.TIGHT_SPACING,
            };

            var header = ControlFactory.CreateColumn(contentWidth, HEADER_HEIGHT, LayoutMetrics.TIGHT_SPACING);
            header.Add(title, 0f);
            header.Add(ControlFactory.CreateSeparator(contentWidth), 0f);

            var rail = new TexturedBox() {
                Width = railWidth,
                Height = contentHeight,
                Color = Style.ChromeBackgroundColor,
            };

            new BorderBox(rail) {
                DimAlignment = DimAlignments.Both,
                Color = Style.SubtleBorderColor,
                Thickness = 1f,
            };

            var railLayout = new HudChain(true, rail) {
                ParentAlignment = ParentAlignments.Top | ParentAlignments.InnerV,
                Spacing = LayoutMetrics.TIGHT_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = railWidth,
                Height = contentHeight,
            };

            var viewHost = ControlFactory.CreateColumn(viewWidth, contentHeight);

            AddView(new ColorSetsView(viewWidth, contentHeight), ModText.BC_UI_ColorSets.GetString(), railWidth, railLayout, viewHost);
            AddView(new GeneratorView(viewWidth, contentHeight), ModText.BC_UI_Generator.GetString(), railWidth, railLayout, viewHost);
            AddView(new PaintJobsView(viewWidth, contentHeight), ModText.BC_UI_PaintJobs.GetString(), railWidth, railLayout, viewHost);

            var contentRow = ControlFactory.CreateRow(contentWidth, contentHeight, LayoutMetrics.BLOCK_SPACING);
            contentRow.Add(rail, 0f);
            contentRow.Add(viewHost, 0f);

            var layout = new HudChain(true, _body) {
                ParentAlignment = ParentAlignments.Inner,
                Spacing = LayoutMetrics.SECTION_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = size.Y - LayoutMetrics.CONTENT_PADDING_Y * 2f,
            };

            layout.Add(header, 0f);
            layout.Add(contentRow, 0f);

            ShowView(0);
        }

        public event RichHudFramework.EventHandler DialogRequested;

        /// <summary>
        /// The size the panel was built for.
        /// </summary>
        public Vector2 BuiltForSize { get; private set; }

        /// <summary>
        /// Rebuilds the paint job list after something outside the panel has changed it.
        /// </summary>
        public void RefreshPaintJobs(PaintJob jobToSelect = null) {
            foreach (var view in _views) {
                var paintJobs = view as PaintJobsView;

                if (paintJobs != null) {
                    paintJobs.Refresh(jobToSelect);
                }
            }
        }

        /// <summary>
        /// Updates the share counts on every page after the inbox changed.
        /// </summary>
        public void RefreshShares() {
            foreach (var view in _views) {
                view.RefreshShares();
            }
        }

        /// <summary>
        /// Called when the panel leaves the screen.
        /// </summary>
        public void Commit() {
            foreach (var view in _views) {
                view.Commit();
            }
        }

        /// <summary>
        /// Called when the panel comes back on screen.
        /// </summary>
        public void Reload() {
            if (_selectedView >= 0 && _selectedView < _views.Count) {
                _views[_selectedView].Refresh();
            }
        }

        protected override void Draw() {
            base.Draw();

            var opacity = MyAPIGateway.Session?.Config?.UIBkOpacity ?? 1f;

            _border.Color = Style.BorderColor.SetAlphaPct(opacity);
            _body.Color = Style.BodyBackgroundColor.SetAlphaPct(opacity);
        }

        protected override void Layout() {
            base.Layout();

            Offset = PickerScreenLayout.PanelOffset;
            BuiltForSize = Size;
        }

        private void AddView(PanelView view, string name, float railWidth, HudChain railLayout, HudChain viewHost) {
            var index = _views.Count;

            var button = new NavButton(name, railWidth);
            button.MouseInput.LeftClicked += (sender, args) => {
                ShowView(index);
                HudSoundUtils.PlaySound("HudMouseClick");
            };

            view.DialogRequested += OnDialogRequested;

            _navButtons.Add(button);
            _views.Add(view);

            railLayout.Add(button, 0f);
            viewHost.Add(view, 0f);
        }

        private void ShowView(int index) {
            if (index < 0 || index >= _views.Count) {
                return;
            }

            if (index != _selectedView) {
                _views[_selectedView].Commit();
            }

            _selectedView = index;

            for (var i = 0; i < _views.Count; i++) {
                _views[i].Visible = i == index;
                _navButtons[i].Selected = i == index;
            }

            _views[index].Refresh();
        }

        private void OnDialogRequested(object sender, EventArgs args) {
            DialogRequested?.Invoke(this, args);
        }
    }
}
