using System.Collections.Generic;
using UnityEngine;

namespace Scanner.Megaship {
    class Module : MonoBehaviour {

        internal Megaship Ship { get; private set; }
        List<Plug> plugs = new();  

        internal void AssignToShip(Megaship ship) {
            this.Ship = ship;
        }

        internal void RegisterPlug(Plug plug) {
            plugs.Add(plug);
        }

        internal IReadOnlyList<Plug> AllPlugs() => plugs;
    }
}
