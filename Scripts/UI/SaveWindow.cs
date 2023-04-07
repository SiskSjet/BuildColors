using RichHudFramework.UI;
using System;
using VRageMath;

namespace Sisk.BuildColors.UI {

    public class SaveWindow : WindowBase {
        private readonly TextField _text;
        private Settings.Models.ColorSet _colorset;

        public SaveWindow(HudParentBase parent = null, Settings.Models.ColorSet? colorSet = null) : base(parent) {
            if (colorSet.HasValue) {
                _colorset = colorSet.Value;
            }

            BodyColor = new Color(41, 54, 62, 150);
            BorderColor = new Color(58, 68, 77);
            AllowResizing = false;
            Size = new Vector2(330, 150f);
            Padding = new Vector2(10f, 10f);
            HeaderText = "Name your color set";
            ZOffset = 0;

            _text = new TextField() {
                Text = "",
                Width = 310
            };

            var save = new BorderedButton() {
                Text = "Save",
                Padding = Vector2.Zero,
                Width = 150
            };

            var cancel = new BorderedButton() {
                Text = "Cancel",
                Padding = Vector2.Zero,
                Width = 150
            };

            var controls = new HudChain(false) {
                DimAlignment = DimAlignments.Width,
                CollectionContainer = { save, cancel },
                Spacing = 8f,
            };

            var layout = new HudChain(true) {
                ParentAlignment = ParentAlignments.Inner,
                DimAlignment = DimAlignments.Width,
                CollectionContainer = { _text, controls },
                Spacing = 10f,
            };

            layout.Register(body);

            save.MouseInput.LeftClicked += OnSaveClicked;
            cancel.MouseInput.LeftClicked += OnCancelClicked;
        }

        public event RichHudFramework.EventHandler SaveClicked;

        public Settings.Models.ColorSet ColorSet {
            get { return _colorset; }
        }

        public string Name {
            get { return _text.Text.ToString(); }
        }

        private void OnCancelClicked(object sender, EventArgs e) {
            Unregister();
        }

        private void OnSaveClicked(object sender, EventArgs e) {
            Unregister();
            SaveClicked?.Invoke(this, e);
        }
    }
}