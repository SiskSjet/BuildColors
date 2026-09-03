using RichHudFramework.UI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Services;
using Sisk.Utils.Localization.Extensions;
using System;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Picks who a color set or paint job is sent to.
    /// </summary>
    internal class ShareDialog : DialogBase {
        private const float LIST_HEIGHT = 260f;
        private const float WIDTH = 460f;

        private readonly ActionButton _sendButton;
        private readonly RangeClampedListBox<SharePlayer> _recipients;

        public ShareDialog(string title, HudParentBase parent = null) : base(parent) {
            var contentWidth = ContentWidth(WIDTH);

            var hintLabel = ControlFactory.CreateCaption(ModText.BC_Share_PickRecipient.GetString(), contentWidth);

            _recipients = ControlFactory.CreateList<SharePlayer>(contentWidth, LIST_HEIGHT);

            var cancelButton = CreateCancelButton(140f);
            _sendButton = ControlFactory.CreateButton(ModText.BC_Share_Send.GetString(), 140f, ButtonRole.Primary);

            var height = HEADER_HEIGHT
                + Padding.Y
                + LayoutMetrics.CONTENT_PADDING_Y * 2f
                + hintLabel.Height
                + LIST_HEIGHT
                + LayoutMetrics.BUTTON_HEIGHT
                + LayoutMetrics.ROW_SPACING * 2f;

            Size = new Vector2(WIDTH, height);
            HeaderText = title;

            var layout = CreateContentColumn(LayoutMetrics.ROW_SPACING);

            layout.Add(hintLabel, 0f);
            layout.Add(_recipients, 0f);
            layout.Add(ControlFactory.CreateButtonRow(contentWidth, cancelButton, _sendButton), 0f);

            _sendButton.MouseInput.LeftClicked += OnSend;

            FillRecipients(hintLabel);
        }

        /// <summary>
        /// Raised with the player the share is meant for.
        /// </summary>
        public event Action<SharePlayer> Confirmed;

        private void FillRecipients(Label hintLabel) {
            if (!ShareNetwork.IsAvailable) {
                hintLabel.Text = ModText.BC_Share_NeedsMultiplayer.GetString();
                _sendButton.InputEnabled = false;

                return;
            }

            var players = ShareNetwork.GetRecipients();

            if (players.Count == 0) {
                hintLabel.Text = ModText.BC_Share_NoPlayersOnline.GetString();
                _sendButton.InputEnabled = false;

                return;
            }

            _recipients.Add(ModText.BC_Share_Everyone.GetString(), new SharePlayer(ShareNetwork.EVERYONE, null));

            foreach (var player in players) {
                _recipients.Add(player.Name, player);
            }

            _recipients.SetSelectionAt(0);
        }

        private void OnSend(object sender, EventArgs args) {
            if (_recipients.Value == null) {
                return;
            }

            HudSoundUtils.PlaySound("HudBleep");

            var handler = Confirmed;
            if (handler != null) {
                handler(_recipients.Value.AssocMember);
            }

            Close();
        }
    }
}
