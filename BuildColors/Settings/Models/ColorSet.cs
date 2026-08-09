using ProtoBuf;
using System;
using System.Collections.Generic;
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
            Colors = null;
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
        /// The slots as the game holds them.
        /// </summary>
        [ProtoMember(3)]
        [XmlArray(Order = 3)]
        [XmlArrayItem]
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
        /// A copy with the masks filled in and padded, so everything downstream can assume they are there.
        /// </summary>
        public ColorSet Upgraded() {
            var copy = this;
            var masks = ResolveMasks();

            if (masks.Length < SLOTS) {
                Array.Resize(ref masks, SLOTS);
            }

            copy.Masks = masks;
            copy.Colors = null;

            return copy;
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
        /// The masks, worked out from the legacy colors when a file written before version 2 has none.
        /// </summary>
        private ColorMask[] ResolveMasks() {
            if (Masks != null && Masks.Length > 0) {
                return Masks;
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
