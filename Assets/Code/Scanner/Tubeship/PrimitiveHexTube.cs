using Shapes;
using UnityEngine;

namespace Scanner.TubeShip.View {

    // first implementation
    [ExecuteAlways]
    internal class PrimitiveHexTube : ImmediateModeShapeDrawer {

        public float Radius => radius;
        public int Polygon => numItems;
        public int Depth => depth;

        [SerializeField][Range(1f, 10f)]float radius;
        [SerializeField][Range(3, 36)] int numItems;
        [SerializeField][Range(1, 20)] int depth;
        [SerializeField] float drawMargin;        
        [SerializeField][Range(0f, 3f)] float thiccness;

        [SerializeField] bool nonAlternatingMode;
        [SerializeField] bool outer;

        [SerializeField]
        [Range(-1f, 1f)] float dotMargin;

        enum DrawTypes {
            HexFlatAxiswise,
            HexFlatCirclewise,
            Circle,
        }
        [SerializeField] DrawTypes type;

        public override void DrawShapes(Camera cam) {
            var alpha = Mathf.PI / numItems;
            var bHalf = Mathf.Sin(alpha) * radius;
            var d = 2f * bHalf / Mathf.Sqrt(3);
            var h = Mathf.Cos(alpha) * radius;

            using (Draw.Command(cam, UnityEngine.Rendering.CameraEvent.AfterImageEffects)) {  
                // Draw.Ring(radius: radius, thickness: thiccness, pos: transform.TransformPoint(Vector3.zero), rot: transform.rotation * Quaternion.Euler(0,0,0));
                // d is hexagon radius

                for (var z = 0; z < depth; z++ ) {
                    for (var p = 0; p < numItems; p++) { 

                        var oddZ = z % 2 == 1;

                        var angle = Mathf.PI * 2 * p / numItems;
                        if (oddZ && !nonAlternatingMode) angle += Mathf.PI * 1 / numItems;

                        var x = Mathf.Sin(angle) * h;
                        var y = Mathf.Cos(angle) * h;
                        var direction = new Vector3(x,y,0);
                        var center = direction + Vector3.forward * (z * d * 1.5f); 
                        var rot = Quaternion.LookRotation(direction, Vector3.forward);
                        if (type == DrawTypes.HexFlatCirclewise) { 
                            rot *= Quaternion.Euler(0,0,30);
                        }

                        var dot = Vector3.Dot(cam.transform.forward, (transform.rotation * rot) * Vector3.forward) + (outer ? dotMargin : -dotMargin);
                        if (outer) dot *= -1;
                        var color = Color.white;
                        color.a = Mathf.Clamp01(dot);
                        if (dot < 0) { color = Color.red; color.a = Mathf.Clamp01(-dot) / 4f; }
                        
                        if (type == DrawTypes.Circle) {
                            Draw.Ring(
                                radius: d - drawMargin,
                                thickness: thiccness,
                                colors: color,
                                pos: transform.TransformPoint(center),
                                rot: transform.rotation * rot
                            );
                        } else {
                            Draw.RegularPolygonBorder(
                                sideCount: 6,
                                radius: d - drawMargin,
                                thickness: thiccness,
                                color: color,
                                pos: transform.TransformPoint(center),
                                rot: transform.rotation * rot
                            );
                        }
                    }
                }
            }
        }
    }
}