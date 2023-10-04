using K3;
using Shapes;
using UnityEngine;

namespace Scanner.ModularShip {
    [ExecuteAlways]
    [RequireComponent(typeof(Tube))]
    internal class TubeSlotDisplay : ImmediateModeShapeDrawer {
        [SerializeField] float squareDimension;
        [SerializeField] float squareThickness;
        public override void DrawShapes(Camera cam) {
            var tube = GetComponent<Tube>();
            if (tube == null) return;
            var tp = tube.GetAllTubePoints();

            using (Draw.Command(cam, UnityEngine.Rendering.CameraEvent.AfterImageEffects)) {
                foreach (var a in tp) {
                    var item = tube.GetUnrolledTubePoint(a.axisPos, a.arcPos, tube.Unroll);
                    
                    var tile = tube.GetTile(a.arcPos, a.axisPos);
                    if (tile?.occupiedBy != null) {
                        continue;
                    }

                    var fwd = transform.forward;
                    var up = transform.TransformVector(item.up);
                    var pos = transform.TransformPoint(item.pos);
                    var rot = Quaternion.LookRotation(fwd, up);
                    rot *= Quaternion.Euler(90, 0,0);
                    var dot = Vector3.Dot(cam.transform.forward, (transform.rotation * rot) * Vector3.forward);

                    rot = transform.rotation * rot;

                    var color = Color.white;
                    color.a = dot.Map(-0.4f, 0.2f, 0.1f, 1f);
                    Draw.RectangleBorder(
                        pos: pos,
                        rot: rot,
                        thickness: squareThickness,
                        rect: new Rect(-squareDimension/2, -squareDimension/2, squareDimension, squareDimension),
                        color: color
                    );
                }
            }
        }
    }
}