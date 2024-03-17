using Core.H3;
using K3.Hex;
using UnityEngine;

namespace Scanner.Atomship {
    class HexTest : MonoBehaviour {
        private void Start() {
            var h = new Hex(0, 1);
            var h2 = h.RotateAroundZero(1);
            Debug.Log($"Rot once : {h} => {h2}");
            Debug.Log(Hexes.HexToPixel(h, GridTypes.FlatTop, 1f));

            foreach (var dir in K3.Enums.IterateValues<HexDir>()) {
                var inv = dir.Inverse();
                Debug.Log($"{dir} => {inv}");
            }
        }
    }
}