namespace Sisk.BuildColors.Settings.Models.ColorSpace {

    public struct HSV {

        public HSV(float h, float s, float v) {
            H = h;
            S = s;
            V = v;
        }

        public float H { get; set; }
        public float S { get; set; }
        public float V { get; set; }

        public override string ToString() {
            return string.Format("H: {0}, S: {1}, V: {2}", H, S, V);
        }
    }
}