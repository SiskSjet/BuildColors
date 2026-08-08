using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Sisk.BuildColors.Settings.Models.PaintJobs {

    [ProtoContract]
    public class PaintJob : IEquatable<PaintJob> {

        public PaintJob() {
            Rules = new List<PaintRule>();
            Options = new PaintJobOptions();

            // Deliberately no EnsureRules() here. XmlSerializer appends to the list returned by the
            // Rules getter instead of replacing it, so anything seeded in the constructor would be kept
            // and the saved rules appended after it, growing the job by one rule on every load.
            // Callers that need a usable job call EnsureRules() explicitly.
        }

        [ProtoMember(1)]
        [XmlAttribute("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [ProtoMember(2)]
        [XmlAttribute("name")]
        public string Name { get; set; } = "New Paint Job";

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
        ///     Creates an independent copy, keeping the identity of the original so that saving the copy
        ///     replaces the job it was made from. Used to give dialogs a working copy that can be discarded.
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
                Rules.Add(PaintRule.CreateDefault("Rule 1"));
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
