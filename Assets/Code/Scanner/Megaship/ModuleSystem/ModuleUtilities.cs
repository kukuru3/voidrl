using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Scanner.Megaship {
    internal static class ModuleUtilities {
        public static IEnumerable<Module> AllConnectedModules(Module m) {
            if (m.Ship == null) return Enumerable.Empty<Module>();

            HashSet<Module> allOthers = new();
            foreach (var contact in m.Ship.Linkages) {
                if (contact.Links(m)) {
                    allOthers.UnionWith(contact.OtherModulesInContact(m));
                }
            }
            return allOthers;
        }

        public static bool Links(this Linkage c, Module m) {
            foreach (var pairing in c.pairings) {
                if (pairing.a.Module == m) return true;
                if (pairing.b.Module == m) return true;
            }
            return false;
        }

        public static IEnumerable<Module> OtherModulesInContact(this Linkage c, Module m) {
            var others = new HashSet<Module>();
            foreach (var p in c.pairings) {
                if (p.a.Module == m) others.Add(p.b.Module);
                if (p.b.Module == m) others.Add(p.a.Module);
            }
            return others;
        }

        internal static IEnumerable<Linkage> AllShipLinkagesOf(Module m) {
            if (m.Ship == null) yield break;
            foreach (var l in m.Ship.Linkages) {
                if (l.Links(m)) yield return l;
            }
        }

        internal static IEnumerable<IPlug> ListUnoccupiedPlugs(Ship s) => s.AllShipModules().SelectMany(ListUnoccupiedPlugs)
            .ToArray()
        ;

        internal static IEnumerable<IPlug> ListUnoccupiedPlugs(Ship s, Polarities polarity) => 
            s.AllShipModules()
                .SelectMany(ListUnoccupiedPlugs)
                .Where(p => p.Polarity == polarity)
                .ToArray()
        ;

        internal static IEnumerable<IPlug> ListAllPlugs(Module module) {
               return module
                .GetComponentsInChildren<IPlug>(true);
        }

        internal static IEnumerable<IPlug> ListUnoccupiedPlugs(Module module) {
            return module
                .GetComponentsInChildren<IPlug>(true)
                .Where(p => p.ActiveContact == null);
        }

        internal static IPlug FindSurrogate(IPlug originalPlug, Module originalModule, Module replacementModule) {
            var plugsA = originalModule.GetComponentsInChildren<IPlug>();
            var plugsB = replacementModule.GetComponentsInChildren<IPlug>();
            Debug.Assert(originalModule.Name == replacementModule.Name);
            Debug.Assert(plugsA.Length == plugsB.Length, "Plugs mismatch!");
            var idx = Array.IndexOf(plugsA, originalPlug);
            Debug.Assert(idx >= 0, "Are you sure plug is there?");
            Debug.Assert(plugsB[idx].Name == plugsA[idx].Name, "plug name mismatch?");
            return plugsB[idx];
        }
    }
}
