using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using ColorModel = Sisk.BuildColors.Settings.Models.Color;

namespace Sisk.BuildColors.Services {

    /// <summary>
    ///     Turns the raw argument string of a chat command into tokens and reads the value forms the paint
    ///     job commands accept. Names hold spaces far more often than not, so a token is quoted with double
    ///     quotes and a doubled quote inside a quoted token stands for a literal one.
    /// </summary>
    internal static class CommandArguments {

        public static List<string> Split(string arguments) {
            var tokens = new List<string>();

            if (string.IsNullOrWhiteSpace(arguments)) {
                return tokens;
            }

            var builder = new StringBuilder();
            var inQuotes = false;
            var hasToken = false;

            for (var i = 0; i < arguments.Length; i++) {
                var character = arguments[i];

                if (character == '"') {
                    if (inQuotes && i + 1 < arguments.Length && arguments[i + 1] == '"') {
                        builder.Append('"');
                        i++;
                        continue;
                    }

                    inQuotes = !inQuotes;

                    // An empty pair of quotes is a token, which is how an empty name or skin is passed.
                    hasToken = true;
                    continue;
                }

                if (!inQuotes && char.IsWhiteSpace(character)) {
                    if (hasToken) {
                        tokens.Add(builder.ToString());
                        builder.Clear();
                        hasToken = false;
                    }

                    continue;
                }

                builder.Append(character);
                hasToken = true;
            }

            if (hasToken) {
                tokens.Add(builder.ToString());
            }

            return tokens;
        }

        /// <summary>
        ///     Splits a <c>field=value</c> token at its first equals sign. Values may hold further equals
        ///     signs, which keeps skin ids and definition names intact.
        /// </summary>
        public static bool TrySplitAssignment(string token, out string field, out string value) {
            field = null;
            value = null;

            if (string.IsNullOrWhiteSpace(token)) {
                return false;
            }

            var separator = token.IndexOf('=');
            if (separator <= 0) {
                return false;
            }

            field = token.Substring(0, separator).Trim().ToLowerInvariant();
            value = token.Substring(separator + 1).Trim();

            return field.Length > 0;
        }

        /// <summary>
        ///     Reads an on/off value. <c>toggle</c> flips <paramref name="current" />, which lets a command
        ///     switch an option without the caller having to know its state.
        /// </summary>
        public static bool TryParseFlag(string value, bool current, out bool result) {
            result = current;

            if (string.IsNullOrWhiteSpace(value)) {
                return false;
            }

            switch (value.Trim().ToLowerInvariant()) {
                case "on":
                case "true":
                case "yes":
                case "y":
                case "1":
                    result = true;
                    return true;

                case "off":
                case "false":
                case "no":
                case "n":
                case "0":
                    result = false;
                    return true;

                case "toggle":
                    result = !current;
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Reads a color as <c>R,G,B</c> or as a hex triplet with or without a leading hash.
        /// </summary>
        public static bool TryParseColor(string value, out ColorModel color) {
            color = new ColorModel();

            if (string.IsNullOrWhiteSpace(value)) {
                return false;
            }

            var text = value.Trim();
            byte r;
            byte g;
            byte b;

            if (text.IndexOf(',') < 0) {
                if (text.StartsWith("#", StringComparison.Ordinal)) {
                    text = text.Substring(1);
                }

                if (text.Length != 6) {
                    return false;
                }

                if (!byte.TryParse(text.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out r)
                    || !byte.TryParse(text.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out g)
                    || !byte.TryParse(text.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out b)) {
                    return false;
                }

                color = new ColorModel(r, g, b);
                return true;
            }

            var parts = text.Split(',');
            if (parts.Length != 3) {
                return false;
            }

            if (!byte.TryParse(parts[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out r)
                || !byte.TryParse(parts[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out g)
                || !byte.TryParse(parts[2].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out b)) {
                return false;
            }

            color = new ColorModel(r, g, b);
            return true;
        }

        public static bool TryParseInteger(string value, out int result) {
            return int.TryParse(value != null ? value.Trim() : null, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        public static bool TryParsePercentage(string value, out float result) {
            return float.TryParse(value != null ? value.Trim() : null, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
        }

        /// <summary>
        ///     Reads a <c>#3</c> style selector. Positions are 1 based because that is how the listings number
        ///     their entries.
        /// </summary>
        public static bool TryParseSelector(string value, out int position) {
            position = 0;

            if (string.IsNullOrWhiteSpace(value) || value[0] != '#') {
                return false;
            }

            return TryParseInteger(value.Substring(1), out position);
        }
    }
}
