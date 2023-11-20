using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scanner.Megaship.ShipFunctions {
    internal class RingMass : BaseMassProvider {
        [SerializeField][Range(0.01f, 5f)] float radius;
        [SerializeField] float zOffset;

        public override IEnumerable<PointMass> GetPointMasses() {
            var circumference = Mathf.PI * 2 * radius;
            var numPoints = Mathf.RoundToInt(circumference * ModuleMass.ProceduralResolution);
            if (numPoints < 3) numPoints = 3;
            var angleStep = 360f / numPoints;
            for (int i = 0; i < numPoints; i++) {
                var x = Mathf.Cos(angleStep * i * Mathf.Deg2Rad);
                var y = Mathf.Sin(angleStep * i * Mathf.Deg2Rad);
                var pos = new Vector3(x, y, 0) * radius;
                pos.z = zOffset;
                yield return new PointMass { localPosition = pos };
            }
        }
    }
}