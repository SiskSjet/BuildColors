namespace Sisk.BuildColors.Settings.Models.ColorSpace {

    public struct HSL {

        public HSL(float h, float s, float l) {
            H = h;
            S = s;
            L = l;
        }

        public float H { get; set; }
        public float L { get; set; }
        public float S { get; set; }

        public override string ToString() {
            return string.Format("H: {0}, S: {1}, L: {2}", H, S, L);
        }
    }
}