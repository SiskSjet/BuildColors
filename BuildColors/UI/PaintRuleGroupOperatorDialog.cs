using RichHudFramework.UI;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using System;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Small editor for a single condition group, reached through the Edit button of the condition tree so
    /// that groups and conditions are edited the same way.
    /// </summary>
    public class PaintRuleGroupOperatorDialog : DialogBase {
        private const float DIALOG_HEIGHT = 320f;
        private const float MIN_DIALOG_WIDTH = 420f;
        private const float PREFERRED_DIALOG_WIDTH = 520f;

        private readonly PaintRuleConditionGroup _group;
        private readonly Dropdown<PaintRuleLogicalOperator> _operatorDropdown;

        public PaintRuleGroupOperatorDialog(PaintRuleConditionGroup group, bool isRoot, HudParentBase parent = null) : base(parent) {
            _group = group;

            var dialogWidth = MathHelper.Clamp(DialogSafeArea.GetAvailableWidth(), MIN_DIALOG_WIDTH, PREFERRED_DIALOG_WIDTH);

            Size = new Vector2(dialogWidth, DIALOG_HEIGHT);
            HeaderText = isRoot ? "Edit Conditions Root" : "Edit Group";

            var contentWidth = dialogWidth - Padding.X - LayoutMetrics.CONTENT_PADDING_X;

            _operatorDropdown = new Dropdown<PaintRuleLogicalOperator>() { DimAlignment = DimAlignments.Width, Height = LayoutMetrics.CONTROL_HEIGHT };
            _operatorDropdown.Add("Match ALL members (AND)", PaintRuleLogicalOperator.And);
            _operatorDropdown.Add("Match ANY member (OR)", PaintRuleLogicalOperator.Or);
            _operatorDropdown.SetSelection(_group.Operator);

            var saveButton = new BorderedButton() { Text = "Save", Padding = Vector2.Zero, Width = 140f, Height = LayoutMetrics.BUTTON_HEIGHT };
            var cancelButton = new BorderedButton() { Text = "Cancel", Padding = Vector2.Zero, Width = 140f, Height = LayoutMetrics.BUTTON_HEIGHT };

            var buttonRow = new HudChain(false) {
                CollectionContainer = { saveButton, cancelButton },
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.AlignMembersEnd,
                Width = contentWidth,
                Height = LayoutMetrics.BUTTON_HEIGHT,
            };

            var layout = new HudChain(true, body) {
                ParentAlignment = ParentAlignments.Inner,
                DimAlignment = DimAlignments.UnpaddedSize,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                CollectionContainer = {
                    { CreateLabel("How should the members of this group be combined?"), 0f },
                    { CreateLabel("Operator"), 0f },
                    { _operatorDropdown, 0f },
                    { new EmptyHudElement(), 1f },
                    { buttonRow, 0f }
                },
                Spacing = LayoutMetrics.SECTION_SPACING,
                Padding = new Vector2(LayoutMetrics.CONTENT_PADDING_X, LayoutMetrics.CONTENT_PADDING_Y)
            };

            _operatorDropdown.MouseInput.CursorEntered += OnMouseOver;
            saveButton.MouseInput.LeftClicked += OnSaveClicked;
            saveButton.MouseInput.CursorEntered += OnMouseOver;
            cancelButton.MouseInput.LeftClicked += OnCancelClicked;
            cancelButton.MouseInput.CursorEntered += OnMouseOver;
        }

        public event RichHudFramework.EventHandler Saved;

        private static Label CreateLabel(string text) {
            return new Label() {
                Text = text,
                Format = Style.BodyText,
                AutoResize = false,
                DimAlignment = DimAlignments.Width,
                Height = LayoutMetrics.LABEL_HEIGHT,
            };
        }

        private void OnSaveClicked(object sender, EventArgs e) {
            if (_operatorDropdown.Value != null) {
                _group.Operator = _operatorDropdown.Value.AssocMember;
            }

            HudSoundUtils.PlaySound("HudBleep");
            Saved?.Invoke(this, EventArgs.Empty);
            Close();
        }

        private void OnCancelClicked(object sender, EventArgs e) {
            HudSoundUtils.PlaySound("HudLockingLost");
            Close();
        }

        private void OnMouseOver(object sender, EventArgs e) {
            HudSoundUtils.PlaySound("HudMouseOver");
        }
    }
}
