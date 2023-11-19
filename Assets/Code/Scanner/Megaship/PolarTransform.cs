using UnityEngine;

namespace Scanner.Megaship {
    [ExecuteAlways]
    internal class PolarTransform : MonoBehaviour {
        [SerializeField] float angle;
        [SerializeField] float distance;

        void Update() {
            transform.localPosition = new Vector3(
                Mathf.Sin(angle* Mathf.Deg2Rad) * distance,
                Mathf.Cos(angle* Mathf.Deg2Rad) * distance,
                0
            );
            transform.localRotation = Quaternion.Euler(0, 0, -angle);
        }

    }
}
