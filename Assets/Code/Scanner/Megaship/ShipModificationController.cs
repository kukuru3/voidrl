using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanner.Megaship {

    // includes the UI
    internal class ShipModificationController : MonoBehaviour {
        [SerializeField] Module[] modulePrefabs;
        private IList<IModificationRuleInjector> ruleInjectors;

        private void Start() {
            GeneratePhantoms();
            CollectRules();
        }

        private Module[] phantomModules;

        private void CollectRules() {
            var types = K3.ReflectionUtility.GetTypeAttributesInProject<InjectModificationRuleAttribute>();
            ruleInjectors = types.Select(t => t.type).Select(t => (IModificationRuleInjector)Activator.CreateInstance(t)).ToList();
            Debug.Log($"Collected {ruleInjectors.Count} rule injectors");
        }

        private void GeneratePhantoms() { 
            this.phantomModules = modulePrefabs.Select(GeneratePhantomInstance).ToArray();
        }

        Module GeneratePhantomInstance(Module prefab) {
            var go = Instantiate(prefab.gameObject, transform);
            go.SetActive(false);
            return go.GetComponent<Module>();
        }
    }
}
