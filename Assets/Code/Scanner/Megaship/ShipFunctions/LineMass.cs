using System.Collections.Generic;
using UnityEngine;



namespace Scanner.Megaship.ShipFunctions {
    internal class LineMass : BaseMassProvider {
        [SerializeField] Vector3 fromLocal;
        [SerializeField] Vector3 toLocal;

        public override IEnumerable<PointMass> GetPointMasses() {
            var d = fromLocal - toLocal;
            var numPoints = Mathf.RoundToInt(d.magnitude * ModuleMass.ProceduralResolution);
            if (numPoints < 3) numPoints = 3;
            for (var i = 0; i < numPoints; i++) {
                var p = Vector3.Lerp(fromLocal, toLocal, (float)i/(numPoints-1));
                yield return new PointMass { localPosition = p };
            }
        }
    }
}