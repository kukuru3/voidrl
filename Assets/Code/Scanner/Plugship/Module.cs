using System.Collections.Generic;
using UnityEngine;

namespace Scanner.Plugship {
    public class Module : MonoBehaviour {

        [field:SerializeField] public string Name { get; private set; }

        internal List<IPlug> AllPlugs { get; private set; }

        internal IEnumerable<Module> ListDirectlyConnectedModules() {
            var set = new HashSet<Module>();
            foreach (var plug in AllPlugs) {
                set.Add(plug.ConnectedToModule);
            }
            return set;
        }

        internal Ship Ship { get; private set; }

        private void Start() {
            
            AllPlugs = new List<IPlug>(GetComponentsInChildren<IPlug>(true));
            Ship = GetComponentInParent<Ship>();
        }

        private void Awake() {
            
        }
    }
}
