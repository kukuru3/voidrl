using K3;
using Shapes;
using UnityEngine;

namespace Scanner.GridVisualiser {
    internal class FlowGridPipeView : MonoBehaviour {
        internal FlowPipe Pipe { get; set; }
        internal Line line;
        [SerializeField] ShapeRenderer tip;

        const float AdjustLerp = 0.2f;
        const float PullApart  = 0.04f;
        internal void Bind(FlowPipe edge) {
            Pipe = edge;
            line = GetComponent<Line>();
        }

        private void LateUpdate() {
            var a = Pipe.from.position;
            var b = Pipe.to.position;

            var dir = (b - a).normalized;
            var ortho = new Vector2(dir.y, -dir.x);


            var dist = Vector3.Distance(a,b);
            var lerpFactor = AdjustLerp / dist;
            var fixedA = Vector2.Lerp(a,b, lerpFactor); // + ortho * PullApart;
            var fixedB = Vector2.Lerp(a,b, 1f - lerpFactor); // + ortho * PullApart;

            line.Start = fixedA.Deflatten() * FlowNodeView.SCALE;
            line.End = fixedB.Deflatten() * FlowNodeView.SCALE;

            line.Thickness = Pipe.capacity.Map(0f, 1000f, 0.01f, 0.1f);

            if (Mathf.Abs(Pipe.currentFlow) > 1f) {
                line.Dashed = true;
                if (line.Dashed) { 
                    line.DashOffset += Pipe.currentFlow * Time.deltaTime * 0.01f;
                }
                if (Pipe.capacity > 0) { 
                    line.Color = Color.Lerp(Color.gray / 3, Color.red, Mathf.Abs(Pipe.currentFlow) / Pipe.capacity) * 3f;
                }
            } else {
                line.Color = Color.gray;
                line.Dashed = false;
            }

            tip.transform.localPosition = line.End;
            tip.gameObject.SetActive(false);
        }
    }
}
