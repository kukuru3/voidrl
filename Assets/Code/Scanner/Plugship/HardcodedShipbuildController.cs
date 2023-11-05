using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanner.Plugship {

    internal class HardcodedShipbuildController : MonoBehaviour {
        [SerializeField] Module[] modulePrefabs;
        internal IShipBuilder Builder { get; private set; }
        List<Module> phantomModuleInstances = new List<Module>();

        Transform phantomHolder;

        Module GeneratePhantom(Module prefab) {
            var m = Instantiate(prefab, transform);
            m.name = $"PHANTOM: [{prefab.name}]";
            m.transform.localPosition = new Vector3(1000, 0, 0);
            foreach (var mc in m.gameObject.GetComponentsInChildren<MeshRenderer>()) mc.enabled = false;
            return m;
        }

        int moduleID = 0;

        Module GenerateModule(string name) {
            foreach (var p in modulePrefabs) if (p?.name == name || p.Name == name) { 
                var m = Instantiate(p, transform);
                m.gameObject.name = $"MODULE: {m.Name} [{++moduleID}]";
                return m;
            }
            throw new KeyNotFoundException($"Module by name of `{name}` not known");
        }

        private void Start() {
            Builder = GetComponentInChildren<IShipBuilder>();
            phantomModuleInstances = modulePrefabs.Select(GeneratePhantom).ToList();
            phantomHolder = new GameObject("Phantoms").transform;
            phantomHolder.transform.parent = transform;
            foreach (var i in phantomModuleInstances) i.transform.parent = phantomHolder;

            Builder.InsertModuleWithoutPlugs(GenerateModule("SpineSegment"));
        }
    }
}
