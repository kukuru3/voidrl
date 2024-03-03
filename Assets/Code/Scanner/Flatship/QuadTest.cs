using System.Linq;
using UnityEngine;

namespace Fudbalica {

    class QuadTest : MonoBehaviour {

        [SerializeField] Transform[] quadControlPoints; // 4 points that define the quadrangle. COUNTERCLOCKWISE, PLEASE:
        // 3---2
        // |   |
        // 0---1

        [SerializeField] Transform[] players;

        [SerializeField][Range(10, 100)] int solverPrecision;

        Vector2[] parametrizedPlayers;

        private void Start() {
            var quad = quadControlPoints.Select(FlatPos).ToArray();
            parametrizedPlayers =  players.Select(p => Quadrangle.NumericalAnalyzePointInQuad(quad, FlatPos(p), solverPrecision)).ToArray();
            for (var i = 0; i < players.Length; i++) {
                players[i].position = Quadrangle.PointInQuad(quad, parametrizedPlayers[i]).Deflatten();
            }
        }

        Vector2 FlatPos(Transform t) => new(t.position.x, t.position.z);

        private void Update() {
            var quad = quadControlPoints.Select(FlatPos).ToArray();

            for (var i = 0; i < players.Length; i++) {
                players[i].position = Quadrangle.PointInQuad(quad, parametrizedPlayers[i]).Deflatten();
            }
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.red;
            for (var i = 0; i < quadControlPoints.Length; i++) {
                var j = (i + 1) % quadControlPoints.Length;
                Gizmos.DrawLine(quadControlPoints[i].position, quadControlPoints[j].position);
            }
        }
    }

    static class VectorUtils {
        public static Vector2 Flatten(this Vector3 vector) => new Vector2(vector.x, vector.z);
        public static Vector3 Deflatten(this Vector2 vector) => new Vector3(vector.x, 0, vector.y);
    }

    class Quadrangle {
        static public Vector2 NumericalAnalyzePointInQuad(Vector2[] quad, Vector2 P, int precision = 100) {
            var t = 0.5f;
            var s = 0.25f;
            var (a,b,c,d) = (quad[0], quad[1], quad[2], quad[3]);

            for (var i = 0; i < precision; i++) { 
                var m = Vector2.Lerp(a,b,t);
                var n = Vector2.Lerp(d,c,t);

                var isRight = IsRight(m, n, P);
                if (isRight) t += s; else t -= s;
                s /= 2;
            }

            var mm = Vector2.Lerp(a, b, t);
            var nn = Vector2.Lerp(d, c, t);
            var proj = ProjectPointOnSegment(P, mm, nn);
            return new Vector2(t, proj);
        }

        static float ProjectPointOnSegment(Vector2 point, Vector2 segmentA, Vector2 segmentB) {
            float l2 = (segmentA - segmentB).sqrMagnitude;
            if (l2 < 0.000001f) return 0f;
            float t = Vector2.Dot(point - segmentA, segmentB - segmentA) / l2;
            return t;
        }

        static float Cross(Vector2 a, Vector2 b) {
            return a.x * b.y - a.y * b.x;
        }
        /// <summary>Is point p "to the right" of a->b</summary>
        static bool IsRight(Vector2 a, Vector2 b, Vector2 p) {
            return Cross(p - a, b - a) > 0;
        }
        
        public static Vector2 PointInQuad(Vector2[] quad, Vector2 parametrizedUV) {
            var (a, b, c, d) = (quad[0], quad[1], quad[2], quad[3]);
            var pAB = a + parametrizedUV.x * (b - a);
            var pCD = d + parametrizedUV.x * (c - d);
            return pAB + parametrizedUV.y * (pCD - pAB);
        }
    }
}