using RichHudFramework.UI;
using Sandbox.ModAPI;
using Sisk.BuildColors.Localization;
using Sisk.BuildColors.Services;
using Sisk.BuildColors.Settings.Models;
using Sisk.Utils.Localization.Extensions;
using System;
using System.Linq;
using VRageMath;

namespace Sisk.BuildColors.UI {

    /// <summary>
    /// Saved color sets, the live palette beside them, and what can be done with one.
    /// </summary>
    internal class ColorSetsView : PanelView {
        /// <summary>
        /// Widest the list column gets.
        /// </summary>
        private const float MAX_LIST_WIDTH = 380f;

        private readonly Card _detailCard;
        private readonly GameInputBlockingTextField _filterField;
        private readonly ListBox<ColorSet> _list;
        private readonly SwatchGrid _paletteGrid;
        private readonly SwatchGrid _setGrid;

        private readonly ActionButton _duplicateButton;
        private readonly ActionButton _exportButton;
        private readonly ActionButton _favoriteButton;
        private readonly ActionButton _loadButton;
        private readonly ActionButton _loadRow1Button;
        private readonly ActionButton _loadRow2Button;
        private readonly ActionButton _removeButton;
        private readonly ActionButton _renameButton;

        private DateTime _lastClickTime;
        private string _lastSelectedName;
        private bool _refreshing;

        public ColorSetsView(float width, float height, HudParentBase parent = null) : base(width, height, parent) {
            var listWidth = Math.Min(MAX_LIST_WIDTH, width * .35f);
            var detailWidth = width - listWidth - LayoutMetrics.SECTION_SPACING;

            var listCard = new Card(ModText.BC_UI_ColorSets.GetString(), listWidth, height);
            var listContentWidth = Card.ContentWidth(listWidth);
            var listHeight = Card.ContentHeight(height)
                - LayoutMetrics.CONTROL_HEIGHT
                - LayoutMetrics.BUTTON_HEIGHT * 2f
                - LayoutMetrics.LABEL_HEIGHT
                - LayoutMetrics.ROW_SPACING * 4f;

            _filterField = ControlFactory.CreateTextField(listContentWidth);
            _list = ControlFactory.CreateList<ColorSet>(listContentWidth, Math.Max(listHeight, LayoutMetrics.CONTROL_HEIGHT));

            var saveButton = ControlFactory.CreateButton(ModText.BC_UI_SaveCurrentPalette.GetString());
            var importButton = ControlFactory.CreateButton(ModText.BC_UI_Import.GetString());

            listCard.Content.Add(ControlFactory.CreateControlRow(ModText.BC_UI_Filter.GetString(), _filterField, listContentWidth), 0f);
            listCard.Content.Add(_list, 0f);
            listCard.Content.Add(ControlFactory.CreateCaption(ModText.BC_UI_ColorSetsHint.GetString(), listContentWidth), 0f);
            listCard.Content.Add(ControlFactory.CreateButtonRow(listContentWidth, saveButton), 0f);
            listCard.Content.Add(ControlFactory.CreateButtonRow(listContentWidth, importButton), 0f);

            _detailCard = new Card(ModText.BC_UI_NoColorSetSelected.GetString(), detailWidth, height);
            var detailContentWidth = Card.ContentWidth(detailWidth);

            var gridSpace = Card.ContentHeight(height)
                - LayoutMetrics.LABEL_HEIGHT * 3f
                - LayoutMetrics.SEPARATOR_HEIGHT
                - LayoutMetrics.BUTTON_HEIGHT * 4f
                - LayoutMetrics.ROW_SPACING * 10f;

            var swatchRowHeight = SwatchGrid.FitRowHeight(gridSpace, 2);

            _setGrid = new SwatchGrid(detailContentWidth, swatchRowHeight, true);
            _setGrid.SlotClicked += OnSlotClicked;

            _paletteGrid = new SwatchGrid(detailContentWidth, swatchRowHeight);

            _loadButton = ControlFactory.CreateButton(ModText.BC_UI_Load.GetString(), role: ButtonRole.Primary);
            _loadRow1Button = ControlFactory.CreateButton(ModText.BC_UI_LoadFirstRow.GetString());
            _loadRow2Button = ControlFactory.CreateButton(ModText.BC_UI_LoadSecondRow.GetString());
            _renameButton = ControlFactory.CreateButton(ModText.BC_UI_Rename.GetString());
            _duplicateButton = ControlFactory.CreateButton(ModText.BC_UI_Duplicate.GetString());
            _favoriteButton = ControlFactory.CreateButton(ModText.BC_UI_Favorite.GetString());
            _exportButton = ControlFactory.CreateButton(ModText.BC_UI_Export.GetString());
            _removeButton = ControlFactory.CreateButton(ModText.BC_UI_Remove.GetString(), role: ButtonRole.Danger);

            _detailCard.Content.Add(ControlFactory.CreateCaption(ModText.BC_UI_SetColors.GetString(), detailContentWidth), 0f);
            _detailCard.Content.Add(_setGrid, 0f);
            _detailCard.Content.Add(ControlFactory.CreateCaption(ModText.BC_UI_EditSlotHint.GetString(), detailContentWidth), 0f);
            _detailCard.Content.Add(ControlFactory.CreateSeparator(detailContentWidth), 0f);
            _detailCard.Content.Add(ControlFactory.CreateCaption(ModText.BC_UI_CurrentPalette.GetString(), detailContentWidth), 0f);
            _detailCard.Content.Add(_paletteGrid, 0f);

            _detailCard.Content.Add(new EmptyHudElement() { Width = detailContentWidth }, 1f);
            _detailCard.Content.Add(ControlFactory.CreateButtonRow(detailContentWidth, _loadButton), 0f);
            _detailCard.Content.Add(ControlFactory.CreateButtonRow(detailContentWidth, _loadRow1Button, _loadRow2Button), 0f);
            _detailCard.Content.Add(ControlFactory.CreateButtonRow(detailContentWidth, _renameButton, _duplicateButton), 0f);
            _detailCard.Content.Add(ControlFactory.CreateButtonRow(detailContentWidth, _favoriteButton, _exportButton, _removeButton), 0f);

            var layout = ControlFactory.CreateRow(width, height, LayoutMetrics.SECTION_SPACING);
            layout.Add(listCard, 0f);
            layout.Add(_detailCard, 0f);

            layout.Register(this);

            _list.ValueChanged += OnSelectionChanged;
            _filterField.TextChanged += OnFilterChanged;

            saveButton.MouseInput.LeftClicked += OnSaveCurrentPalette;
            importButton.MouseInput.LeftClicked += OnImport;

            _loadButton.MouseInput.LeftClicked += OnLoad;
            _loadRow1Button.MouseInput.LeftClicked += OnLoadFirstRow;
            _loadRow2Button.MouseInput.LeftClicked += OnLoadSecondRow;
            _renameButton.MouseInput.LeftClicked += OnRename;
            _duplicateButton.MouseInput.LeftClicked += OnDuplicate;
            _favoriteButton.MouseInput.LeftClicked += OnToggleFavorite;
            _exportButton.MouseInput.LeftClicked += OnExport;
            _removeButton.MouseInput.LeftClicked += OnRemove;

            Refresh();
        }

