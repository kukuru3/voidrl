using K3;
using UnityEngine;

namespace Scanner.GridVisualiser {
    internal class FlowNodeView : MonoBehaviour {
        [SerializeField] TMPro.TMP_Text label;

        public const float SCALE = 5f;
        internal FlowNode Node { get; set; }
        internal void Bind(FlowNode node) {
            Node = node;
        }

        private void Update() {
            transform.position = Node.position.Deflatten() * SCALE;
        }
    }
}
