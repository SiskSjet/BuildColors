using Sisk.BuildColors.Settings.Models.PaintJobs;
using System;
using System.Collections.Generic;

namespace Sisk.BuildColors.Services {

    /// <summary>
    ///     One addressable node of a condition tree, either a group or a condition.
    /// </summary>
    internal class PaintRuleNode {

        /// <summary>
        ///     Group holding this node, null for the root group.
        /// </summary>
        public PaintRuleConditionGroup Parent { get; set; }

        public PaintRuleConditionGroup Group { get; set; }

        public PaintRuleCondition Condition { get; set; }

        /// <summary>
        ///     Index inside the list of the parent this node lives in, either its conditions or its children.
        /// </summary>
        public int Index { get; set; }

        public string Path { get; set; }

        public int Depth { get; set; }

        public bool IsGroup {
            get { return Group != null; }
        }
    }

    /// <summary>
    ///     Addresses nodes of a condition tree by path so the console can reach every part of a tree the UI
    ///     lets the player click on. Members of a group are numbered from 1, conditions first and nested
    ///     groups after them, which is the order the tree is shown in. The root group is <c>0</c>, so
    ///     <c>3.1</c> is the first member of the third member of the root.
    /// </summary>
    internal static class PaintRulePath {
        public const string ROOT = "0";

        /// <summary>
        ///     Flattens the tree into the order it is listed in, root first.
        /// </summary>
        public static List<PaintRuleNode> Flatten(PaintRuleConditionGroup root) {
            var nodes = new List<PaintRuleNode>();

            if (root == null) {
                return nodes;
            }

            nodes.Add(new PaintRuleNode { Group = root, Parent = null, Index = 0, Path = ROOT, Depth = 0 });
            Walk(root, string.Empty, 1, nodes);

            return nodes;
        }

        /// <summary>
        ///     Resolves a path against a tree, returning null when it addresses nothing.
        /// </summary>
        public static PaintRuleNode Resolve(PaintRuleConditionGroup root, string path) {
            if (root == null) {
                return null;
            }

            if (IsRoot(path)) {
                return new PaintRuleNode { Group = root, Parent = null, Index = 0, Path = ROOT, Depth = 0 };
            }

            // The root is printed as 0 and its members without a prefix, but writing them as 0.1 is the
            // obvious reading of that listing, so both forms are accepted.
            var text = path.Trim();
            if (text.StartsWith("0.", StringComparison.Ordinal)) {
                text = text.Substring(2);
            }

            var segments = text.Split('.');
            var current = root;
            PaintRuleNode node = null;
            var prefix = string.Empty;

            for (var i = 0; i < segments.Length; i++) {
                // A condition is a leaf, so anything addressed below one cannot exist.
                if (node != null && !node.IsGroup) {
                    return null;
                }

                int position;
                if (!CommandArguments.TryParseInteger(segments[i], out position) || position < 1) {
                    return null;
                }

                var conditionCount = current.Conditions != null ? current.Conditions.Count : 0;
                var childCount = current.Children != null ? current.Children.Count : 0;

                if (position > conditionCount + childCount) {
                    return null;
                }

                prefix = Combine(prefix, position);

                if (position <= conditionCount) {
                    node = new PaintRuleNode {
                        Condition = current.Conditions[position - 1],
                        Parent = current,
                        Index = position - 1,
                        Path = prefix,
                        Depth = i + 1
                    };

                    continue;
                }

                var childIndex = position - conditionCount - 1;
                var child = current.Children[childIndex];

                node = new PaintRuleNode {
                    Group = child,
                    Parent = current,
                    Index = childIndex,
                    Path = prefix,
                    Depth = i + 1
                };

                current = child;
            }

            return node;
        }

        public static bool IsRoot(string path) {
            if (string.IsNullOrWhiteSpace(path)) {
                return true;
            }

            var trimmed = path.Trim();

            return trimmed == ROOT || string.Equals(trimmed, "root", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        ///     True when <paramref name="candidate" /> is <paramref name="group" /> itself or sits below it.
        ///     Moving a group into its own subtree would detach it from the rule, so it has to be refused.
        /// </summary>
        public static bool Contains(PaintRuleConditionGroup group, PaintRuleConditionGroup candidate) {
            if (group == null || candidate == null) {
                return false;
            }

            if (ReferenceEquals(group, candidate)) {
                return true;
            }

            if (group.Children == null) {
                return false;
            }

            foreach (var child in group.Children) {
                if (Contains(child, candidate)) {
                    return true;
                }
            }

            return false;
        }

        private static void Walk(PaintRuleConditionGroup group, string prefix, int depth, List<PaintRuleNode> nodes) {
            var conditionCount = group.Conditions != null ? group.Conditions.Count : 0;

            for (var i = 0; i < conditionCount; i++) {
                nodes.Add(new PaintRuleNode {
                    Condition = group.Conditions[i],
                    Parent = group,
                    Index = i,
                    Path = Combine(prefix, i + 1),
                    Depth = depth
                });
            }

            if (group.Children == null) {
                return;
            }

            for (var i = 0; i < group.Children.Count; i++) {
                var path = Combine(prefix, conditionCount + i + 1);

                nodes.Add(new PaintRuleNode {
                    Group = group.Children[i],
                    Parent = group,
                    Index = i,
                    Path = path,
                    Depth = depth
                });

                Walk(group.Children[i], path, depth + 1, nodes);
            }
        }

        private static string Combine(string prefix, int position) {
            return string.IsNullOrEmpty(prefix) ? position.ToString() : string.Format("{0}.{1}", prefix, position);
        }
    }
}
