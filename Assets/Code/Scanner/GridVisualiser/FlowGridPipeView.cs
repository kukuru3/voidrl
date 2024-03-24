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
            var fixedA = Vector2.Lerp(a,b, lerpFactor) + ortho * PullApart;
            var fixedB = Vector2.Lerp(a,b, 1f - lerpFactor)+ ortho * PullApart;

            line.Start = fixedA.Deflatten() * FlowNodeView.SCALE;
            line.End = fixedB.Deflatten() * FlowNodeView.SCALE;

            line.Thickness = Pipe.totalCapacity / 20_000f; // 0.05;
            var ef = Pipe.EffectiveFlow;
            line.Color = Color.Lerp(Color.grey, Color.red, ef / Pipe.totalCapacity);
            // line.Color = Color.Lerp(Color.green, Color.red, Pipe.currentFlow / Pipe.totalCapacity);

            tip.transform.localPosition = line.End;

            line.Dashed = ef > float.Epsilon;
            if (line.Dashed) { 
                line.DashOffset += (ef / Pipe.totalCapacity) * Time.deltaTime * 5f;
            }
        }
    }
}
