using System.Linq;
using Shapes;
using UnityEngine;

namespace Scanner.Charting {
    [ExecuteAlways]
    internal class GranuleChart : Chart {
        
        [SerializeField] float w;

        [SerializeField] float squareDimension;
        [SerializeField] float squareSeparator;

        [SerializeField] float singleUnitAmount = 1f;

        [SerializeField] int rowsForDimensionScaling;
        [SerializeField] float dimensionScaleDim;
        [SerializeField] float dimensionScaleAmount;

        public override void DrawShapes(Camera cam) {
            if (singleUnitAmount < float.Epsilon) return;
            if (!cam.name.Contains("UI")) return;

            int X = 0;
            int Y = 0;
            int cubesPerRow = 100;
            var dimension = squareDimension;

            using (Draw.Command(cam, UnityEngine.Rendering.CameraEvent.AfterForwardAlpha)) {
                if (entries == null || entries.Count == 0) return;
                Draw.Matrix = transform.localToWorldMatrix;

                var nominalCubesPerRow = Mathf.FloorToInt(w / (dimension + squareSeparator));
                cubesPerRow = nominalCubesPerRow;
                var amountPerUnit = singleUnitAmount;

                var amountOfAllEntries = this.entries.Sum(e => e.amount);

                var totalNumCubes = amountOfAllEntries / singleUnitAmount + entries.Count;
                var totalNumRows = Mathf.CeilToInt(totalNumCubes / (float)nominalCubesPerRow);

                if (dimensionScaleAmount > 1f && rowsForDimensionScaling > 0 && totalNumRows > rowsForDimensionScaling) {
                    // scale!
                    dimension *= dimensionScaleDim;
                    cubesPerRow = Mathf.FloorToInt(w / (dimension + squareSeparator));
                    amountPerUnit *= dimensionScaleAmount;
                }

                foreach (var entry in entries) {
                    var numWholeCubes =  entry.amount / amountPerUnit;
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

            void AdvanceIndex() { X++; if (X >= cubesPerRow) { X = 0; Y++; } }

            void DrawCube(Color color, int X, int Y) {
                var x0 = X * (dimension + squareSeparator);
                var y0 = -Y * (dimension + squareSeparator);
                Draw.Rectangle(new Rect(x0, y0, dimension, dimension), color);
            }
        }

        // [SerializeField] float overflowHeight;
    }
}
