using K3;
using Scanner.ScannerView;
using UnityEngine;

namespace Scanner.TubeShip.View {

    internal static class TubeUtility {
        public static (bool hasResult, float radial, float spinal, float distance) RaycastTube(Ray ray, TubeView tube) {

            var originInTubespace = tube.transform.InverseTransformPoint(ray.origin);
            var directionInTubespace = tube.transform.InverseTransformDirection(ray.direction);

            var dx = directionInTubespace.x;
            var dy = directionInTubespace.y;
            var dx2 = dx * dx;
            var dy2 = dy * dy;

            var x = originInTubespace.x;
            var y = originInTubespace.y;
            var x2 = x * x;
            var y2 = y * y;

            var r = tube.Radius;

            var a = dx2 + dy2;
            var b = 2 * x * dx + 2 * y * dy;
            var c = x2 + y2 - r * r;
            var result = Numbers.SmallestPositive(Numbers.QuadraticEquation(a, b, c));
            if (result.HasValue && result.Value > 0f) {
                // result is "t", or, how many times is DIRECTION traversed before intersecting (even if dir. is not a unit vector)
                // tales principle: t can be used back in the 3d equation!
                var intersectPoint3D = originInTubespace + directionInTubespace * result.Value;
                var z = intersectPoint3D.z;
                var angle = Mathf.Atan2(intersectPoint3D.x, intersectPoint3D.y) / Mathf.PI / 2;
                if (angle < 0f) angle += 1f;
                return (true, angle, z, result.Value);
            }

            return (false, 0f, 0f, 0f);
        }
    }
    internal class TubeView : MonoBehaviour {
        [field:SerializeField][field:Range(3, 60)] public int ArcSegments { get; set; }
        [field:SerializeField][field:Range(1, 20)] public int SpineSegments { get; set; }
        [field:SerializeField][field:Range(0.3f, 10f)] public float Radius { get; set; }

        [field:SerializeField][field:Range(0.4f, 3f)] public float ZSquash { get; set; } = 1.5f;

        public float Unroll => GetComponentInParent<TubeshipView>().Unroll;

        public TubePoint[,] GetAllTubePoints() {
            var result = new TubePoint[SpineSegments, ArcSegments];
            for (var s = 0; s < SpineSegments; s++) {
                for (var a = 0; a < ArcSegments; a++) { 
                    result[s, a] = GetTubePoint(s, a);
                }
            }
            return result;
        }

        public TubePoint GetTubePoint(int axis, int arc) {
            var alpha = Mathf.PI / ArcSegments;
            var b = Mathf.Sin(alpha) * 2 * Radius;
            var d = b / Mathf.Sqrt(3);
            var h = Mathf.Cos(alpha) * Radius;
            var angle = Mathf.PI * 2f * arc / ArcSegments;
            var x = Mathf.Sin(angle) * h;
            var y = Mathf.Cos(angle) * h;
            var z = axis * d * 1.5f * ZSquash;
            var center = new Vector3(x, y, z);
            var up = new Vector3(x,y, 0);

             return new TubePoint() {
                 arcPos = arc, 
                 axisPos = axis, 
                 position = center,
                 up = up,
             };
        }
        
        public (Vector3 pos, Vector3 up) GetUnrolledTubePoint(float axisPos, float arcPos, float unroll) {
            var alpha = Mathf.PI / ArcSegments;
            var b = Mathf.Sin(alpha) * 2 * Radius;
            var d = b / Mathf.Sqrt(3);
            var h = Mathf.Cos(alpha) * Radius;
            var angle = Mathf.PI * 2f * arcPos / ArcSegments;

            var multiplier = 1f / (1f - unroll);

            var sumOfSideLengthsOverCircumference = ArcSegments * Mathf.Sin(alpha) / Mathf.PI;

            if (Mathf.Approximately(unroll, 1f)) {
                var zz = axisPos * d * 1.5f * ZSquash;
                var cc = new Vector3(arcPos * b * sumOfSideLengthsOverCircumference , h, zz);
                var uu = Vector3.up;
                return (cc, uu);
            }

            var h2 = h * multiplier;
            var unrollHCompensation = h2 - h;

            h *= multiplier;
            angle /= multiplier;

            var x = Mathf.Sin(angle) * h;
            var y = Mathf.Cos(angle) * h;

            var z = axisPos * d * 1.5f * ZSquash;
            var center = new Vector3(x, y - unrollHCompensation, z);
            var up = new Vector3(x,y, 0).normalized;

            return (center, up);
        }
    }


}
