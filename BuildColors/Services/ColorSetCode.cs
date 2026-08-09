using Sisk.BuildColors.Settings.Models;
using System;
using System.Text;

namespace Sisk.BuildColors.Services {

    /// <summary>
    /// Packs a color set into a short piece of text and reads it back.
    /// </summary>
    public static class ColorSetCode {
        /// <summary>
        /// Marks the text as a color set and says which layout it uses.
        /// </summary>
        public const string PREFIX = "BC1:";

        /// <summary>
        /// Channels are stored as sixteen bit fractions.
        /// </summary>
        private const float SCALE = 65535f;

        private const int MAX_NAME_LENGTH = 64;

        public static string Encode(ColorSet colorSet) {
            var set = colorSet.Upgraded();
            var name = set.Name ?? string.Empty;

            if (name.Length > MAX_NAME_LENGTH) {
                name = name.Substring(0, MAX_NAME_LENGTH);
            }

            var nameBytes = Encoding.UTF8.GetBytes(name);
            var payload = new byte[2 + nameBytes.Length + ColorSet.SLOTS * 6];
            var offset = 0;

            payload[offset++] = 1;
            payload[offset++] = (byte)nameBytes.Length;

            Array.Copy(nameBytes, 0, payload, offset, nameBytes.Length);
            offset += nameBytes.Length;

            for (var i = 0; i < ColorSet.SLOTS; i++) {
                var mask = i < set.Masks.Length ? set.Masks[i] : default(ColorMask);

                offset = WriteChannel(payload, offset, mask.H, 0f, 1f);
                offset = WriteChannel(payload, offset, mask.S, -1f, 1f);
                offset = WriteChannel(payload, offset, mask.V, -1f, 1f);
            }

            return PREFIX + ToUrlSafeBase64(payload);
        }

        public static bool TryDecode(string code, out ColorSet colorSet) {
            colorSet = default(ColorSet);

            if (string.IsNullOrEmpty(code)) {
                return false;
            }

            var trimmed = code.Trim();

            if (trimmed.StartsWith(PREFIX, StringComparison.OrdinalIgnoreCase)) {
                trimmed = trimmed.Substring(PREFIX.Length);
            }

            byte[] payload;
            if (!TryFromUrlSafeBase64(trimmed, out payload) || payload.Length < 2) {
                return false;
            }

            var offset = 0;
            var version = payload[offset++];

            if (version != 1) {
                return false;
            }

            var nameLength = payload[offset++];

            if (payload.Length < offset + nameLength + ColorSet.SLOTS * 6) {
                return false;
            }

            var name = Encoding.UTF8.GetString(payload, offset, nameLength);
            offset += nameLength;

            var masks = new ColorMask[ColorSet.SLOTS];

            for (var i = 0; i < ColorSet.SLOTS; i++) {
                float hue, saturation, value;

                offset = ReadChannel(payload, offset, 0f, 1f, out hue);
                offset = ReadChannel(payload, offset, -1f, 1f, out saturation);
                offset = ReadChannel(payload, offset, -1f, 1f, out value);

                masks[i] = new ColorMask(hue, saturation, value);
            }

            colorSet = new ColorSet(name, masks);

            return true;
        }

        private static int WriteChannel(byte[] payload, int offset, float value, float min, float max) {
            var normalized = (value - min) / (max - min);
            var scaled = (ushort)Math.Round(Math.Max(0f, Math.Min(1f, normalized)) * SCALE);

            payload[offset++] = (byte)(scaled >> 8);
            payload[offset++] = (byte)(scaled & 0xFF);

            return offset;
        }

        private static int ReadChannel(byte[] payload, int offset, float min, float max, out float value) {
            var scaled = (ushort)((payload[offset] << 8) | payload[offset + 1]);
            value = min + (max - min) * (scaled / SCALE);

            return offset + 2;
        }

        /// <summary>
        /// Base64 without the characters that a chat line or a URL would mangle.
        /// </summary>
        private static string ToUrlSafeBase64(byte[] payload) {
            return Convert.ToBase64String(payload).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static bool TryFromUrlSafeBase64(string text, out byte[] payload) {
            payload = null;

            var restored = text.Replace('-', '+').Replace('_', '/');

            switch (restored.Length % 4) {
                case 2:
                    restored += "==";
                    break;

                case 3:
                    restored += "=";
                    break;

                case 1:
                    return false;
            }

            try {
                payload = Convert.FromBase64String(restored);
            } catch (FormatException) {
                return false;
            }

            return true;
        }
    }
}
