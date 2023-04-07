using System.Collections.Generic;

namespace Sisk.BuildColors.Settings.Models {

    public class PlayerColorComparer : IEqualityComparer<PlayerColors> {

        public bool Equals(PlayerColors colors, PlayerColors colors2) {
            return colors.Id.Equals(colors2.Id);
        }

        public int GetHashCode(PlayerColors colors) {
            return colors.Id.GetHashCode();
        }
    }
}