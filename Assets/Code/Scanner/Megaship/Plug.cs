using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanner.Megaship {
    // a Point Linkable
    internal class Plug : MonoBehaviour, IPlug {
        [field:SerializeField] public Polarities Polarity { get; private set; }
        [field:SerializeField] public string Tag { get; private set; }
        public Module Module { get; set; }

        [field:SerializeField] [field:Range(0, 6)] public int GroupID { get; private set; }

        Linkage IPlug.ActiveContact { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    class InjectModificationRuleAttribute : Attribute {

    }

    
    internal interface IModificationRuleInjector {
        IEnumerable<ModificationOpportunity> Inject(Ship ship, LinkQueryContext context);
    }

    [InjectModificationRule]
    public class GenerateMatches : IModificationRuleInjector {

        IEnumerable<ModificationOpportunity> IModificationRuleInjector.Inject(Ship ship, LinkQueryContext context) {
            // list all dangling link groups with the flavour "spine"

            foreach (var pmodule in context.phantomModules) {
                var matches = MatchingUtility.FindPossibleMatches(ModuleUtilities.ListUnoccupiedPlugs(ship), ModuleUtilities.ListUnoccupiedPlugs(pmodule));
                foreach (var match in matches) {
                    if (MatchingUtility.AllPlugsContainTag(match, "spine-ext")) {
                        yield return new BuildAndAttachOpportunity() {
                            orientation = 0,
                            symmetry = 0,
                            targetContact = match, 
                            type = OpportunityTypes.OfferTweak,
                        };
                    }
                }
            }

            yield break;
        }
    }


    class PlugGroup {
        public int index;
        public List<IPlug> plugs = new();
    }

    internal static class MatchingUtility {
        internal static IEnumerable<Linkage> FindPossibleMatches(IEnumerable<IPlug> shipsideFreePlugs, IEnumerable<IPlug> modulesideFreePlugs) {
            var groupsShipside = DistributeIntoGroups(shipsideFreePlugs);
            var groupsModuleside = DistributeIntoGroups(modulesideFreePlugs);
            foreach (var groupA in groupsShipside) {
                foreach (var groupB in groupsModuleside) {
                    if (MatchGroup(groupA, groupB)) {
                        yield return Linkage.FromGroups(groupA, groupB);
                    }
                }
            }
        }

        static bool MatchGroup(PlugGroup a, PlugGroup b) {
            if (a.plugs.Count != b.plugs.Count) return false;
            var firstPlugA = a.plugs[0];
            for (var indexB = 0; indexB < b.plugs.Count; indexB++) {
                var plugB = b.plugs[indexB];
                // todo: position the plug group parent in such a way 
                // that firstPlugA matches with plugB
                // then check if all other plugs match.
            }
            return true;
        }

        internal static IEnumerable<PlugGroup> DistributeIntoGroups(IEnumerable<IPlug> basePlugs) {
            Dictionary<int, PlugGroup> groups = new();
            foreach (var plug in basePlugs) {
                if (plug.GroupID == 0) {
                    yield return new PlugGroup() { index = -1, plugs = new List<IPlug>() { plug } };
                } else {
                    if (!groups.ContainsKey(plug.GroupID)) {
                        groups[plug.GroupID] = new PlugGroup() { index = plug.GroupID, plugs = new List<IPlug>() { plug } };
                    }
                }
            }

            foreach (var key in groups.Keys.OrderBy(k => k)) yield return groups[key];            
        }

        internal static bool AllPlugsContainTag(Linkage link, string tag) {
            return link.AllPlugs.All(p => p.Tag == tag);
        }
    }

    internal class LinkQueryContext {
        public int concreteSymmetry = 1;
        public List<Module> phantomModules;
    }

    public enum OpportunityTypes {
        Hidden,
        OfferTweak,
        ConstructFromMenu,
    }

    public abstract class ModificationOpportunity {

    }

    public class BuildAndAttachOpportunity : ModificationOpportunity {
        internal OpportunityTypes type;
        internal Linkage targetContact;
        internal int symmetry;
        internal int orientation; // bonus, can be anything.
    }

    public class DestroyModuleOpportunity : ModificationOpportunity {
        internal Module targetModule;
    }
}
