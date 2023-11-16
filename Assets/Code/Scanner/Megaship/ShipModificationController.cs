using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanner.Megaship {

    // includes the UI
    internal class ShipModificationController : MonoBehaviour {
        
        [SerializeField] Module[] modulePrefabs;
        [SerializeField] Transform shipRoot;
        [SerializeField] Transform phantomsRoot;

        private IList<IModificationRuleInjector> ruleInjectors;

        private void Start() {
            // GeneratePhantoms();
            CollectPhantomsFromTemplate();
            CollectRules();
            GenerateInitialShip();
            RegenerateModifications();
        }

        private void CollectPhantomsFromTemplate() {
            phantomModules = phantomsRoot.GetComponentsInChildren<Module>(true);
            foreach (var pm in phantomModules) {
                pm.gameObject.SetActive(false);
                pm.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }
        
        private void GeneratePhantoms() { 
            phantomModules = modulePrefabs.Select(GeneratePhantomInstance).ToArray();
        }

        private Module[] phantomModules;

        private void CollectRules() {
            var types = K3.ReflectionUtility.GetTypeAttributesInProject<InjectModificationRuleAttribute>();
            ruleInjectors = types.Select(t => t.type).Select(t => (IModificationRuleInjector)Activator.CreateInstance(t)).ToList();
            Debug.Log($"Collected {ruleInjectors.Count} rule injectors");
        }


        Module GeneratePhantomInstance(Module prefab) {
            var go = Instantiate(prefab.gameObject, transform);
            go.SetActive(false);
            return go.GetComponent<Module>();
        }

        public Ship Ship { get; set; }

        void GenerateInitialShip() {
            Ship = shipRoot.GetComponent<Ship>();
            var spine = GenerateModule("spine");
            Ship.AddRootModule(spine);
            spine.Ship = Ship;
        }

        public Module GenerateModule(string name) {
            var sourceModule = phantomModules.First(m => m.Name.ToUpperInvariant() == name.ToUpperInvariant());
            var copy = Instantiate(sourceModule.gameObject, shipRoot).GetComponent<Module>();
            copy.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            copy.gameObject.SetActive(true);
            return copy;
        }

        public void RegenerateModifications() {
            var lqc = new LinkQueryContext() {
                concreteSymmetry = 1,
                explicitModule = null,
                phantomModules = this.phantomModules.ToList(),
            };

            List<ModificationOpportunity> opportunities = new();

            foreach (var injector in ruleInjectors) {
                var ops = injector.Inject(Ship, lqc);
                foreach (var op in ops) {
                    Debug.Log($"Injector {injector.GetType().Name} injected opportunity: {op.Print()}");
                    opportunities.Add(op);
                }
            }
        }
    }
}
