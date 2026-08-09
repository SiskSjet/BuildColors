using ProtoBuf;
using System.Xml.Serialization;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    [ProtoContract]
    public class PaintJobOptions {
        [ProtoMember(1)]
        [XmlAttribute("includeSubgrids")]
        public bool IncludeSubgrids { get; set; } = true;

        [ProtoMember(2)]
        [XmlAttribute("includeProjected")]
        public bool IncludeProjectedGrids { get; set; }

        [ProtoMember(3)]
        [XmlAttribute("affectPreview")]
        public bool IncludePreviewGrids { get; set; }

        /// <summary>
        /// Creates an independent copy.
        /// </summary>
        public PaintJobOptions Clone() {
            return new PaintJobOptions {
                IncludeSubgrids = IncludeSubgrids,
                IncludeProjectedGrids = IncludeProjectedGrids,
                IncludePreviewGrids = IncludePreviewGrids
            };
        }
    }
}
