using System;
using System.Collections.Generic;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    public class PaintJobComparer : IEqualityComparer<PaintJob> {

        public bool Equals(PaintJob x, PaintJob y) {
            if (ReferenceEquals(x, y)) {
                return true;
            }

            if (x == null || y == null) {
                return false;
            }

            return x.Id == y.Id;
        }

        public int GetHashCode(PaintJob obj) {
            return obj.Id.GetHashCode();
        }
    }
}
