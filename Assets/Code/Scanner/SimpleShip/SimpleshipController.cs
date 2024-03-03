using UnityEngine;

namespace Scanner.SimpleShip {
    class SimpleshipController : MonoBehaviour {
        private ColonyShip ship;

        private void Start() {
            Hardcoder.InitializeStructures();
            ship = Hardcoder.CreateHardcodedShip();
        }
    }
}