        public override void Refresh() {
            var selected = _list.Value != null ? _list.Value.AssocMember.Name : _lastSelectedName;
            var filter = _filterField.Text.ToString().Trim();

            _refreshing = true;

            _list.ClearEntries();

            var sets = Mod.Static?.ColorSets;

            if (sets != null) {
                var ordered = sets
                    .Where(set => Matches(set, filter))
                    .OrderByDescending(set => set.Favorite)
                    .ThenBy(set => set.Name, StringComparer.InvariantCultureIgnoreCase);

                foreach (var colorSet in ordered) {
                    _list.Add(GetLabel(colorSet), colorSet);
                }
            }

            RefreshPalette();

            if (_list.Count == 0) {
                _refreshing = false;

                UpdateDetail(false, default(ColorSet));

                return;
            }

            var index = IndexOf(selected);
            _list.SetSelectionAt(index >= 0 ? index : 0);

            _refreshing = false;
        }

        private static bool Matches(ColorSet set, string filter) {
            return string.IsNullOrEmpty(filter)
                || (set.Name != null && set.Name.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) >= 0);
        }

        private static string GetLabel(ColorSet set) {
            return set.Favorite ? ModText.BC_UI_FavoriteMark.GetString(set.Name) : set.Name;
        }

        /// <summary>
        /// The selected set as it is stored, rather than the copy the list handed out.
        /// </summary>
        private bool TryGetSelected(out ColorSet colorSet) {
            colorSet = default(ColorSet);

            if (_list.Value == null || Mod.Static?.ColorSets == null) {
                return false;
            }

            var name = _list.Value.AssocMember.Name;

            foreach (var set in Mod.Static.ColorSets) {
                if (StringComparer.InvariantCultureIgnoreCase.Equals(set.Name, name)) {
                    colorSet = set;
                    return true;
                }
            }

            return false;
        }

