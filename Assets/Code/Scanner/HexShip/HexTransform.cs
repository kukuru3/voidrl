using UnityEngine;

namespace Scanner.HexShip {
    [ExecuteAlways]
    internal class HexTransform : MonoBehaviour {

        [SerializeField] int zed;
        [SerializeField] int d; 
        [SerializeField] int alpha;

        protected virtual void LateUpdate() {
            var angle = d switch {
                0 => 0,
                1 => 60,
                2 => 30,
                3 => 20,
                4 => 15,
                _ => 0,
            };
            var distance = d * Mathf.Sqrt(3);
            var x = distance * Mathf.Sin((angle * alpha) * Mathf.Deg2Rad);
            var y = distance * Mathf.Cos((angle * alpha) * Mathf.Deg2Rad);
            var z = zed * 2f; 

            transform.localPosition = new Vector3(x, y, z);
            transform.localRotation = Quaternion.Euler(0, 0, -angle * alpha + ExtraAngle());
        }

        protected virtual float ExtraAngle() => 0f;

    }
}
