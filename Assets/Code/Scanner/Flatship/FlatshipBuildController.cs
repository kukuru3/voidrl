using UnityEngine;

namespace Scanner.Flatship {
    public class FlatshipBuildController : MonoBehaviour {
        Ship ship;
        private void Start() {
            var ship = new ShipHardcoder().CreateHardcodedShip();
        }
    }
}