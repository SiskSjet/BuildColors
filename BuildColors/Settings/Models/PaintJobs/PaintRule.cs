using ProtoBuf;
using Sisk.BuildColors.Localization;
using Sisk.Utils.Localization.Extensions;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    [ProtoContract]
    public class PaintRule : IEquatable<PaintRule> {

        public PaintRule() {
            Id = Guid.NewGuid();
            Name = ModText.BC_UI_DefaultRuleNameFallback.GetString();
            ConditionGroup = PaintRuleConditionGroup.CreateDefault();
            Action = new PaintRuleAction();
        }

        [ProtoMember(1)]
        [XmlAttribute("id")]
        public Guid Id { get; set; }

        [ProtoMember(2)]
        [XmlAttribute("name")]
        public string Name { get; set; }

        [ProtoMember(3)]
        [XmlElement(Order = 3)]
        public PaintRuleConditionGroup ConditionGroup { get; set; }

        [ProtoMember(4)]
        [XmlElement(Order = 4)]
        public PaintRuleAction Action { get; set; }

        public static PaintRule CreateDefault(string name = null) {
            var rule = new PaintRule();
            if (!string.IsNullOrWhiteSpace(name)) {
                rule.Name = name;
            }

            if (rule.ConditionGroup == null) {
                rule.ConditionGroup = PaintRuleConditionGroup.CreateDefault();
            }

            return rule;
        }

        /// <summary>
        ///     Creates an independent copy, keeping the identity of the original.
        /// </summary>
        public PaintRule Clone() {
            return new PaintRule {
                Id = Id,
                Name = Name,
                ConditionGroup = ConditionGroup != null ? ConditionGroup.Clone() : PaintRuleConditionGroup.CreateDefault(),
                Action = Action != null ? Action.Clone() : new PaintRuleAction()
            };
        }

        public bool Equals(PaintRule other) {
            if (ReferenceEquals(this, other)) {
                return true;
            }

            if (other == null) {
                return false;
            }

            return Id == other.Id;
        }

        public override string ToString() {
            return string.IsNullOrWhiteSpace(Name) ? ModText.BC_UI_DefaultRuleNameFallback.GetString() : Name;
        }
    }
}
