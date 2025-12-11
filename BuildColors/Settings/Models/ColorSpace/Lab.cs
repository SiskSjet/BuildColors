namespace Sisk.BuildColors.Settings.Models.ColorSpace {

    public struct Lab {

        public Lab(float l, float a, float b) {
            L = l;
            A = a;
            B = b;
        }

        public float A { get; set; }
        public float B { get; set; }
        public float L { get; set; }

        public override string ToString() {
            return string.Format("L: {0}, A: {1}, B: {2}", L, A, B);
        }
    }
}