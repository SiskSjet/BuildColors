namespace Sisk.BuildColors.Settings.Models.ColorSpace {

    public struct XYZ {
        public static readonly XYZ D65 = new XYZ(0.9505, 1.0, 1.0890);

        public XYZ(double x, double y, double z) {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public override string ToString() {
            return string.Format("X: {0}, Y: {1}, Z: {2}", X, Y, Z);
        }
    }
}