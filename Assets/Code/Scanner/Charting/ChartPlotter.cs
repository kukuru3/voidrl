using System;
using System.Collections.Generic;
using K3;
using Shapes;
using UnityEngine;

namespace Scanner.Charting {

    [ExecuteAlways]
    internal class ChartPlotter : ImmediateModeShapeDrawer {
        private ChartData model;

        [SerializeField] float lineWidth;
        [SerializeField] float w;
        [SerializeField] float h;
        private AxisDynamicData xData;
        private AxisDynamicData yData;

        public override void DrawShapes(Camera cam) {
            using (Draw.Command(cam)) {

                Draw.LineGeometry = LineGeometry.Volumetric3D;
                Draw.ThicknessSpace = ThicknessSpace.Pixels;
                Draw.Thickness = lineWidth;
                Draw.Matrix = transform.localToWorldMatrix;

                Draw.Line(Vector3.zero, Vector3.right * w, Color.white);
                Draw.Line(Vector3.zero, Vector3.up * h, Color.white);

                Draw.Thickness = lineWidth / 2;

                if (model == null) return;

                var recursionGuard = 0;
                for (var i = yData.min; i <= yData.max; i += yData.stepActual) { 
                    recursionGuard++; if (recursionGuard >= 1000) break;
                    
                    var y = (float)Map(i, yData.min, yData.max, 0, (decimal)h, true);
                    var x1 = 0; var x2 = 10;
                    Draw.Line(new Vector3(x1, y, 0), new Vector3(x2, y, 0), Color.white);
                }

                //for (var i = 0; i < 10; i++) {
                //    var x1 = 0; var x2 = 10; var y = (h * i / 10);
                //    Draw.Line(new Vector3(x1, y, 0), new Vector3(x2, y, 0), Color.white);
                //}
            }
        }

        static decimal Map(decimal source, decimal sourceFrom, decimal sourceTo, decimal targetFrom, decimal targetTo, bool constrained = true) {
            var t = (source - sourceFrom) / (sourceTo - sourceFrom);
            if (constrained) t = Math.Clamp(t, 0, 1);
            return Math.Round(targetFrom + t * (targetTo - targetFrom));
        }

        internal void Refresh(ChartData model) {
            this.model = model;
            Calculate();
        }

        void Calculate() {
            var minX = 0m; var minY = 0m;
            var maxX = 0m; var maxY = 0m;

            foreach (var plot in model.plots) {
                foreach (var pt in plot.points) {
                    if (pt.x < minX) minX = pt.x;
                    if (pt.x > maxX) maxX = pt.x;
                    
                    if (pt.y < minY) minY = pt.y;
                    if (pt.y > maxY) maxY = pt.y;
                }
            }

            xData = CalculateDynData(model.xAxis, minX, maxX);
            yData = CalculateDynData(model.yAxis, minY, maxY);
        }

        private AxisDynamicData CalculateDynData(AxisConfig axis, decimal min, decimal max) {
            var result = new AxisDynamicData();
            if (min < 0 && axis.autoGrowToEncompassLowValues)
                result.min = min;
            if (max > axis.initialMax && axis.autoGrowToEncompassHighValues)
                result.max = max;
            else 
                result.max = axis.initialMax;

            var delta = result.max - result.min;
            if (delta <= 0m) delta = 1m;
            var grain = Math.Ceiling(Math.Log10((double)(delta)));
            var s = (decimal)Math.Pow(10, grain);

            if (s < 0.000001m) s = 0.000001m;

            var a = Math.Floor(result.min / s) * s;
            var b = Math.Floor(result.max / s + 1) * s;

            result.stepActual = s;
            result.min = a;
            result.max = b;

            return result;
        }
    }

    public struct AxisConfig {
        public string caption;
        
        public decimal initialMax;

        public bool autoGrowToEncompassHighValues;
        public bool autoGrowToEncompassLowValues;
    }

    public struct AxisDynamicData {
        public decimal min;
        public decimal max;
        public decimal stepActual;
    }

    public struct Datapoint {
        public decimal x;
        public decimal y;
    }

    public class Plot {
        public List<Datapoint> points = new();
    }

    public class ChartData {
        public string title;
        public AxisConfig   xAxis;
        public AxisConfig   yAxis;
        public List<Plot> plots = new List<Plot>();
    }
}
