using Shapes;
using UnityEngine;

namespace Scanner.Charting {
    [ExecuteAlways]
    internal class GranuleChart : Chart {
        
        [SerializeField] float w;

        [SerializeField] float squareDimension;
        [SerializeField] float squareSeparator;

        [SerializeField] float singleUnitAmount = 1f;

        public override void DrawShapes(Camera cam) {
            if (singleUnitAmount < float.Epsilon) return;
            if (!cam.name.Contains("UI")) return;

            int X = 0;
            int Y = 0;
            int cubesPerRow = 100;
            void AdvanceIndex() { X++; if (X >= cubesPerRow) { X = 0; Y++; } }

            void DrawCube(Color color, int X, int Y) {
                var x0 = X * (squareDimension + squareSeparator);
                var y0 = -Y * (squareDimension + squareSeparator);
                Draw.Rectangle(new Rect(x0, y0, squareDimension, squareDimension), color);
            }

            using (Draw.Command(cam, UnityEngine.Rendering.CameraEvent.AfterForwardOpaque)) {
                if (entries == null || entries.Count == 0) return;
                Draw.Matrix = transform.localToWorldMatrix;

                cubesPerRow = Mathf.FloorToInt(w / (squareDimension + squareSeparator));
                foreach (var entry in entries) {
                    var numWholeCubes =  entry.amount / singleUnitAmount;
                    var numCubesInt = Mathf.FloorToInt(numWholeCubes);
                    var extraCubeOpacity = numWholeCubes - numCubesInt;
                    for (var i = 0; i < numCubesInt; i++) {
                        DrawCube(entry.color, X, Y);
                        AdvanceIndex();
                    }
                    if (extraCubeOpacity > 0.1f) {
                        var color = entry.color;
                        color.a *= extraCubeOpacity * extraCubeOpacity;
                        DrawCube(color, X, Y);
                        AdvanceIndex();
                    }
                }
            }
        }

        // [SerializeField] float overflowHeight;
    }
}
