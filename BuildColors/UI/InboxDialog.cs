using RichHudFramework.UI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Services;
using Sisk.Utils.Localization.Extensions;
using System;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// What other players have sent, and the choice to keep it or turn it down.
    /// </summary>
    internal class InboxDialog : DialogBase {
        private const float LIST_HEIGHT = 220f;
        private const float SWATCH_ROW_HEIGHT = 26f;
        private const float WIDTH = 560f;

        private readonly ActionButton _acceptButton;
        private readonly ActionButton _declineAllButton;
        private readonly ActionButton _declineButton;
        private readonly Label _detailLabel;
        private readonly ShareInbox _inbox;
        private readonly SafeListBox<ShareOffer> _offers;
        private readonly SwatchGrid _preview;

        private bool _rebuilding;

        public InboxDialog(ShareInbox inbox, HudParentBase parent = null) : base(parent) {
            _inbox = inbox;

            var contentWidth = ContentWidth(WIDTH);
            var previewHeight = SwatchGrid.HeightFor(SWATCH_ROW_HEIGHT);

            _offers = ControlFactory.CreateList<ShareOffer>(contentWidth, LIST_HEIGHT);
            _detailLabel = ControlFactory.CreateCaption(string.Empty, contentWidth);

            _preview = new SwatchGrid(contentWidth, SWATCH_ROW_HEIGHT);

            var previewHost = ControlFactory.CreateColumn(contentWidth, previewHeight, LayoutMetrics.TIGHT_SPACING);
            previewHost.Add(_preview, 0f);

            _acceptButton = ControlFactory.CreateButton(ModText.BC_Share_Accept.GetString(), role: ButtonRole.Primary);
            _declineButton = ControlFactory.CreateButton(ModText.BC_Share_Decline.GetString());
            _declineAllButton = ControlFactory.CreateButton(ModText.BC_Share_DeclineAll.GetString(), role: ButtonRole.Danger);

            var closeButton = CreateCancelButton(140f, ModText.BC_UI_Done.GetString());

            var height = HEADER_HEIGHT
                + Padding.Y
                + LayoutMetrics.CONTENT_PADDING_Y * 2f
                + LIST_HEIGHT
                + _detailLabel.Height
                + previewHeight
                + LayoutMetrics.BUTTON_HEIGHT * 2f
                + LayoutMetrics.ROW_SPACING * 4f;

            Size = new Vector2(WIDTH, height);
            HeaderText = ModText.BC_Share_InboxTitle.GetString();

            var layout = CreateContentColumn(LayoutMetrics.ROW_SPACING);

            layout.Add(_offers, 0f);
            layout.Add(_detailLabel, 0f);
            layout.Add(previewHost, 0f);
            layout.Add(ControlFactory.CreateButtonRow(contentWidth, _declineButton, _acceptButton), 0f);
            layout.Add(ControlFactory.CreateButtonRow(contentWidth, _declineAllButton, closeButton), 0f);

            _offers.ValueChanged += OnSelectionChanged;
            _acceptButton.MouseInput.LeftClicked += OnAccept;
            _declineButton.MouseInput.LeftClicked += OnDecline;
            _declineAllButton.MouseInput.LeftClicked += OnDeclineAll;

            Closed += OnDialogClosed;

            if (_inbox != null) {
                _inbox.Changed += Rebuild;
            }

            Rebuild();
        }

        /// <summary>
        /// Fills the list from the inbox, keeping the selection where it can.
        /// </summary>
        private void Rebuild() {
            var selected = _offers.Value != null ? _offers.Value.AssocMember : null;

            _rebuilding = true;
            _offers.ClearEntries();

            if (_inbox != null) {
                foreach (var offer in _inbox.Offers) {
                    _offers.Add(offer.Describe(), offer);
                }
            }

            _rebuilding = false;

            if (_offers.Count == 0) {
                UpdateDetail(null);
                return;
            }

            var index = 0;

            for (var i = 0; i < _offers.EntryList.Count; i++) {
                if (_offers.EntryList[i].AssocMember == selected) {
                    index = i;
                    break;
                }
            }

            _offers.SetSelectionAt(index);
        }

        private void UpdateDetail(ShareOffer offer) {
            var hasOffer = offer != null;

            _acceptButton.InputEnabled = hasOffer;
            _declineButton.InputEnabled = hasOffer;
            _declineAllButton.InputEnabled = _inbox != null && _inbox.Count > 0;

            if (!hasOffer) {
                _detailLabel.Text = ModText.BC_Share_InboxEmpty.GetString();
                _preview.Visible = false;

                return;
            }

            _detailLabel.Text = offer.DescribeDetail();
            _preview.Visible = offer.Kind == ShareKind.ColorSet;

            if (_preview.Visible) {
                _preview.SetColorSet(offer.ColorSet);
            }
        }

        private void OnSelectionChanged(object sender, EventArgs args) {
            if (_rebuilding) {
                return;
            }

            UpdateDetail(_offers.Value != null ? _offers.Value.AssocMember : null);
        }

        private void OnAccept(object sender, EventArgs args) {
            if (_inbox == null || _offers.Value == null) {
                return;
            }

            string name;

            if (_inbox.Accept(_offers.Value.AssocMember, out name)) {
                HudSoundUtils.PlaySound("HudBleep");
                Mod.Static?.RefreshShares();
            }
        }

        private void OnDecline(object sender, EventArgs args) {
            if (_inbox == null || _offers.Value == null) {
                return;
            }

            if (_inbox.Decline(_offers.Value.AssocMember)) {
                HudSoundUtils.PlaySound("HudMouseClick");
                Mod.Static?.RefreshShares();
            }
        }

        private void OnDeclineAll(object sender, EventArgs args) {
            if (_inbox == null || _inbox.Count == 0) {
                return;
            }

            _inbox.Clear();
            HudSoundUtils.PlaySound("HudLockingLost");
            Mod.Static?.RefreshShares();
        }

        private void OnDialogClosed(object sender, EventArgs args) {
            Closed -= OnDialogClosed;

            if (_inbox != null) {
                _inbox.Changed -= Rebuild;
            }
        }
    }
}
