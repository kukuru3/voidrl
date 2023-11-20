using System.Collections.Generic;
using UnityEngine;

namespace Scanner.Megaship.ShipFunctions {
    internal class SimplePointMass : BaseMassProvider {
        public override IEnumerable<PointMass> GetPointMasses() {
            yield return new PointMass { localPosition = Vector3.zero };
        }
    }
}