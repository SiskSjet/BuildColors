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
    /// <para>
    /// Rows are rearranged by picking one up and putting it down again. The mouse drives that by dragging,
    /// keyboard and controller by the reorder binds, but all three run the same grab session.
    /// </para>
    /// </summary>
    public class PaintRuleConditionGroupDialog : DialogBase {
        private const float BUTTON_ROW_HEIGHT = LayoutMetrics.BUTTON_HEIGHT;
        private const float DIALOG_HEIGHT = 760f;
        private const float DRAG_THRESHOLD = 6f;
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
        private readonly BorderedButton _moveButton;
        private readonly Label _statusLabel;

        private PaintRuleConditionDialog _activeConditionDialog;
        private PaintRuleGroupOperatorDialog _activeGroupDialog;
        private PaintRuleConditionGroup _pendingTargetGroup;
        private PaintRuleConditionGroup _pendingGroup;
        private PaintRuleCondition _pendingCondition;
        private bool _pendingConditionIsNew;

        // Grab session state
        private readonly List<DropSlot> _slots = new List<DropSlot>();
        private readonly List<int> _slotAtRow = new List<int>();
        private PaintRuleCondition _carriedCondition;
        private PaintRuleConditionGroup _carriedGroup;
        private PaintRuleConditionGroup _originGroup;
        private int _originIndex;
        private int _slotIndex;
        private bool _isGrabbed;
        private bool _isMouseDrag;

        // Mouse drag tracking
        private ConditionNode _pressedNode;
        private Vector2 _pressPosition;
        private bool _isPressed;

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

            var helpLabel = CreateLabel("Drag a row to move it, or select one and press Move. Groups combine their members with AND or OR.");

            _treeList = new ListBox<ConditionNode>() { DimAlignment = DimAlignments.Width };

            _addConditionButton = CreateButton("Add Condition");
            _addGroupButton = CreateButton("Add Group");
            _editButton = CreateButton("Edit");
            _removeButton = CreateButton("Remove");
            _moveButton = CreateButton("Move");

            var treeButtons = new HudChain(false) {
                CollectionContainer = {
                    { _addConditionButton, 1f }, { _addGroupButton, 1f }, { _editButton, 1f },
                    { _removeButton, 1f }, { _moveButton, 1f }
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
            _moveButton.MouseInput.LeftClicked += OnMoveClicked;
            _moveButton.MouseInput.CursorEntered += OnMouseOver;

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
        /// Walks the tree once, producing the rows to display and, while a node is carried, every position it
        /// could be dropped into. Slots are only emitted for the list that can actually accept the carried
        /// node, and the carried node itself is never part of the walk because grabbing detaches it.
        /// </summary>
        private void BuildTree(List<TreeItem> items) {
            items.Clear();
            Walk(_workingGroup, null, null, 0, items);
        }

        private void Walk(PaintRuleConditionGroup group, PaintRuleConditionGroup parent, PaintRuleConditionGroup grandParent, int depth, List<TreeItem> items) {
            if (group == null) {
                return;
            }

            items.Add(new TreeItem {
                Node = new ConditionNode { Group = group, Parent = parent, GrandParent = grandParent, Depth = depth },
                Depth = depth
            });

            var childDepth = depth + 1;
            var conditions = group.Conditions;

            for (var i = 0; i <= (conditions?.Count ?? 0); i++) {
                if (_carriedCondition != null) {
                    items.Add(new TreeItem { Slot = new DropSlot { Target = group, Index = i, Depth = childDepth }, Depth = childDepth });
                }

                if (conditions != null && i < conditions.Count) {
                    items.Add(new TreeItem {
                        Node = new ConditionNode { Condition = conditions[i], Parent = group, GrandParent = parent, Depth = childDepth },
                        Depth = childDepth
                    });
                }
            }

            var children = group.Children;

            for (var i = 0; i <= (children?.Count ?? 0); i++) {
                if (_carriedGroup != null) {
                    items.Add(new TreeItem { Slot = new DropSlot { Target = group, Index = i, Depth = childDepth }, Depth = childDepth });
                }

                if (children != null && i < children.Count) {
                    Walk(children[i], group, parent, childDepth, items);
                }
            }
        }

        /// <summary>
        /// Rebuilds the list. While a node is carried the current drop position is shown as an insertion row,
        /// which keeps the indicator working the same for mouse, keyboard and controller.
        /// </summary>
        private void RefreshTree(object nodeToSelect = null) {
            var items = new List<TreeItem>();
            BuildTree(items);

            _slots.Clear();
            _slotAtRow.Clear();
            _treeList.ClearEntries();

            foreach (var item in items) {
                if (item.Slot != null) {
                    _slots.Add(item.Slot);
                }
            }

            if (_isGrabbed) {
                _slotIndex = MathHelper.Clamp(_slotIndex, 0, Math.Max(_slots.Count - 1, 0));
            }

            var slotsSeen = 0;
            var indicatorRow = -1;

            foreach (var item in items) {
                if (item.Slot != null) {
                    if (_isGrabbed && slotsSeen == _slotIndex) {
                        indicatorRow = _treeList.Count;
                        _slotAtRow.Add(slotsSeen);
                        _treeList.Add(BuildIndicatorText(item.Depth), null);
                    }

                    slotsSeen++;
                    continue;
                }

                _slotAtRow.Add(MathHelper.Clamp(slotsSeen, 0, Math.Max(_slots.Count - 1, 0)));
                _treeList.Add(BuildRowText(item.Node), item.Node);
            }

            if (_treeList.Count == 0) {
                UpdateControlState();
                return;
            }

            if (_isGrabbed) {
                // Keep the insertion row in view as it moves.
                _treeList.SetSelectionAt(indicatorRow >= 0 ? indicatorRow : 0);
            } else {
                var index = nodeToSelect != null ? FindRow(items, nodeToSelect) : 0;
                _treeList.SetSelectionAt(index >= 0 && index < _treeList.Count ? index : 0);
            }

            UpdateControlState();
        }

        private static int FindRow(List<TreeItem> items, object target) {
            var row = 0;

            foreach (var item in items) {
                if (item.Slot != null) {
                    continue;
                }

                if (item.Node.Represents(target)) {
                    return row;
                }

                row++;
            }

            return -1;
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

        private string BuildIndicatorText(int depth) {
            return string.Format("{0}>> {1} <<", new string(' ', depth * INDENT_SPACES), CarriedDescription());
        }

        private string CarriedDescription() {
            if (_carriedGroup != null) {
                var operatorText = _carriedGroup.Operator == PaintRuleLogicalOperator.And ? "ALL of" : "ANY of";
                return "Group - match " + operatorText;
            }

            return PaintRuleConditionText.Describe(_carriedCondition);
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

            _addConditionButton.InputEnabled = !_isGrabbed && node != null;
            _addGroupButton.InputEnabled = !_isGrabbed && node != null;
            _editButton.InputEnabled = !_isGrabbed && node != null;
            _removeButton.InputEnabled = !_isGrabbed && movable;
            _moveButton.InputEnabled = _isGrabbed || movable;
            _moveButton.Text = _isGrabbed ? "Drop" : "Move";

            if (_isGrabbed) {
                _statusLabel.Text = string.Format(
                    "Moving {0}  -  Up/Down move, Left/Right change group, Enter/A drop, Esc/B cancel.",
                    CarriedDescription());
            }
        }

        // ---- grab session ----

        private void OnMoveClicked(object sender, EventArgs e) {
            if (_isGrabbed) {
                Drop();
            } else {
                BeginGrab(GetSelectedNode(), false);
            }
        }

        /// <summary>
        /// Detaches the node so the tree no longer contains it, then works out where it may be dropped.
        /// </summary>
        private void BeginGrab(ConditionNode node, bool fromMouse) {
            if (_isGrabbed || node == null || node.Parent == null || _activeConditionDialog != null || _activeGroupDialog != null) {
                return;
            }

            _originGroup = node.Parent;

            if (node.IsGroup) {
                _originIndex = node.Parent.Children.IndexOf(node.Group);
                if (_originIndex < 0) {
                    return;
                }

                _carriedGroup = node.Group;
                node.Parent.Children.RemoveAt(_originIndex);
            } else {
                _originIndex = node.Parent.Conditions.IndexOf(node.Condition);
                if (_originIndex < 0) {
                    return;
                }

                _carriedCondition = node.Condition;
                node.Parent.Conditions.RemoveAt(_originIndex);
            }

            _isGrabbed = true;
            _isMouseDrag = fromMouse;

            // The list must not consume the arrow keys while they drive the grab.
            _treeList.InputEnabled = false;

            // The first pass builds the slot list; the second only runs if the origin is not slot zero.
            _slotIndex = 0;
            RefreshTree();
            SetSlot(FindSlot(_originGroup, _originIndex));

            HudSoundUtils.PlaySound("HudMouseClick");
        }

        private int FindSlot(PaintRuleConditionGroup target, int index) {
            for (var i = 0; i < _slots.Count; i++) {
                if (ReferenceEquals(_slots[i].Target, target) && _slots[i].Index == index) {
                    return i;
                }
            }

            return 0;
        }

        private void Drop() {
            if (!_isGrabbed) {
                return;
            }

            var slot = _slots.Count > 0 ? _slots[MathHelper.Clamp(_slotIndex, 0, _slots.Count - 1)] : null;
            var carried = Insert(slot ?? new DropSlot { Target = _originGroup, Index = _originIndex });

            EndGrab();
            RefreshTree(carried);
            HudSoundUtils.PlaySound("HudBleep");
        }

        private void CancelGrab() {
            if (!_isGrabbed) {
                return;
            }

            var carried = Insert(new DropSlot { Target = _originGroup, Index = _originIndex });

            EndGrab();
            RefreshTree(carried);
            HudSoundUtils.PlaySound("HudLockingLost");
        }

        private object Insert(DropSlot slot) {
            if (_carriedGroup != null) {
                var children = slot.Target.Children ?? (slot.Target.Children = new List<PaintRuleConditionGroup>());
                children.Insert(MathHelper.Clamp(slot.Index, 0, children.Count), _carriedGroup);
                return _carriedGroup;
            }

            var conditions = slot.Target.Conditions ?? (slot.Target.Conditions = new List<PaintRuleCondition>());
            conditions.Insert(MathHelper.Clamp(slot.Index, 0, conditions.Count), _carriedCondition);
            return _carriedCondition;
        }

        private void EndGrab() {
            _isGrabbed = false;
            _isMouseDrag = false;
            _isPressed = false;
            _pressedNode = null;
            _carriedCondition = null;
            _carriedGroup = null;
            _originGroup = null;
            _treeList.InputEnabled = true;
            _statusLabel.Text = string.Empty;
        }

        /// <summary>
        /// Moves to the nearest slot shallower or deeper than the current one, which is how a node is moved
        /// into or out of a nested group without a dedicated button.
        /// </summary>
        private void StepDepth(int direction) {
            if (_slots.Count == 0) {
                return;
            }

            var currentDepth = _slots[_slotIndex].Depth;

            for (var distance = 1; distance < _slots.Count; distance++) {
                var forward = _slotIndex + distance;
                var backward = _slotIndex - distance;

                if (forward < _slots.Count && Matches(_slots[forward].Depth, currentDepth, direction)) {
                    SetSlot(forward);
                    return;
                }

                if (backward >= 0 && Matches(_slots[backward].Depth, currentDepth, direction)) {
                    SetSlot(backward);
                    return;
                }
            }
        }

        private static bool Matches(int depth, int currentDepth, int direction) {
            return direction < 0 ? depth < currentDepth : depth > currentDepth;
        }

        private void SetSlot(int index) {
            var clamped = MathHelper.Clamp(index, 0, Math.Max(_slots.Count - 1, 0));

            if (clamped == _slotIndex) {
                return;
            }

            _slotIndex = clamped;
            RefreshTree();
        }

        // ---- input ----

        protected override void HandleInput(Vector2 cursorPos) {
            base.HandleInput(cursorPos);

            if (_activeConditionDialog != null || _activeGroupDialog != null) {
                return;
            }

            if (_isGrabbed) {
                HandleGrabInput(cursorPos);
                return;
            }

            HandleIdleInput(cursorPos);
        }

        private void HandleIdleInput(Vector2 cursorPos) {
            var node = GetSelectedNode();

            if (ReorderInput.GrabPressed && node != null && !_treeList.IsMousedOver) {
                BeginGrab(node, false);
                return;
            }

            // Press and drag on a row starts the same grab session the keyboard uses.
            if (SharedBinds.LeftButton.IsNewPressed && _treeList.IsMousedOver) {
                _isPressed = true;
                _pressPosition = cursorPos;
                _pressedNode = node;
                return;
            }

            if (!SharedBinds.LeftButton.IsPressed) {
                _isPressed = false;
                _pressedNode = null;
                return;
            }

            if (_isPressed && Math.Abs(cursorPos.Y - _pressPosition.Y) > DRAG_THRESHOLD) {
                // The selection follows the press, so re-read it before grabbing.
                BeginGrab(_pressedNode ?? GetSelectedNode(), true);
            }
        }

        private void HandleGrabInput(Vector2 cursorPos) {
            if (_isMouseDrag) {
                UpdateSlotFromCursor(cursorPos);

                if (SharedBinds.LeftButton.IsReleased) {
                    Drop();
                    return;
                }

                if (SharedBinds.RightButton.IsNewPressed) {
                    CancelGrab();
                    return;
                }
            }

            switch (ReorderInput.Poll()) {
                case ReorderIntent.Previous:
                    SetSlot(_slotIndex - 1);
                    break;
                case ReorderIntent.Next:
                    SetSlot(_slotIndex + 1);
                    break;
                case ReorderIntent.Shallower:
                    StepDepth(-1);
                    break;
                case ReorderIntent.Deeper:
                    StepDepth(1);
                    break;
                case ReorderIntent.Drop:
                    Drop();
                    break;
                case ReorderIntent.Cancel:
                    CancelGrab();
                    break;
            }
        }

        /// <summary>
        /// Maps the cursor onto the nearest list row and from there onto a drop slot. Rows are uniform height,
        /// so the closest row centre is a stable target even as the insertion row moves around.
        /// </summary>
        private void UpdateSlotFromCursor(Vector2 cursorPos) {
            var entries = _treeList.EntryList;
            if (entries.Count == 0 || _slotAtRow.Count == 0) {
                return;
            }

            var nearest = -1;
            var nearestDistance = float.MaxValue;

            for (var i = 0; i < entries.Count && i < _slotAtRow.Count; i++) {
                var distance = Math.Abs(entries[i].Element.Position.Y - cursorPos.Y);

                if (distance < nearestDistance) {
                    nearestDistance = distance;
                    nearest = i;
                }
            }

            if (nearest < 0) {
                return;
            }

            var slot = _slotAtRow[nearest];

            // Dragging past the ends walks the list so long trees can still be traversed.
            if (cursorPos.Y > entries[0].Element.Position.Y) {
                slot = _slotIndex - 1;
            } else if (cursorPos.Y < entries[entries.Count - 1].Element.Position.Y) {
                slot = _slotIndex + 1;
            }

            SetSlot(slot);
        }

        // ---- editing ----

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

            if (node.IsGroup) {
                node.Parent.Children.Remove(node.Group);
            } else {
                node.Parent.Conditions.Remove(node.Condition);
            }

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
            if (_isGrabbed) {
                CancelGrab();
            }

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
        /// A position the carried node can be dropped into.
        /// </summary>
        private class DropSlot {
            public PaintRuleConditionGroup Target { get; set; }
            public int Index { get; set; }
            public int Depth { get; set; }
        }

        /// <summary>
        /// One step of the tree walk: either a row to display or a drop position between rows.
        /// </summary>
        private class TreeItem {
            public ConditionNode Node { get; set; }
            public DropSlot Slot { get; set; }
            public int Depth { get; set; }
        }

        /// <summary>
        /// One row of the flattened condition tree. Exactly one of Group or Condition is set.
        /// </summary>
        private class ConditionNode {
            public PaintRuleConditionGroup Group { get; set; }
            public PaintRuleCondition Condition { get; set; }
            public PaintRuleConditionGroup Parent { get; set; }

            /// <summary>
            /// Group owning <see cref="Parent"/>, kept so a node knows the level above its own.
            /// </summary>
            public PaintRuleConditionGroup GrandParent { get; set; }

            public int Depth { get; set; }

            public bool IsGroup {
                get { return Group != null; }
            }

            public bool Represents(object target) {
                return IsGroup ? ReferenceEquals(Group, target) : ReferenceEquals(Condition, target);
            }
        }
    }
}
