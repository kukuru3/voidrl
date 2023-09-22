using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanner.ModularShip {

    static public class AttachmentArbiter {

    }

    public class ShipModule : MonoBehaviour
    {
        [SerializeField] string moduleName;
        [SerializeField] float  weight;

        ModularVehicle vehicle;

        public void OnAttached(ModularVehicle vehicle) {
            this.vehicle = vehicle;
        }

        IEnumerable<Slot> GetSlots() {
            return GetComponentsInChildren<Slot>();
        }

        public IEnumerable<Slot> GetFreeSlots() => GetSlots().Where(s => s.ConnectedTo == null);
    }
}