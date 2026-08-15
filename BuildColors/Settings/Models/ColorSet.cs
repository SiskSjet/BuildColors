using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using VRageMath;

// ReSharper disable ExplicitCallerInfoArgument

namespace Sisk.BuildColors.Settings.Models {

    /// <summary>
    /// A named set of build color slots.
    /// </summary>
    [ProtoContract]
    public struct ColorSet : IEquatable<ColorSet>, IEquatable<string> {
        /// <summary>
        /// Number of build color slots the game holds.
        /// </summary>
        public const int SLOTS = 14;

        public ColorSet(string name, ColorMask[] masks) {
            Name = name;
            Masks = masks;
            Hsv = null;
            Colors = null;
            LegacyMasks = null;
            CreatedTicks = DateTime.UtcNow.Ticks;
            Tags = null;
            Favorite = false;
        }

        [ProtoMember(1)]
        [XmlElement(Order = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Colors as written before version 2.
        /// </summary>
        [ProtoMember(2)]
        [XmlArray(Order = 2)]
        [XmlArrayItem]
        public Color[] Colors { get; set; }

        /// <summary>
        /// Slots as the game's offset mask, written before version 3.
        /// </summary>
        [ProtoMember(3)]
        [XmlArray(ElementName = "Masks", Order = 3)]
        [XmlArrayItem]
        public ColorMask[] LegacyMasks { get; set; }

        /// <summary>
        /// The slots as Space Engineers' own color picker shows them.
        /// </summary>
        [ProtoMember(7)]
        [XmlArray(Order = 7)]
        [XmlArrayItem]
        public SeHsv[] Hsv { get; set; }

        /// <summary>
        /// The slots the game holds, resolved from whichever saved format is present. Not itself saved;
        /// <see cref="Upgraded"/> keeps <see cref="Hsv"/>.
        /// </summary>
        [XmlIgnore]
        public ColorMask[] Masks { get; set; }

        [ProtoMember(4)]
        [XmlElement(Order = 4)]
        public long CreatedTicks { get; set; }

        [ProtoMember(5)]
        [XmlArray(Order = 5)]
        [XmlArrayItem]
        public string[] Tags { get; set; }

        [ProtoMember(6)]
        [XmlElement(Order = 6)]
        public bool Favorite { get; set; }

        /// <summary>
        /// The slots to paint with, padded so a short or malformed set cannot shrink the palette.
        /// </summary>
        public List<Vector3> ToBuildColorSlots() {
            var masks = ResolveMasks();
            var slots = new List<Vector3>(SLOTS);

            for (var i = 0; i < SLOTS; i++) {
                slots.Add(i < masks.Length ? (Vector3)masks[i] : Vector3.Zero);
            }

            return slots;
        }

        /// <summary>
        /// A copy with the masks filled in and padded, so everything downstream can assume they are there,
        /// and re-saved as SE HSV regardless of which format it was loaded from.
        /// </summary>
        public ColorSet Upgraded() {
            var copy = this;
            var masks = ResolveMasks();

            if (masks.Length < SLOTS) {
                Array.Resize(ref masks, SLOTS);
            }

            copy.Masks = masks;
            copy.Hsv = masks.Select(mask => (SeHsv)mask).ToArray();
            copy.Colors = null;
            copy.LegacyMasks = null;

            return copy;
        }

        /// <summary>
        /// Whether any saved format, current or legacy, actually holds colors.
        /// </summary>
        public bool HasSavedColors() {
            return (Masks != null && Masks.Length > 0)
                || (Hsv != null && Hsv.Length > 0)
                || (LegacyMasks != null && LegacyMasks.Length > 0)
                || (Colors != null && Colors.Length > 0);
        }

        public ColorSet WithName(string name) {
            var copy = this;
            copy.Name = name;

            return copy;
        }

        public bool Equals(ColorSet other) {
            return StringComparer.InvariantCultureIgnoreCase.Equals(Name, other.Name);
        }

        public bool Equals(string other) {
            return StringComparer.InvariantCultureIgnoreCase.Equals(Name, other);
        }

        /// <summary>
        /// The masks, preferring the current in-memory ones, then each older saved format in turn.
        /// </summary>
        private ColorMask[] ResolveMasks() {
            if (Masks != null && Masks.Length > 0) {
                return Masks;
            }

            if (Hsv != null && Hsv.Length > 0) {
                return Hsv.Select(hsv => (ColorMask)hsv).ToArray();
            }

            if (LegacyMasks != null && LegacyMasks.Length > 0) {
                return LegacyMasks;
            }

            if (Colors == null) {
                return new ColorMask[0];
            }

            var masks = new ColorMask[Colors.Length];

            for (var i = 0; i < Colors.Length; i++) {
                masks[i] = ColorMask.FromColor(Colors[i]);
            }

            return masks;
        }
    }
}
