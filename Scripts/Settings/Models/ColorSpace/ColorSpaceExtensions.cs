using System;
using System.Collections.Generic;
using System.Linq;

namespace Sisk.BuildColors.Settings.Models.ColorSpace {

    public static class ColorSpaceExtensions {

        public static double GetDistance(this RGB rgb, RGB other) {
            return Math.Sqrt(Math.Pow(rgb.R - other.R, 2) + Math.Pow(rgb.G - other.G, 2) + Math.Pow(rgb.B - other.B, 2));
        }

        public static double GetDistance(this HSL hsl, HSL other) {
            return Math.Sqrt(Math.Pow(hsl.H - other.H, 2) + Math.Pow(hsl.S - other.S, 2) + Math.Pow(hsl.L - other.L, 2));
        }

        public static double GetDistance(this HSV hsv, HSV other) {
            return Math.Sqrt(Math.Pow(hsv.H - other.H, 2) + Math.Pow(hsv.S - other.S, 2) + Math.Pow(hsv.V - other.V, 2));
        }

        public static double GetDistance(this XYZ xyz, XYZ other) {
            return Math.Sqrt(Math.Pow(xyz.X - other.X, 2) + Math.Pow(xyz.Y - other.Y, 2) + Math.Pow(xyz.Z - other.Z, 2));
        }

        public static double GetDistance(this Lab lab, Lab other) {
            return Math.Sqrt(Math.Pow(lab.L - other.L, 2) + Math.Pow(lab.A - other.A, 2) + Math.Pow(lab.B - other.B, 2));
        }

        public static HSL ToHSL(this HSV hsv) {
            double modifiedS, modifiedV, hslS, hslL;

            modifiedS = hsv.S;
            modifiedV = hsv.V;

            hslL = modifiedV * (1 - modifiedS / 2);
            hslS = (hslL == 0 || hslL == 1) ? 0 : (modifiedV - hslL) / Math.Min(hslL, 1 - hslL);

            return new HSL(hsv.H, (float)hslS, (float)hslL);
        }

        public static HSL ToHSL(this RGB rgb) {
            double modifiedR, modifiedG, modifiedB, min, max, delta, h, s, l;

            modifiedR = rgb.R / 255.0;
            modifiedG = rgb.G / 255.0;
            modifiedB = rgb.B / 255.0;

            min = new List<double>() { modifiedR, modifiedG, modifiedB }.Min();
            max = new List<double>() { modifiedR, modifiedG, modifiedB }.Max();
            delta = max - min;
            l = (min + max) / 2;

            if (delta == 0) {
                h = 0;
                s = 0;
            } else {
                s = (l <= 0.5) ? (delta / (min + max)) : (delta / (2 - max - min));

                if (modifiedR == max) {
                    h = (modifiedG - modifiedB) / 6 / delta;
                } else if (modifiedG == max) {
                    h = (1.0 / 3) + ((modifiedB - modifiedR) / 6 / delta);
                } else {
                    h = (2.0 / 3) + ((modifiedR - modifiedG) / 6 / delta);
                }

                h = (h < 0) ? ++h : h;
                h = (h > 1) ? --h : h;
            }

            return new HSL((float)h * 360, (float)s, (float)l);
        }

        public static HSL ToHSL(this XYZ xyz) {
            return ToHSL(ToRGB(xyz));
        }

        public static HSL ToHSL(this Lab lab) {
            return ToHSL(ToRGB(lab));
        }

        public static HSV ToHSV(this XYZ xyz) {
            return ToHSV(ToRGB(xyz));
        }

        public static HSV ToHSV(this RGB rgb) {
            return ToHSV(ToHSL(rgb));
        }

        public static HSV ToHSV(this Lab lab) {
            return ToHSV(ToHSL(lab));
        }

        public static HSV ToHSV(this HSL hsl) {
            double modifiedS, modifiedL, hsvS, hsvV;

            modifiedS = hsl.S;
            modifiedL = hsl.L;

            hsvV = modifiedL + modifiedS * Math.Min(modifiedL, 1 - modifiedL);

            hsvS = (hsvV == 0) ? 0 : 2 * (1 - modifiedL / hsvV);

            return new HSV(hsl.H, (float)hsvS, (float)hsvV);
        }

        public static Lab ToLab(this HSV hsv) {
            return ToLab(ToXYZ(hsv));
        }

        public static Lab ToLab(this HSL hsl) {
            return ToLab(ToXYZ(hsl));
        }

        public static Lab ToLab(this RGB rgb) {
            return ToLab(ToXYZ(rgb));
        }

        public static Lab ToLab(this XYZ xyz) {
            double modifiedX = xyz.X / XYZ.D65.X, modifiedY = xyz.Y / XYZ.D65.Y, modifiedZ = xyz.Z / XYZ.D65.Z;
            modifiedX = XYZToLabTransform(modifiedX);
            modifiedY = XYZToLabTransform(modifiedY);
            modifiedZ = XYZToLabTransform(modifiedZ);

            return new Lab((byte)Math.Round((116 * modifiedY) - 16), (byte)Math.Round(500 * (modifiedX - modifiedY)), (byte)Math.Round(200 * (modifiedY - modifiedZ)));
        }

