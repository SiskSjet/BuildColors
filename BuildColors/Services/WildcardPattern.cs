namespace Sisk.BuildColors.Services {

    /// <summary>
    ///     Case insensitive glob matching with <c>*</c> and <c>?</c>, used by block definition conditions so a
    ///     single condition can cover a family of blocks. Matching walks both strings in place, so it runs on
    ///     every block without allocating.
    /// </summary>
    internal static class WildcardPattern {

        /// <summary>
        ///     True when the value satisfies the pattern. An empty pattern matches anything, which is how an
        ///     empty TypeId or SubtypeId field keeps meaning "any value".
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

            // Position of the last '*' and the value position it started matching at, so the match can back
            // up and let that '*' swallow one more character instead of failing outright.
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

        public static bool ContainsWildcard(string pattern) {
            return !string.IsNullOrEmpty(pattern) && (pattern.IndexOf('*') >= 0 || pattern.IndexOf('?') >= 0);
        }

        private static bool CharEquals(char left, char right) {
            return left == right || char.ToUpperInvariant(left) == char.ToUpperInvariant(right);
        }
    }
}
