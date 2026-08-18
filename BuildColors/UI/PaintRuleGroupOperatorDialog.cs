using RichHudFramework.UI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using Sisk.Utils.Localization.Extensions;
using System;
using VRageMath;

using static Sisk.BuildColors.UI.ControlFactory;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Editor for a single condition group.
    /// </summary>
    public class PaintRuleGroupOperatorDialog : DialogBase {
        private const float DIALOG_HEIGHT = 420f;
        private const float MIN_DIALOG_WIDTH = 420f;
        private const float PREFERRED_DIALOG_WIDTH = 520f;

        private readonly PaintRuleConditionGroup _group;
        private readonly Dropdown<PaintRuleLogicalOperator> _operatorDropdown;
        private readonly BorderedCheckBox _negateCheckbox;

        public PaintRuleGroupOperatorDialog(PaintRuleConditionGroup group, bool isRoot, HudParentBase parent = null) : base(parent) {
            _group = group;

            var dialogWidth = MathHelper.Clamp(DialogSafeArea.GetAvailableWidth(), MIN_DIALOG_WIDTH, PREFERRED_DIALOG_WIDTH);

            Size = new Vector2(dialogWidth, DIALOG_HEIGHT);
            HeaderText = isRoot ? ModText.BC_UI_GroupDialogTitleRoot.GetString() : ModText.BC_UI_GroupDialogTitle.GetString();

            var contentWidth = ContentWidth(dialogWidth);

            _operatorDropdown = CreateFullWidthDropdown<PaintRuleLogicalOperator>();
            _operatorDropdown.Add(ModText.BC_UI_Operator_And.GetString(), PaintRuleLogicalOperator.And);
            _operatorDropdown.Add(ModText.BC_UI_Operator_Or.GetString(), PaintRuleLogicalOperator.Or);
            _operatorDropdown.SetSelection(_group.Operator);

            _negateCheckbox = CreateCheckbox();
            _negateCheckbox.Value = _group.Negate;

            var negateLabel = new Label() {
                Text = ModText.BC_UI_Operator_Negate.GetString(),
                Format = Style.BodyText,
                AutoResize = false,
                Height = LayoutMetrics.CHECKBOX_SIZE,
            };

            var negateRow = new HudChain(false) {
                CollectionContainer = {
                    { _negateCheckbox, 0f },
                    { negateLabel, 1f }
                },
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = LayoutMetrics.CHECKBOX_SIZE,
            };

            var cancelButton = CreateCancelButton(140f);
            var saveButton = CreateButton(ModText.BC_UI_Save.GetString(), 140f, ButtonRole.Primary);

            var buttonRow = new HudChain(false) {
                CollectionContainer = { cancelButton, saveButton },
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.AlignMembersEnd,
                Width = contentWidth,
                Height = LayoutMetrics.BUTTON_HEIGHT,
            };

            var layout = CreateContentColumn(LayoutMetrics.SECTION_SPACING);

            layout.Add(CreateFullWidthCaption(ModText.BC_UI_GroupHint.GetString()), 0f);
            layout.Add(CreateFullWidthLabel(ModText.BC_UI_Operator.GetString()), 0f);
            layout.Add(_operatorDropdown, 0f);
            layout.Add(negateRow, 0f);
            layout.Add(CreateFullWidthCaption(ModText.BC_UI_GroupHint_Empty.GetString()), 0f);
            layout.Add(new EmptyHudElement(), 1f);
            layout.Add(buttonRow, 0f);

            saveButton.MouseInput.LeftClicked += OnSaveClicked;
        }

        public event RichHudFramework.EventHandler Saved;

        private void OnSaveClicked(object sender, EventArgs e) {
            if (_operatorDropdown.Value != null) {
                _group.Operator = _operatorDropdown.Value.AssocMember;
            }

            _group.Negate = _negateCheckbox.Value;

            HudSoundUtils.PlaySound("HudBleep");
            Saved?.Invoke(this, EventArgs.Empty);
            Close();
        }
    }
}
