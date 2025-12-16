using ProtoBuf;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    [ProtoContract]
    public class PaintRuleConditionGroup {

        public static PaintRuleConditionGroup CreateDefault() {
            return new PaintRuleConditionGroup();
        }

        [ProtoMember(1)]
        [XmlAttribute("operator")]
        public PaintRuleLogicalOperator Operator { get; set; } = PaintRuleLogicalOperator.And;

        [ProtoMember(2)]
        [XmlArray(Order = 2)]
        [XmlArrayItem]
        public List<PaintRuleCondition> Conditions { get; set; } = new List<PaintRuleCondition>();

        [ProtoMember(3)]
        [XmlArray(Order = 3)]
        [XmlArrayItem]
        public List<PaintRuleConditionGroup> Children { get; set; } = new List<PaintRuleConditionGroup>();

        /// <summary>
        ///     Creates an independent copy of this group and everything nested inside it.
        /// </summary>
        public PaintRuleConditionGroup Clone() {
            var clone = new PaintRuleConditionGroup {
                Operator = Operator,
                Conditions = new List<PaintRuleCondition>(),
                Children = new List<PaintRuleConditionGroup>()
            };

            if (Conditions != null) {
                foreach (var condition in Conditions) {
                    clone.Conditions.Add(condition.Clone());
                }
            }

            if (Children != null) {
                foreach (var child in Children) {
                    clone.Children.Add(child.Clone());
                }
            }

            return clone;
        }
    }
}
