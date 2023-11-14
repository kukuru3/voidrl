using System;
using System.Collections.Generic;
using System.Linq;

namespace Scanner.Megaship {
    internal static class ModuleUtilities {
        public static IEnumerable<Module> AllConnectedModules(Module m) {
            if (m.Ship == null) return Enumerable.Empty<Module>();

            HashSet<Module> allOthers = new();
            foreach (var contact in m.Ship.Linkages) {
                if (contact.ModuleParticipatesInContact(m)) {
                    allOthers.UnionWith(contact.OtherModulesInContact(m));
                }
            }
            return allOthers;
        }

        public static bool ModuleParticipatesInContact(this Linkage c, Module m) {
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

        internal static IEnumerable<IPlug> ListUnoccupiedPlugs(Ship s) => s.AllShipModules().SelectMany(ListUnoccupiedPlugs);

        internal static IEnumerable<IPlug> ListUnoccupiedPlugs(Module module) {
            return module
                .GetComponentsInChildren<IPlug>(true)
                .Where(p => p.ActiveContact == null);
        }
    }
}
