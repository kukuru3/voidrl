using UnityEngine;

namespace Scanner.HexShip {
    internal class DetailedHexTransform : HexTransform {
        [SerializeField] float angleOffset;
        protected override float ExtraAngle() => angleOffset;
    }
}
