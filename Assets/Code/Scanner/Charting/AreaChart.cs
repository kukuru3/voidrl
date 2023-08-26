using System.Collections.Generic;
using System.Linq;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scanner.Charting {
    [ExecuteAlways]
    internal class AreaChart : ImmediateModeShapeDrawer {

        [System.Serializable]
        public struct Entry {
            public float amount;
            public Color color;
            public string name;
        }
        [SerializeField] List<Entry> entries;

        [SerializeField] float w;
        [SerializeField] float hPerAmountUnit;
        [SerializeField] float fixedHeight;

        [SerializeField] float rimThickness;
        [SerializeField] float rimDistance;
        [SerializeField] Color rimColor;

        public void ClearEntries() {
            entries.Clear();
        }
        public void AddEntry(string name, float amount, Color color) {
            var entry = new Entry { amount = amount, name = name, color = color };
            entries.Add(entry);
        }

        public override void DrawShapes(Camera cam) {

            if (!cam.name.Contains("UI")) return;
            //Draw.UseGradientFill = true;

            //Draw.GradientFill = new GradientFill() { colorStart = Color.white, colorEnd = Color.white, linearStart = Vector2.zero, linearEnd = Vector2.right, type = FillType.LinearGradient, space = FillSpace.Local};
            using (Draw.Command(cam, UnityEngine.Rendering.CameraEvent.AfterForwardOpaque)) {
                if (entries == null || entries.Count == 0) return;
                var sumAll = entries.Where(e => e.amount >= float.Epsilon).Sum(e => e.amount);
                Draw.Matrix = transform.localToWorldMatrix;

                var delimiterWidth = 2;
                var widthAvailable = w - delimiterWidth * (entries.Count-1);
                var h = hPerAmountUnit * sumAll;
                if (h < 20) h = 20;
                if (fixedHeight >= 1) h = fixedHeight;

                var previouslabelAlsoCouldntFit = false;
                var bottomlabelOffsetIndex = 0;

               
                var x0 = 0f;
                foreach (var entry in entries) {
                    var wEntry = widthAvailable * entry.amount / sumAll;
                    if (entry.amount < float.Epsilon) continue;
                    if (wEntry < 1) wEntry = 1;
                    // var f = Draw.GradientFill; f.colorStart = entry.color; f.colorEnd = entry.color;
                    Draw.Rectangle(new Vector3(x0 + wEntry / 2, -h/2 , 0), wEntry, h, color: entry.color);
                    var criticalTextWidth = 8 * entry.name.Length;
                    if (wEntry < criticalTextWidth) {
                        if (previouslabelAlsoCouldntFit) bottomlabelOffsetIndex++;
                        Draw.Text(pos: new Vector3(x0 + wEntry / 2, -h - bottomlabelOffsetIndex * 15, 0), content: entry.name, color: entry.color, fontSize: 150, align: TextAlign.Top);                        
                        previouslabelAlsoCouldntFit = true;
                    } else {
                        Draw.Text(pos: new Vector3(x0 + wEntry / 2, -h/2, 0), content: entry.name, color: Color.white, fontSize: 150, align: TextAlign.Center);
                        previouslabelAlsoCouldntFit = false;
                        bottomlabelOffsetIndex = 0;
                    }
                    x0 += wEntry;
                    x0 += delimiterWidth;
                }

                 if (rimThickness > 0) { 
                    var rd = rimDistance + rimThickness;
                    Draw.RectangleBorder(new Vector3(0,-h,0), new Rect(-rd, -rd, w + 2 * rd, h + 2 * rd), rimThickness, rimColor);
                    // Draw.Rectangle(new Vector3(0, -h, 0), new Rect(-rimDistance, -rimDistance, w + 2 * rimDistance, h + 2 * rimDistance), /* rimThickness, */ rimColor);
                }
            }
        }
    }
}
