namespace Sisk.BuildColors.Services {

    /// <summary>
    /// Case insensitive glob matching with * and ?.
    /// </summary>
    internal static class WildcardPattern {
        /// <summary>
        /// True when the value satisfies the pattern.
        /// </summary>
        public static bool Matches(string pattern, string value) {
            if (string.IsNullOrEmpty(pattern)) {
                return true;
            }

            if (value == null) {
                value = string.Empty;
            }

            var patternIndex = 0;
            var valueIndex = 0;

            var starIndex = -1;
            var starValueIndex = 0;

            while (valueIndex < value.Length) {
                var patternChar = patternIndex < pattern.Length ? pattern[patternIndex] : '\0';

                if (patternIndex < pattern.Length && (patternChar == '?' || CharEquals(patternChar, value[valueIndex]))) {
                    patternIndex++;
                    valueIndex++;
                } else if (patternIndex < pattern.Length && patternChar == '*') {
                    starIndex = patternIndex;
                    starValueIndex = valueIndex;
                    patternIndex++;
                } else if (starIndex >= 0) {
                    patternIndex = starIndex + 1;
                    starValueIndex++;
                    valueIndex = starValueIndex;
                } else {
                    return false;
                }
            }

            while (patternIndex < pattern.Length && pattern[patternIndex] == '*') {
                patternIndex++;
            }

            return patternIndex == pattern.Length;
        }

        private static bool CharEquals(char left, char right) {
            return left == right || char.ToUpperInvariant(left) == char.ToUpperInvariant(right);
        }
    }
}
