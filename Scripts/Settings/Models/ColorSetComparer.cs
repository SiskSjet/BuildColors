using System;
using System.Collections.Generic;

namespace Sisk.BuildColors.Settings.Models {
    public class ColorSetComparer : IEqualityComparer<ColorSet> {
        public bool Equals(ColorSet set, ColorSet set2) {
            return StringComparer.InvariantCultureIgnoreCase.Equals(set.Name, set2.Name);
        }

        public int GetHashCode(ColorSet item) {
            return StringComparer.InvariantCultureIgnoreCase.GetHashCode(item.Name);
        }
    }
}