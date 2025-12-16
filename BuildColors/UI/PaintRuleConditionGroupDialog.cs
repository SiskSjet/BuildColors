using RichHudFramework.UI;
using Sisk.BuildColors.Settings.Models.PaintJobs;
using System;
using System.Collections.Generic;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Editor for the condition tree of a single paint rule. Conditions live in groups, groups can be nested
    /// inside other groups, and every group combines its members with either AND or OR. That makes
    /// expressions such as (A AND B) OR (C AND D) expressible.
    /// </summary>
    public class PaintRuleConditionGroupDialog : DialogBase {
        private const float BUTTON_ROW_HEIGHT = LayoutMetrics.BUTTON_HEIGHT;
        private const float DIALOG_HEIGHT = 760f;
        private const int INDENT_SPACES = 4;
        private const float MIN_DIALOG_WIDTH = 620f;
        private const float PREFERRED_DIALOG_WIDTH = 900f;

        private readonly PaintRule _rule;
        private readonly PaintRuleConditionGroup _workingGroup;
        private readonly ListBox<ConditionNode> _treeList;
        private readonly BorderedButton _addConditionButton;
        private readonly BorderedButton _addGroupButton;
        private readonly BorderedButton _editButton;
        private readonly BorderedButton _removeButton;
        private readonly BorderedButton _moveUpButton;
        private readonly BorderedButton _moveDownButton;
        private readonly BorderedButton _moveIntoButton;
        private readonly BorderedButton _moveOutButton;
        private readonly Label _statusLabel;

        private PaintRuleConditionDialog _activeConditionDialog;
        private PaintRuleGroupOperatorDialog _activeGroupDialog;
        private PaintRuleConditionGroup _pendingTargetGroup;
        private PaintRuleConditionGroup _pendingGroup;
        private PaintRuleCondition _pendingCondition;
        private bool _pendingConditionIsNew;

        public PaintRuleConditionGroupDialog(PaintRule rule, HudParentBase parent = null) : base(parent) {
            _rule = rule;

            if (_rule.ConditionGroup == null) {
                _rule.ConditionGroup = PaintRuleConditionGroup.CreateDefault();
            }

            // The tree is edited on a working copy. The rule only receives it when Done commits.
            _workingGroup = _rule.ConditionGroup.Clone();

            var dialogWidth = MathHelper.Clamp(DialogSafeArea.GetAvailableWidth(), MIN_DIALOG_WIDTH, PREFERRED_DIALOG_WIDTH);

            Size = new Vector2(dialogWidth, DIALOG_HEIGHT);
            HeaderText = string.Format("Conditions - {0}", _rule.Name);

            var contentWidth = dialogWidth - Padding.X - LayoutMetrics.CONTENT_PADDING_X;

            var helpLabel = CreateLabel("Select a row and use Edit. Groups combine their members with AND or OR; nest groups to build complex conditions.");

            _treeList = new ListBox<ConditionNode>() { DimAlignment = DimAlignments.Width };

            _addConditionButton = CreateButton("Add Condition");
            _addGroupButton = CreateButton("Add Group");
            _editButton = CreateButton("Edit");
            _removeButton = CreateButton("Remove");

            var treeButtons = new HudChain(false) {
                CollectionContainer = {
                    { _addConditionButton, 1f }, { _addGroupButton, 1f }, { _editButton, 1f }, { _removeButton, 1f }
                },
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = BUTTON_ROW_HEIGHT,
            };

            _moveUpButton = CreateButton("Move Up");
            _moveDownButton = CreateButton("Move Down");
            _moveIntoButton = CreateButton("Move Into Group");
            _moveOutButton = CreateButton("Move Out");

            var moveButtons = new HudChain(false) {
                CollectionContainer = {
                    { _moveUpButton, 1f }, { _moveDownButton, 1f }, { _moveIntoButton, 1f }, { _moveOutButton, 1f }
                },
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                Width = contentWidth,
                Height = BUTTON_ROW_HEIGHT,
            };

            _statusLabel = new Label() {
                Text = string.Empty,
                Format = Style.BodyText,
                AutoResize = false,
                DimAlignment = DimAlignments.Width,
                Height = LayoutMetrics.STATUS_HEIGHT,
            };

            var doneButton = CreateButton("Done");
            doneButton.Width = 150f;

            var cancelButton = CreateButton("Cancel");
            cancelButton.Width = 150f;

            var buttonRow = new HudChain(false) {
                CollectionContainer = { doneButton, cancelButton },
                Spacing = LayoutMetrics.ROW_SPACING,
                SizingMode = HudChainSizingModes.AlignMembersEnd,
                Width = contentWidth,
                Height = BUTTON_ROW_HEIGHT,
            };

            var layout = new HudChain(true, body) {
                ParentAlignment = ParentAlignments.Inner,
                DimAlignment = DimAlignments.UnpaddedSize,
                SizingMode = HudChainSizingModes.FitMembersOffAxis,
                CollectionContainer = {
                    helpLabel,
                    CreateSeparator(),
                    { _treeList, 1f },
                    treeButtons,
                    moveButtons,
                    _statusLabel,
                    buttonRow
                },
                Spacing = LayoutMetrics.SECTION_SPACING,
                Padding = new Vector2(LayoutMetrics.CONTENT_PADDING_X, LayoutMetrics.CONTENT_PADDING_Y)
            };

            _treeList.ValueChanged += OnSelectionChanged;

            _addConditionButton.MouseInput.LeftClicked += OnAddCondition;
            _addConditionButton.MouseInput.CursorEntered += OnMouseOver;
            _addGroupButton.MouseInput.LeftClicked += OnAddGroup;
            _addGroupButton.MouseInput.CursorEntered += OnMouseOver;
            _editButton.MouseInput.LeftClicked += OnEdit;
            _editButton.MouseInput.CursorEntered += OnMouseOver;
            _removeButton.MouseInput.LeftClicked += OnRemove;
            _removeButton.MouseInput.CursorEntered += OnMouseOver;

            _moveUpButton.MouseInput.LeftClicked += (s2, e2) => MoveSelected(-1);
            _moveUpButton.MouseInput.CursorEntered += OnMouseOver;
            _moveDownButton.MouseInput.LeftClicked += (s2, e2) => MoveSelected(1);
            _moveDownButton.MouseInput.CursorEntered += OnMouseOver;
            _moveIntoButton.MouseInput.LeftClicked += OnMoveIntoGroup;
            _moveIntoButton.MouseInput.CursorEntered += OnMouseOver;
            _moveOutButton.MouseInput.LeftClicked += OnMoveOut;
            _moveOutButton.MouseInput.CursorEntered += OnMouseOver;

            doneButton.MouseInput.LeftClicked += OnDoneClicked;
            doneButton.MouseInput.CursorEntered += OnMouseOver;
            cancelButton.MouseInput.LeftClicked += OnCancelClicked;
            cancelButton.MouseInput.CursorEntered += OnMouseOver;

            RefreshTree();
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

        private static TexturedBox CreateSeparator() {
            return new TexturedBox() { DimAlignment = DimAlignments.Width, Height = LayoutMetrics.SEPARATOR_HEIGHT, Color = Style.SeparatorColor };
        }

        private static BorderedButton CreateButton(string text) {
            return new BorderedButton() { Text = text, Padding = Vector2.Zero, Height = LayoutMetrics.BUTTON_HEIGHT };
        }

        /// <summary>
        /// Rebuilds the flattened view of the condition tree.
        /// </summary>
        private void RefreshTree(object nodeToSelect = null) {
            _treeList.ClearEntries();

            var nodes = new List<ConditionNode>();
            Flatten(_workingGroup, null, null, 0, nodes);

            foreach (var node in nodes) {
                _treeList.Add(BuildRowText(node), node);
            }

            if (_treeList.Count > 0) {
                var index = nodeToSelect != null ? nodes.FindIndex(node => node.Represents(nodeToSelect)) : 0;
                _treeList.SetSelectionAt(index >= 0 ? index : 0);
            }

            UpdateControlState();
        }

        private static void Flatten(PaintRuleConditionGroup group, PaintRuleConditionGroup parent, PaintRuleConditionGroup grandParent, int depth, List<ConditionNode> nodes) {
            if (group == null) {
                return;
            }

            nodes.Add(new ConditionNode { Group = group, Parent = parent, GrandParent = grandParent, Depth = depth });

            if (group.Conditions != null) {
                foreach (var condition in group.Conditions) {
                    nodes.Add(new ConditionNode { Condition = condition, Parent = group, GrandParent = parent, Depth = depth + 1 });
                }
            }

            if (group.Children != null) {
                foreach (var child in group.Children) {
                    Flatten(child, group, parent, depth + 1, nodes);
                }
            }
        }

        private string BuildRowText(ConditionNode node) {
            var indent = new string(' ', node.Depth * INDENT_SPACES);

            if (node.IsGroup) {
                var operatorText = node.Group.Operator == PaintRuleLogicalOperator.And ? "ALL of" : "ANY of";
                var label = node.Parent == null ? "Match " + operatorText : "Group - match " + operatorText;
                return string.Format("{0}[ {1} ]", indent, label);
            }

            return string.Format("{0}- {1}", indent, PaintRuleConditionText.Describe(node.Condition));
        }

        private ConditionNode GetSelectedNode() {
            return _treeList.Value != null ? _treeList.Value.AssocMember : null;
        }

        /// <summary>
        /// Group that new members are added to: the selected group, or the group owning the selected condition.
        /// </summary>
        private PaintRuleConditionGroup GetTargetGroup() {
            var node = GetSelectedNode();
            if (node == null) {
                return _workingGroup;
            }

            return node.IsGroup ? node.Group : node.Parent;
        }

        private void UpdateControlState() {
            var node = GetSelectedNode();
            var isRoot = node != null && node.IsGroup && node.Parent == null;
            var movable = node != null && !isRoot;

            _addConditionButton.InputEnabled = node != null;
            _addGroupButton.InputEnabled = node != null;
            _editButton.InputEnabled = node != null;
            _removeButton.InputEnabled = movable;

            var siblingCount = 0;
            var index = movable ? GetSiblingIndex(node, out siblingCount) : -1;

            _moveUpButton.InputEnabled = index > 0;
            _moveDownButton.InputEnabled = index >= 0 && index < siblingCount - 1;
            _moveIntoButton.InputEnabled = movable && FindGroupBelow(node) != null;
            _moveOutButton.InputEnabled = movable && node.GrandParent != null;
        }

        /// <summary>
        /// Position of the node among its siblings. Conditions and nested groups live in separate lists,
        /// so which list applies depends on the node type.
        /// </summary>
        private static int GetSiblingIndex(ConditionNode node, out int siblingCount) {
            siblingCount = 0;

            if (node?.Parent == null) {
                return -1;
            }

            if (node.IsGroup) {
                var groups = node.Parent.Children;
                if (groups == null) {
                    return -1;
                }

                siblingCount = groups.Count;
                return groups.IndexOf(node.Group);
            }

            var conditions = node.Parent.Conditions;
            if (conditions == null) {
                return -1;
            }

            siblingCount = conditions.Count;
            return conditions.IndexOf(node.Condition);
        }

        /// <summary>
        /// First sibling group shown below the node, which is where Move Into Group sends it. Conditions are
        /// listed before nested groups, so for a condition this is simply the parent's first child group.
        /// </summary>
        private static PaintRuleConditionGroup FindGroupBelow(ConditionNode node) {
            var children = node?.Parent?.Children;
            if (children == null || children.Count == 0) {
                return null;
            }

            if (!node.IsGroup) {
                return children[0];
            }

            var index = children.IndexOf(node.Group);
            return index >= 0 && index < children.Count - 1 ? children[index + 1] : null;
        }

        private void MoveSelected(int offset) {
            var node = GetSelectedNode();

            int siblingCount;
            var index = GetSiblingIndex(node, out siblingCount);
            var target = index + offset;

            if (index < 0 || target < 0 || target >= siblingCount) {
                return;
            }

            if (node.IsGroup) {
                node.Parent.Children.RemoveAt(index);
                node.Parent.Children.Insert(target, node.Group);
            } else {
                node.Parent.Conditions.RemoveAt(index);
                node.Parent.Conditions.Insert(target, node.Condition);
            }

            RefreshTree(node.Item);
            _statusLabel.Text = string.Empty;
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private void OnMoveIntoGroup(object sender, EventArgs e) {
            var node = GetSelectedNode();
            var target = FindGroupBelow(node);
            if (node == null || target == null) {
                return;
            }

            Detach(node);

            if (node.IsGroup) {
                target.Children.Add(node.Group);
            } else {
                target.Conditions.Add(node.Condition);
            }

            RefreshTree(node.Item);
            _statusLabel.Text = string.Empty;
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private void OnMoveOut(object sender, EventArgs e) {
            var node = GetSelectedNode();
            if (node == null || node.GrandParent == null) {
                return;
            }

            var target = node.GrandParent;
            Detach(node);

            if (node.IsGroup) {
                target.Children.Add(node.Group);
            } else {
                target.Conditions.Add(node.Condition);
            }

            RefreshTree(node.Item);
            _statusLabel.Text = string.Empty;
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private static void Detach(ConditionNode node) {
            if (node.IsGroup) {
                node.Parent.Children.Remove(node.Group);
            } else {
                node.Parent.Conditions.Remove(node.Condition);
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e) {
            UpdateControlState();
        }

        private void OnAddCondition(object sender, EventArgs e) {
            var target = GetTargetGroup();
            if (target == null || _activeConditionDialog != null) {
                return;
            }

            OpenConditionDialog(target, new PaintRuleCondition(), true);
        }

        private void OnAddGroup(object sender, EventArgs e) {
            var target = GetTargetGroup();
            if (target == null) {
                return;
            }

            if (target.Children == null) {
                target.Children = new List<PaintRuleConditionGroup>();
            }

            // A nested group with the same operator as its parent would be a no-op, so default to the other.
            var group = PaintRuleConditionGroup.CreateDefault();
            group.Operator = target.Operator == PaintRuleLogicalOperator.And
                ? PaintRuleLogicalOperator.Or
                : PaintRuleLogicalOperator.And;

            target.Children.Add(group);

            RefreshTree(group);
            _statusLabel.Text = "Group added. Select it and add conditions to it.";
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private void OnEdit(object sender, EventArgs e) {
            var node = GetSelectedNode();
            if (node == null) {
                return;
            }

            if (node.IsGroup) {
                OpenGroupDialog(node.Group, node.Parent == null);
                return;
            }

            OpenConditionDialog(node.Parent, node.Condition, false);
        }

        private void OnRemove(object sender, EventArgs e) {
            var node = GetSelectedNode();
            if (node == null || node.Parent == null) {
                return;
            }

            Detach(node);

            RefreshTree();
            _statusLabel.Text = string.Empty;
            HudSoundUtils.PlaySound("HudLockingLost");
        }

        private void OpenGroupDialog(PaintRuleConditionGroup group, bool isRoot) {
            if (group == null || _activeGroupDialog != null) {
                return;
            }

            _pendingGroup = group;

            _activeGroupDialog = new PaintRuleGroupOperatorDialog(group, isRoot);
            _activeGroupDialog.Saved += OnGroupDialogSaved;
            _activeGroupDialog.Closed += OnGroupDialogClosed;

            RequestDialog(_activeGroupDialog);
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private void OnGroupDialogSaved(object sender, EventArgs e) {
            RefreshTree(_pendingGroup);
            _statusLabel.Text = string.Empty;
        }

        private void OnGroupDialogClosed(object sender, EventArgs e) {
            if (_activeGroupDialog != null) {
                _activeGroupDialog.Saved -= OnGroupDialogSaved;
                _activeGroupDialog.Closed -= OnGroupDialogClosed;
            }

            _activeGroupDialog = null;
            _pendingGroup = null;

            UpdateControlState();
        }

        private void OpenConditionDialog(PaintRuleConditionGroup targetGroup, PaintRuleCondition condition, bool isNew) {
            if (targetGroup == null || condition == null) {
                return;
            }

            if (targetGroup.Conditions == null) {
                targetGroup.Conditions = new List<PaintRuleCondition>();
            }

            _pendingTargetGroup = targetGroup;
            _pendingCondition = condition;
            _pendingConditionIsNew = isNew;

            _activeConditionDialog = new PaintRuleConditionDialog(condition);
            _activeConditionDialog.Saved += OnConditionDialogSaved;
            _activeConditionDialog.Closed += OnConditionDialogClosed;

            RequestDialog(_activeConditionDialog);
            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private void OnConditionDialogSaved(object sender, EventArgs e) {
            if (_pendingTargetGroup == null || _pendingCondition == null) {
                return;
            }

            if (_pendingConditionIsNew && !_pendingTargetGroup.Conditions.Contains(_pendingCondition)) {
                _pendingTargetGroup.Conditions.Add(_pendingCondition);
            }

            RefreshTree(_pendingCondition);
            _statusLabel.Text = string.Empty;
            HudSoundUtils.PlaySound("HudBleep");
        }

        private void OnConditionDialogClosed(object sender, EventArgs e) {
            if (_activeConditionDialog != null) {
                _activeConditionDialog.Saved -= OnConditionDialogSaved;
                _activeConditionDialog.Closed -= OnConditionDialogClosed;
            }

            _activeConditionDialog = null;
            _pendingTargetGroup = null;
            _pendingCondition = null;
            _pendingConditionIsNew = false;

            UpdateControlState();
        }

        private void OnDoneClicked(object sender, EventArgs e) {
            _rule.ConditionGroup = _workingGroup;

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

        /// <summary>
        /// One row of the flattened condition tree. Exactly one of Group or Condition is set.
        /// </summary>
        private class ConditionNode {
            public PaintRuleConditionGroup Group { get; set; }
            public PaintRuleCondition Condition { get; set; }
            public PaintRuleConditionGroup Parent { get; set; }

            /// <summary>
            /// Group owning <see cref="Parent"/>, needed to move a node out one level.
            /// </summary>
            public PaintRuleConditionGroup GrandParent { get; set; }

            public int Depth { get; set; }

            public bool IsGroup {
                get { return Group != null; }
            }

            /// <summary>
            /// The model object this row stands for, used to restore the selection after a rebuild.
            /// </summary>
            public object Item {
                get { return IsGroup ? (object)Group : Condition; }
            }

            public bool Represents(object target) {
                return IsGroup ? ReferenceEquals(Group, target) : ReferenceEquals(Condition, target);
            }
        }
    }
}