        private void RefreshPalette() {
            _paletteGrid.SetMasks(Mod.Static?.GetCurrentPalette());
        }

        private int IndexOf(string name) {
            if (string.IsNullOrEmpty(name)) {
                return -1;
            }

            for (var i = 0; i < _list.EntryList.Count; i++) {
                if (StringComparer.InvariantCultureIgnoreCase.Equals(_list.EntryList[i].AssocMember.Name, name)) {
                    return i;
                }
            }

            return -1;
        }

        private void UpdateDetail(bool hasSelection, ColorSet colorSet) {
            _loadButton.InputEnabled = hasSelection;
            _loadRow1Button.InputEnabled = hasSelection;
            _loadRow2Button.InputEnabled = hasSelection;
            _renameButton.InputEnabled = hasSelection;
            _duplicateButton.InputEnabled = hasSelection;
            _favoriteButton.InputEnabled = hasSelection;
            _exportButton.InputEnabled = hasSelection;
            _removeButton.InputEnabled = hasSelection;

            if (hasSelection) {
                _detailCard.Title = GetLabel(colorSet);
                _setGrid.SetColorSet(colorSet);
            } else {
                _detailCard.Title = ModText.BC_UI_NoColorSetSelected.GetString();
                _setGrid.Clear();
            }
        }

        private void OnFilterChanged(string text) {
            if (_refreshing) {
                return;
            }

            Refresh();
        }

        private void OnSelectionChanged(object sender, EventArgs args) {
            ColorSet selection;

            if (!TryGetSelected(out selection)) {
                _lastSelectedName = string.Empty;
                UpdateDetail(false, default(ColorSet));
                return;
            }

            UpdateDetail(true, selection);
            RefreshPalette();

            if (_refreshing) {
                _lastSelectedName = selection.Name;
                return;
            }

            HudSoundUtils.PlaySound("HudMouseClick");

            var time = DateTime.Now;

            if (time - _lastClickTime < TimeSpan.FromMilliseconds(500) && selection.Name == _lastSelectedName) {
                Load(selection, 0, ColorSet.SLOTS);
            }

            _lastClickTime = time;
            _lastSelectedName = selection.Name;
        }

        private void Load(ColorSet colorSet, int startSlot, int slotCount) {
            Mod.Static?.LoadColorSet(colorSet.Name, startSlot, slotCount);

            RefreshPalette();
            HudSoundUtils.PlaySound("HudBleep");
        }

        private void OnLoad(object sender, EventArgs args) {
            ColorSet selection;

            if (TryGetSelected(out selection)) {
                Load(selection, 0, ColorSet.SLOTS);
            }
        }

        private void OnLoadFirstRow(object sender, EventArgs args) {
            ColorSet selection;

            if (TryGetSelected(out selection)) {
                Load(selection, 0, ColorSchemeGenerator.RAMP_SLOTS);
            }
        }

        private void OnLoadSecondRow(object sender, EventArgs args) {
            ColorSet selection;

            if (TryGetSelected(out selection)) {
                Load(selection, ColorSchemeGenerator.RAMP_SLOTS, ColorSchemeGenerator.RAMP_SLOTS);
            }
        }

        private void OnSlotClicked(int index) {
            ColorSet selection;

            if (ActiveDialog != null || !TryGetSelected(out selection)) {
                return;
            }

            var set = selection.Upgraded();
            var dialog = new ColorSlotDialog(index, set.Masks[index]);

            dialog.Saved += (sender, args) => {
                set.Masks[index] = dialog.Mask;

                Mod.Static?.SaveColorSet(set);
                Refresh();
            };

            OpenDialog(dialog);
        }

        private void OnSaveCurrentPalette(object sender, EventArgs args) {
            var dialog = new SaveDialog(ModText.BC_UI_SaveCurrentPalette.GetString());

            dialog.Saved += (sender2, args2) => SaveCurrentPaletteAs(dialog.Name);

            OpenDialog(dialog);
        }

        /// <summary>
        /// Saves the palette under a name, asking first when that would replace a set.
        /// </summary>
        private void SaveCurrentPaletteAs(string name) {
            if (string.IsNullOrEmpty(name)) {
                return;
            }

            Action save = () => {
                Mod.Static?.SaveColorSet(name);
                _lastSelectedName = name;

                Refresh();
            };

            if (Mod.Static != null && Mod.Static.HasColorSet(name)) {
                var confirm = new ConfirmDialog(ModText.BC_UI_SaveCurrentPalette.GetString(), ModText.BC_UI_Confirm_Overwrite.GetString(name), ModText.BC_UI_Save.GetString());
                confirm.Confirmed += (sender, args) => save();

                OpenDialog(confirm);
                return;
            }

            save();
        }

