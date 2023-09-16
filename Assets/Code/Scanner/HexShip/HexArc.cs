using UnityEngine;

namespace Scanner.HexShip {
    [ExecuteAlways]
    internal class HexArc : MonoBehaviour {
        MeshFilter _meshFilter;
        HexTransform _hexTransform;

        [SerializeField] float h0;
        [SerializeField] float h1;
        [SerializeField] float margin;

        private void Awake() {
            _meshFilter = GetComponent<MeshFilter>();
            _hexTransform = GetComponent<HexTransform>();
        }
    }
}
