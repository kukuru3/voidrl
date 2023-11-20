using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanner.Megaship {

    internal class Ship : MonoBehaviour {

        List<Module> rootModules = new();
        List<Linkage> linkages = new();
        internal IEnumerable<Linkage> Linkages => linkages;

        List<Module> resolvedModuleList = null;

        public IEnumerable<Module> AllShipModules() {
            if (resolvedModuleList == null) ResolveModuleList();
            return resolvedModuleList;
        }

        public void AddRootModule(Module m) {
            if (!rootModules.Contains(m)) rootModules.Add(m);
            InvalidateModuleList();
        }

        public void RemoveRootModule(Module m) => rootModules.Remove(m);

        public bool IsRootModule(Module m) => rootModules.Contains(m);

        public void AddLinkage(Linkage c) {
            if (!linkages.Contains(c)) linkages.Add(c);
            InvalidateModuleList();
        }

        public void RemoveContact(Linkage c) {
            if (linkages.Remove(c)) 
                InvalidateModuleList();
        }

        private void InvalidateModuleList() => resolvedModuleList = null;

        private void ResolveModuleList() {
            // assumption: all modules are either root, or present via one of the linkages.
            // assumption: there are no "invalid" linkages, linking a pair of modules of which 
            // neither has a path to a root module.
            resolvedModuleList = new List<Module>();
            var set = new HashSet<Module>();
            set.UnionWith(rootModules);
            foreach (var item in linkages) {
                set.UnionWith(item.pairings.Select(p => p.a.Module));
                set.UnionWith(item.pairings.Select(p => p.b.Module));
            }
            resolvedModuleList.AddRange(set);
        }
    }
}
