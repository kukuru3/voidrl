using Haxxor;
using UnityEditor;
using UnityEngine;

namespace Scanner.TubeShip.View {
    internal class TubeView : MonoBehaviour {
        [field:SerializeField][field:Range(3, 24)] public int ArcSegments { get; set; }
        [field:SerializeField][field:Range(1, 20)] public int SpineSegments { get; set; }
        [field:SerializeField][field:Range(1f, 10f)] public float Radius { get; set; }

        [field:SerializeField][field:Range(1f, 3f)] public float ZSquash { get; set; } = 1.5f;

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
            // var unrollCenterOffsetY = -h * unroll;

            h *= multiplier;
            angle /= multiplier;

            // angle *= (1f - unroll);



            var x = Mathf.Sin(angle) * h;
            var y = Mathf.Cos(angle) * h;

            var z = axisPos * d * 1.5f * ZSquash;
            var center = new Vector3(x, y - unrollHCompensation, z);
            var up = new Vector3(x,y, 0);

            return (center, up);
        }
    }


}
