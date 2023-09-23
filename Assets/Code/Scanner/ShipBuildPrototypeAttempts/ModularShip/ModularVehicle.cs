using System.Collections.Generic;
using K3.Collections;
using UnityEngine;

namespace Scanner.ModularShip {
    public class ModularVehicle : MonoBehaviour {
        public SimpleTree<ShipModule> currentModules = new();

        void Initialize(ShipModule rootModule) {
            if (currentModules != null) currentModules = new SimpleTree<ShipModule>();
            currentModules.CreateRoot(rootModule);
        }

        public IEnumerable<ShipModule> ListModules() => currentModules.FlatListOfItems;
    }


}