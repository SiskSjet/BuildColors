namespace Sisk.BuildColors.Settings.Models.ColorSpace {

    public struct RGB {

        public RGB(byte r, byte g, byte b) {
            R = r;
            G = g;
            B = b;
        }

        public byte B { get; set; }
        public byte G { get; set; }
        public byte R { get; set; }

        public override string ToString() {
            return string.Format("R: {0}, G: {1}, B: {2}", R, G, B);
        }
    }
}