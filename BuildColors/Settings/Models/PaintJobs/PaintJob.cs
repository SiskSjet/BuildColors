using ProtoBuf;
using Sisk.BuildColors.Localization;
using Sisk.Utils.Localization.Extensions;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    [ProtoContract]
    public class PaintJob : IEquatable<PaintJob> {
        public PaintJob() {
            Rules = new List<PaintRule>();
            Options = new PaintJobOptions();
        }

        [ProtoMember(1)]
        [XmlAttribute("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [ProtoMember(2)]
        [XmlAttribute("name")]
        public string Name { get; set; } = ModText.BC_UI_NewPaintJobName.GetString();

        [ProtoMember(3)]
        [XmlElement(Order = 3)]
        public PaintJobOptions Options { get; set; }

        [ProtoMember(4)]
        [XmlAttribute("enabled")]
        public bool Enabled { get; set; } = true;

        [ProtoMember(6)]
        [XmlArray(Order = 4)]
        [XmlArrayItem("Rule")]
        public List<PaintRule> Rules { get; set; }

        /// <summary>
        /// Creates an independent copy that keeps the original's identity.
        /// </summary>
        public PaintJob Clone() {
            var clone = new PaintJob {
                Id = Id,
                Name = Name,
                Enabled = Enabled,
                Options = Options != null ? Options.Clone() : new PaintJobOptions(),
                Rules = new List<PaintRule>()
            };

            if (Rules != null) {
                foreach (var rule in Rules) {
                    clone.Rules.Add(rule.Clone());
                }
            }

            clone.EnsureRules();

            return clone;
        }

        public bool Equals(PaintJob other) {
            if (ReferenceEquals(this, other)) {
                return true;
            }

            if (other == null) {
                return false;
            }

            return Id == other.Id;
        }

        public void EnsureRules() {
            if (Rules == null) {
                Rules = new List<PaintRule>();
            }

            if (Rules.Count == 0) {
                Rules.Add(PaintRule.CreateDefault(ModText.BC_UI_DefaultRuleName.GetString(1)));
            }

            foreach (var rule in Rules) {
                if (rule.ConditionGroup == null) {
                    rule.ConditionGroup = PaintRuleConditionGroup.CreateDefault();
                }

                if (rule.ConditionGroup.Conditions == null) {
                    rule.ConditionGroup.Conditions = new List<PaintRuleCondition>();
                }

                if (rule.ConditionGroup.Children == null) {
                    rule.ConditionGroup.Children = new List<PaintRuleConditionGroup>();
                }
            }
        }
    }
}
