using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using K3;
using UnityEngine;

namespace Scanner.Megaship {
    internal static class MatchingUtility {
        internal static IEnumerable<Linkage> FindPossibleMatches(IEnumerable<IPlug> shipsideFreePlugs, IEnumerable<IPlug> modulesideFreePlugs) {
            var groupsModuleside = DistributeIntoGroups(modulesideFreePlugs);


            // debug aid, disable in prod for performance:
            shipsideFreePlugs = shipsideFreePlugs.ToArray();
            modulesideFreePlugs = modulesideFreePlugs.ToArray();
            groupsModuleside = groupsModuleside.ToArray();


            foreach (var groupMS in groupsModuleside) {
                var matches = MatchGroupWithPool(groupMS.plugs, shipsideFreePlugs.Where(p => p.GroupID <= 0));
                if (matches != null) foreach (var match in matches) yield return match;
            }

            // if this code runs, we allow for the possibility of a shipside group to match individual sockets from the pool
            // but it just seems to generate duplicates, especially in the most frequent use case of single items
            //var groupsShipSide = DistributeIntoGroups(shipsideFreePlugs);
            //foreach (var groupSS in groupsShipSide) {
            //    var matches = MatchGroupWithPool(groupSS.plugs, modulesideFreePlugs.Where(p => p.GroupID <= 0));
            //    if (matches != null) foreach (var match in matches) yield return match;
            //}
        }

        static IEnumerable<Linkage> MatchGroupWithPool(IList<IPlug> group, IEnumerable<IPlug> pool) {
            // for each plug in pool:
            // "slot" first plug in group into it (find pose of parent)
            var firstPlugInGroup = group[0];
            var prunedPool = pool.Where(item => IsCompatible(firstPlugInGroup, item));
            var isGroup = group.Count > 1;
            var matchedCandidatesInOrder = new List<IPlug>();

            foreach (var poolCandidate in prunedPool) {
                var pct = (poolCandidate as Component).transform;

                // so, we want to know how to orient the PARENT module of the phantom plug
                // in such a way that the plug coincides with the exact pose of the ship plug

                var worldPoseDestinationPlug = new Pose(pct.transform.position, pct.transform.rotation);
                var localPoseShipPlug = firstPlugInGroup.RelativePose;

                var worldPoseOfModuleSoThatPlugsCoincide = worldPoseDestinationPlug.Mul(localPoseShipPlug.Inverse());
                
                bool allMatch = true;
                matchedCandidatesInOrder.Clear();
                matchedCandidatesInOrder.Add(poolCandidate); // this gets a bit confusing

                if (isGroup) {                    
                    // but is accurate nonetheless:
                    // first we add the pool candidate, then rely on spatial matches to find other pool candidates.

                    //Debug.Log($"  Group match START, first pairing = {firstPlugInGroup.Name} => {poolCandidate.Name}"
                    //    + $"  For this pairing to work, the parent module {firstPlugInGroup.Module.Name} would need to be at worldpos {worldPoseOfModuleSoThatPlugsCoincide.Pretty()}" 
                    //);

                    for (var i = 1; i < group.Count; i++) {
                        var wposeOfPlug = worldPoseOfModuleSoThatPlugsCoincide.Mul(group[i].RelativePose);
                        // Debug.Log($"    Testing {group[i].Name} at tentative world pose {wposeOfPlug.Pretty()} against other potential spatial matches...");

                        bool anyMatch = false;
                        foreach (var poolItem in prunedPool) {
                            var worldPoseOfSocket = (poolItem as Component).transform.ToPose();
                            // var worldPoseOfSocket = poolItem.Module.transform.ToPose().Mul(poolItem.RelativePose);
                            if (PoseUtility.Identical(worldPoseOfSocket, wposeOfPlug)) {
                                // Debug.Log($"      Testing socket {poolItem.Name} against {group[i].Name}... MATCH");
                                matchedCandidatesInOrder.Add(poolItem);
                                anyMatch = true;
                                break;
                            } else {
                                // Debug.Log($"      Testing socket {poolItem.Name} against {group[i].Name}... NO MATCH");
                            }
                        }
                        if (!anyMatch) { allMatch = false; break; }
                    }
                }
                if (allMatch) {
                    yield return Linkage.FromCollections(group, matchedCandidatesInOrder);
                }
            }
        }

        private static bool IsCompatible(IPlug a, IPlug b) {
            if (a.Tag != b.Tag) return false;
            var polaritiesCompatible = (a.Polarity, b.Polarity) switch {
                (Polarities.Male, Polarities.Female) => true,
                (Polarities.Female, Polarities.Male) => true,
                (Polarities.TwoWay, Polarities.TwoWay) => true,
                _ => false,
            };
            if (!polaritiesCompatible) return false;
            return true;
        }

        static internal string Pretty(this Pose pose) {
            return $"{pose.position:f2} ; {pose.rotation.eulerAngles:F0}";
        }

        internal static IEnumerable<PlugGroup> DistributeIntoGroups(IEnumerable<IPlug> basePlugs) {
            Dictionary<int, PlugGroup> groups = new();
            foreach (var plug in basePlugs) {
                if (plug.GroupID == 0) {
                    yield return new PlugGroup() {plugs = new List<IPlug>() { plug } };
                } else {
                    if (!groups.ContainsKey(plug.GroupID)) {
                        groups[plug.GroupID] = new PlugGroup() { plugs = new List<IPlug>() { plug } };
                    } else {
                        groups[plug.GroupID].plugs.Add(plug);
                    }
                }
            }

            foreach (var key in groups.Keys.OrderBy(k => k)) yield return groups[key];            
        }

        internal static bool AllPlugsContainTag(Linkage link, string tag) {
            return link.AllPlugs.All(p => p.Tag == tag);
        }


        internal static IEnumerable<Module> ShipboardModulesOf(Linkage linkage) {
            var set = new HashSet<Module>();
            foreach (var plug in linkage.AllPlugs) {
                if (!plug.Module.IsPhantom) set.Add(plug.Module);
            }
            return set;
        }

        internal static Module ShipboardModuleOf(Linkage linkage) {
            foreach (var plug in linkage.AllPlugs) {
                if (!plug.Module.IsPhantom) return plug.Module;
            }
            return default;
        }
    }
}
