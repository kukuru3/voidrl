using System.Collections.Generic;
using UnityEngine;

namespace Scanner.Plugship {
    internal class Shipbuilder : MonoBehaviour, IShipBuilder {
        internal Ship PrimaryShip { get; set; }

        void Awake() {
            var shipGO = new GameObject("Ship");
            shipGO.transform.parent = transform;
            shipGO.transform.localPosition = default;
            shipGO.transform.localRotation = default;
            PrimaryShip = shipGO.AddComponent<Ship>();
        }

        public void InsertModuleWithoutPlugs(Module instance) {
            PrimaryShip.AttachRootModule(instance);
            instance.transform.parent = PrimaryShip.transform;
            instance.transform.localPosition = default;
            instance.transform.localRotation = default;
        }

        public void Connect(IPlug a, IPlug b) {        

        } 
    }

    public interface IShipBuilder {
        void Connect(IPlug a, IPlug b);
        void InsertModuleWithoutPlugs(Module instance);
    }
}
