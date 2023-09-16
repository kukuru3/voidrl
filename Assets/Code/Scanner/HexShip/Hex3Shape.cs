using Core.h3x;
using K3.Hex;
using Shapes;
using UnityEngine;

namespace Scanner.HexShip {

    [ExecuteAlways]
    internal class Hex3Shape : ImmediateModeShapeDrawer {
        [SerializeField] float hexSize = 1f;
        [SerializeField] float hexH = 1f;

        [SerializeField] [Range(1, 5)]int mapRadius = 0;
        [SerializeField] [Range(1, 10)]int mapH = 0;

        [SerializeField] GridTypes type;

        [SerializeField] float drawMargin;
        [SerializeField] float thiccness;

        public override void DrawShapes(Camera cam) {
            var hexes = Hexes.InRadius(default, mapRadius);

            using (Draw.Command(cam, UnityEngine.Rendering.CameraEvent.AfterForwardOpaque)) {            
                for (var z = 0; z < mapH; z++) {
                    foreach (var hex in hexes) {
                        var h3 = new Hex3(hex, z);
                        var center = CenterOf(h3);
                        Draw.RegularPolygonBorder(sideCount: 6, radius: hexSize - drawMargin, thickness: thiccness, color: Color.white, pos: transform.TransformPoint(center) ); //- Vector3.forward * hexH / 2);
                        // Draw.RegularPolygonBorder(sideCount: 6, radius: hexSize / 2, thickness: 1f, color: Color.white, pos: center + Vector3.forward * hexH / 2);
                    }
                }
            }
        }

        Vector3[] FlatHexagon(GridTypes type, float d) {
            var result = new Vector3[6];
            for (var i = 0; i < 6; i++) {
                var angle = i * 60 * Mathf.Deg2Rad;
                var x = Mathf.Cos(angle) * d;
                var y = Mathf.Sin(angle) * d;
                result[i] = type == GridTypes.PointyTop ? new Vector3(x,y, 0) : new Vector3(y,x,0);
            }
            return result;
        }

        Vector3[] FlatHexagon(GridTypes type, float d, float z) {
            var result = new Vector3[6];
            for (var i = 0; i < 6; i++) {
                var angle = i * 60 * Mathf.Deg2Rad;
                var x = Mathf.Cos(angle) * d;
                var y = Mathf.Sin(angle) * d;
                result[i] = type == GridTypes.PointyTop ? new Vector3(x,y,z) : new Vector3(y,x,z);

            }
            return result;
        }

        public Vector3 CenterOf(Hex3 c) {
            var px = Hexes.HexToPixel(c.hex, GridTypes.FlatTop, hexSize);
            return new Vector3(px.x, px.y, c.zed * hexH);
        }
    }
}
