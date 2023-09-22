﻿using K3;
using Shapes;
using UnityEngine;

namespace Scanner.TubeShip.View {
    [ExecuteAlways]
    [RequireComponent(typeof(TubeView))]
    internal class TubeDebugDisplayer : ImmediateModeShapeDrawer {
        [SerializeField][Range(0f, 1f)] float unroll = 0f;
        public override void DrawShapes(Camera cam) {
            var tube = GetComponent<TubeView>();
            var tp = tube.GetAllTubePoints();

            using (Draw.Command(cam, UnityEngine.Rendering.CameraEvent.AfterImageEffects)) {
                foreach (var a in tp) {
                    var item = tube.GetUnrolledTubePoint(a.axisPos, a.arcPos, unroll);
                    
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
                        thickness: 0.1f,
                        rect: new Rect(-0.2f, -0.2f, 0.4f, 0.4f),
                        color: color
                    );
                }
            }
        }
    }
}