        private void OnRename(object sender, EventArgs args) {
            ColorSet original;

            if (!TryGetSelected(out original)) {
                return;
            }

            var dialog = new SaveDialog(ModText.BC_UI_Rename.GetString(), original.Name);

            dialog.Saved += (sender2, args2) => {
                var name = dialog.Name;

                if (string.IsNullOrEmpty(name) || StringComparer.InvariantCultureIgnoreCase.Equals(name, original.Name)) {
                    return;
                }

                Mod.Static?.RemoveColorSet(original.Name);
                Mod.Static?.SaveColorSet(original.WithName(name));
                _lastSelectedName = name;

                Refresh();
            };

            OpenDialog(dialog);
        }

        private void OnDuplicate(object sender, EventArgs args) {
            ColorSet original;

            if (!TryGetSelected(out original)) {
                return;
            }

            var dialog = new SaveDialog(ModText.BC_UI_Duplicate.GetString(), UniqueName(original.Name));

            dialog.Saved += (sender2, args2) => {
                var name = dialog.Name;

                if (string.IsNullOrEmpty(name)) {
                    return;
                }

                var copy = original.Upgraded().WithName(name);
                copy.CreatedTicks = 0L;
                copy.Masks = (ColorMask[])copy.Masks.Clone();

                Mod.Static?.SaveColorSet(copy);
                _lastSelectedName = name;

                Refresh();
            };

            OpenDialog(dialog);
        }

        private void OnToggleFavorite(object sender, EventArgs args) {
            ColorSet selection;

            if (!TryGetSelected(out selection)) {
                return;
            }

            selection.Favorite = !selection.Favorite;

            Mod.Static?.SaveColorSet(selection);
            _lastSelectedName = selection.Name;

            Refresh();
        }

        private void OnExport(object sender, EventArgs args) {
            ColorSet selection;

            if (ActiveDialog != null || !TryGetSelected(out selection)) {
                return;
            }

            var dialog = new CodeDialog(
                ModText.BC_UI_Export.GetString(),
                ModText.BC_UI_ExportHint.GetString(),
                ModText.BC_UI_Done.GetString(),
                ColorSetCode.Encode(selection));

            OpenDialog(dialog);
        }

        private void OnImport(object sender, EventArgs args) {
            if (ActiveDialog != null) {
                return;
            }

            var dialog = new CodeDialog(
                ModText.BC_UI_Import.GetString(),
                ModText.BC_UI_ImportHint.GetString(),
                ModText.BC_UI_Import.GetString());

            dialog.Submitted += (sender2, args2) => {
                ColorSet imported;

                if (!ColorSetCode.TryDecode(dialog.Text, out imported)) {
                    MyAPIGateway.Utilities.ShowMessage(Mod.NAME, ModText.BC_InvalidColorSetCode.GetString());
                    HudSoundUtils.PlaySound("HudLockingLost");
                    return;
                }

                if (string.IsNullOrEmpty(imported.Name)) {
                    imported = imported.WithName(ModText.BC_UI_ImportedSetName.GetString());
                }

                if (Mod.Static != null && Mod.Static.HasColorSet(imported.Name)) {
                    imported = imported.WithName(UniqueName(imported.Name));
                }

                Mod.Static?.SaveColorSet(imported);
                _lastSelectedName = imported.Name;

                Refresh();
            };

            OpenDialog(dialog);
        }

        private void OnRemove(object sender, EventArgs args) {
            ColorSet selection;

            if (ActiveDialog != null || !TryGetSelected(out selection)) {
                return;
            }

            var dialog = new ConfirmDialog(ModText.BC_UI_Remove.GetString(), ModText.BC_UI_Confirm_RemoveColorSet.GetString(selection.Name));

            dialog.Confirmed += (sender2, args2) => {
                Mod.Static?.RemoveColorSet(selection.Name);
                _lastSelectedName = string.Empty;

                Refresh();
            };

            OpenDialog(dialog);
        }

        private string UniqueName(string name) {
            var sets = Mod.Static?.ColorSets;
            var candidate = ModText.BC_UI_CopyOfName.GetString(name);
            var index = 2;

            while (sets != null && sets.Any(set => StringComparer.InvariantCultureIgnoreCase.Equals(set.Name, candidate))) {
                candidate = string.Format("{0} {1}", ModText.BC_UI_CopyOfName.GetString(name), index);
                index++;
            }

            return candidate;
        }
    }
}
