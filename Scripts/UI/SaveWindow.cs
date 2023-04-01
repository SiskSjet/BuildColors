using RichHudFramework;
using RichHudFramework.UI;
using System;
using VRageMath;

namespace Sisk.BuildColors.UI {
    public class SaveWindow : WindowBase {
        private readonly TextField _text;

        public SaveWindow(HudParentBase parent = null) : base(parent) {
            Size = new Vector2(400f, 200f);
            BodyColor = new Color(41, 54, 62, 150);
            BorderColor = new Color(58, 68, 77);
            AllowResizing = false;
            HeaderText = "Name your color set";

            _text = new TextField() {
                Text = "",
            };

            var save = new BorderedButton() {
                Text = "Save",
            };

            var cancel = new BorderedButton() {
                Text = "Cancel",
            };

            var controls = new HudChain(false) {
                CollectionContainer = { save, cancel },
                Spacing = 8f,
            };

            var layout = new HudChain(true) {
                CollectionContainer = { _text, controls },
                Spacing = 10f,
            };

            layout.Register(body);

            save.MouseInput.LeftClicked += OnSaveClicked;
            cancel.MouseInput.LeftClicked += OnCancelClicked;
        }

        public event RichHudFramework.EventHandler SaveClicked;

        private void OnCancelClicked(object sender, EventArgs e) {
            Unregister();
        }

        private void OnSaveClicked(object sender, EventArgs e) {
            var name = _text.Text.ToString();
            Mod.Static.SaveColorSet(name);
            Unregister();
            SaveClicked?.Invoke(this, e);
        }
    }
}