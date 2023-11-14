using System.Collections.Generic;
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

        public void AddLinkage(Linkage c) {
            if (!linkages.Contains(c)) linkages.Add(c);
            InvalidateModuleList();
        }

        public void RemoveLinkage(Linkage c) {
            if (linkages.Remove(c))
                InvalidateModuleList();
        }

        private void InvalidateModuleList() => resolvedModuleList = null;

        private void ResolveModuleList() {
            var closedList = new HashSet<Module>(rootModules);
            var queue = new Queue<Module>(rootModules);

            var l = new List<Module>();
            while (queue.Count > 0) {
                var activeModule = queue.Dequeue();
                l.Add(activeModule);
                var connectedToThisModule = ModuleUtilities.AllConnectedModules(activeModule);
                foreach (var o in connectedToThisModule) {
                    if (closedList.Contains(o)) continue;
                    queue.Enqueue(o);
                    closedList.Add(o);
                }
            }
            this.resolvedModuleList = l;
        }
    }
}
