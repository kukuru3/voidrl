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

                // apscissa and ordinate
                Draw.Line(Vector3.zero, Vector3.right * (w + 20), Color.white);
                Draw.Line(Vector3.zero, Vector3.up * (h + 20), Color.white);

                // arrow
                var m = Vector3.right * (w + 20);
                Draw.Line(m, m + new Vector3(-11, 4, 0), Color.white);
                Draw.Line(m, m + new Vector3(-11,-4, 0), Color.white);

                // arrow
                m = Vector3.up * (h + 20);
                Draw.Line(m, m + new Vector3(4, -11, 0), Color.white);
                Draw.Line(m, m + new Vector3(-4,-11, 0), Color.white);

                Draw.Thickness = lineWidth / 2;
                yData = new AxisDynamicData {
                    min = 0,
                    max = 214, 
                    stepActual = 25,
                };

                xData = yData;

                // if (model == null) return;

                var data = yData;
                var recursionGuard = 0;
                for (var i = data.min + data.stepActual; i <= data.max; i += data.stepActual) { 
                    recursionGuard++; if (recursionGuard >= 1000) break;
                    
                    var value = (float)Map(i, data.min, data.max, 0, (decimal)h, true);
                    using (Draw.DashedScope()) { 
                        Draw.Line(new Vector3(0, value, 0), new Vector3(w, value, 0), lineWidth / 2, Color.gray);
                    }

                    Draw.Line(new Vector3(-4, value, 0), new Vector3(4, value, 0), lineWidth, Color.white);

                    Draw.Text(pos: new Vector3(-5 , value, 0), fontSize: 180, content: $"{i}", color: Color.gray, align: TextAlign.MidlineRight);
                }

                data = xData;
                for (var i = data.min + data.stepActual; i <= data.max; i += data.stepActual) {
                    recursionGuard++; if (recursionGuard >= 1000) break;

                    var value = (float)Map(i, data.min, data.max, 0, (decimal)w, true);
                    using (Draw.DashedScope()) {
                        Draw.Line(new Vector3(value, 0, 0), new Vector3(value, h, 0), lineWidth / 2, Color.gray);
                    }
                    Draw.Line(new Vector3(value, -4, 0), new Vector3(value, 4, 0), lineWidth, Color.white);
                    Draw.Text(pos: new Vector3(value, -5, 0), fontSize: 180, content: $"{i}", color: Color.gray, align: TextAlign.Top);
                }
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



        static public ChartData GenerateFunctionChart(decimal intervalStart, decimal intervalEnd, NumericFunction fn) {
            var resolution = 300;
            var pts = new List<Datapoint>();
            for (var i = 0; i < resolution; i++) {
                var x = Map(i, 0, resolution, intervalStart, intervalEnd);
                var y = fn(x);
                pts.Add(new Datapoint { x = x, y = y });
            }
            return new ChartData() {
                plots = new List<Plot>() { new Plot() { points = pts } },
                title = "Autogenerated",
                xAxis = new AxisConfig { initialMax = intervalEnd },
                yAxis = new AxisConfig { autoGrowToEncompassHighValues = true, autoGrowToEncompassLowValues = true, caption = "Foo", initialMax = 1 },
            };
        }
    }

    public delegate decimal NumericFunction(decimal d);


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
