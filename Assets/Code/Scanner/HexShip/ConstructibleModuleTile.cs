using Core.h3x;
using UnityEngine;

namespace Scanner.HexShip {
    public class ConstructibleModuleTile : MonoBehaviour, IHasHex3Coords {
        public Hex3 Coords { get; set; }
    }
}

