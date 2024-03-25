using K3;
using Shapes;
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
            var color = Color.black;
            if (Node.productionOrConsumption > float.Epsilon) color = Color.Lerp(color, Color.red, Mathf.Pow(Mathf.Clamp01(Node.productionOrConsumption / 2000), 0.1f));
            else if (Node.productionOrConsumption < float.Epsilon) color = Color.Lerp(color, Color.blue, Mathf.Pow(Mathf.Clamp01(Node.productionOrConsumption / -2000), 0.1f));
            GetComponent<ShapeRenderer>().Color = color;

            label.text = $"{Node}\r\n{Node.calcTemp:F0} [{Node.productionOrConsumption:F0}]";
        }
    }
}
