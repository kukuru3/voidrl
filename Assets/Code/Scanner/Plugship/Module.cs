using System.Collections.Generic;
using UnityEngine;

namespace Scanner.Plugship {
    public class Module : MonoBehaviour {

        [field:SerializeField] public string Name { get; private set; }

        internal List<IPlug> AllPlugs { get; private set; }

        internal IEnumerable<Module> ListDirectlyConnectedModules() {
            var set = new HashSet<Module>();
            foreach (var plug in AllPlugs) {
                if (plug.IsConnected) set.Add(plug.ConnectedToModule);
            }
            return set;
        }

        internal Ship Ship { get; set; }

        private void Start() {            
        }

        private void Awake() {
            AllPlugs = new List<IPlug>(GetComponentsInChildren<IPlug>(true));   
            foreach (var plug in AllPlugs) ((BasePlug)plug).Module = this;
        }
    }
}