        public static RGB ToRGB(this HSV hsv) {
            return ToRGB(ToHSL(hsv));
        }

        public static RGB ToRGB(this Lab lab) {
            return ToRGB(ToXYZ(lab));
        }

        public static RGB ToRGB(this XYZ xyz) {
            double modifiedX = xyz.X / 100.0, modifiedY = xyz.Y / 100.0, modifiedZ = xyz.Z / 100.0;

            var rgb = new double[3];
            rgb[0] = modifiedX * 3.2410 + modifiedY * (-1.5374) + modifiedZ * (-0.4986);
            rgb[1] = modifiedX * (-0.9692) + modifiedY * 1.8760 + modifiedZ * 0.0416;
            rgb[2] = modifiedX * 0.056 + modifiedY * (-0.2040) + modifiedZ * 1.0570;

            for (var x = 0; x < rgb.Length; x++) {
                rgb[x] = (rgb[x] <= 0.0031308) ? 12.92 * rgb[x] : 1.055 * Math.Pow(rgb[x], 0.41666666666) - 0.055;
            }

            return new RGB((byte)Math.Round(rgb[0] * 255), (byte)Math.Round(rgb[1] * 255), (byte)Math.Round(rgb[2] * 255));
        }

        public static RGB ToRGB(this HSL hsl) {
            double modifiedH, modifiedS, modifiedL, r = 1, g = 1, b = 1, q, p;

            modifiedH = hsl.H / 360.0;
            modifiedS = hsl.S;
            modifiedL = hsl.L;

            q = (modifiedL < 0.5) ? modifiedL * (1 + modifiedS) : modifiedL + modifiedS - modifiedL * modifiedS;
            p = 2 * modifiedL - q;

            if (modifiedL == 0) {
                // if the lightness value is 0 it will always be black
                r = 0;
                g = 0;
                b = 0;
            } else if (modifiedS != 0) {
                r = GetHue(p, q, modifiedH + 1.0 / 3);
                g = GetHue(p, q, modifiedH);
                b = GetHue(p, q, modifiedH - 1.0 / 3);
            } else {
                // ensure greys are not converted to white
                r = modifiedL;
                g = modifiedL;
                b = modifiedL;
            }

            return new RGB((byte)Math.Round(r * 255), (byte)Math.Round(g * 255), (byte)Math.Round(b * 255));
        }

        public static XYZ ToXYZ(this HSV hsv) {
            return ToXYZ(ToRGB(hsv));
        }

        public static XYZ ToXYZ(this HSL hsl) {
            return ToXYZ(ToRGB(hsl));
        }

        public static XYZ ToXYZ(this Lab lab) {
            var theta = 6.0 / 29.0;

            var fy = (lab.L + 16) / 116.0;
            var fx = fy + (lab.A / 500.0);
            var fz = fy - (lab.B / 200.0);

            return new XYZ(
                (fx > theta) ? XYZ.D65.X * (fx * fx * fx) : (fx - 16.0 / 116.0) * 3 * (theta * theta) * XYZ.D65.X,
                (fy > theta) ? XYZ.D65.Y * (fy * fy * fy) : (fy - 16.0 / 116.0) * 3 * (theta * theta) * XYZ.D65.Y,
                (fz > theta) ? XYZ.D65.Z * (fz * fz * fz) : (fz - 16.0 / 116.0) * 3 * (theta * theta) * XYZ.D65.Z
            );
        }

        public static XYZ ToXYZ(this RGB rgb) {
            double[] modifiedRGB = { rgb.R / 255.0, rgb.G / 255.0, rgb.B / 255.0 };

            for (var x = 0; x < modifiedRGB.Length; x++) {
                modifiedRGB[x] =
                    (modifiedRGB[x] > 0.04045) ?
                        Math.Pow((modifiedRGB[x] + 0.055) / 1.055, 2.4) :
                        (modifiedRGB[x] / 12.92);

                modifiedRGB[x] *= 100;
            }

            return new XYZ(
                (modifiedRGB[0] * 0.4124 + modifiedRGB[1] * 0.3576 + modifiedRGB[2] * 0.1805),
                (modifiedRGB[0] * 0.2126 + modifiedRGB[1] * 0.7152 + modifiedRGB[2] * 0.0722),
                (modifiedRGB[0] * 0.0193 + modifiedRGB[1] * 0.1192 + modifiedRGB[2] * 0.9505)
            );
        }

        private static double GetHue(double p, double q, double t) {
            var value = p;

            if (t < 0) {
                t++;
            }

            if (t > 1) {
                t--;
            }

            if (t < 1.0 / 6) {
                value = p + (q - p) * 6 * t;
            } else if (t < 1.0 / 2) {
                value = q;
            } else if (t < 2.0 / 3) {
                value = p + (q - p) * (2.0 / 3 - t) * 6;
            }

            return value;
        }

        private static double XYZToLabTransform(double t) {
            return ((t > 0.008856) ? Math.Pow(t, (1.0 / 3.0)) : (7.787 * t + 16.0 / 116.0));
        }
    }
}