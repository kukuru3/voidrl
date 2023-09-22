using UnityEngine;

namespace Scanner.TubeShip.View {
    [ExecuteAlways]
    internal class TubeTransform : MonoBehaviour {
        [field:SerializeField] public int ArcPos { get; set; }
        [field:SerializeField] public int AxisPos { get; set; }

        protected TubeView tubeView;

        protected virtual void LateUpdate() {
            if (tubeView == null) tubeView = GetComponentInParent<TubeView>();
            if (tubeView == null) return;

            var p = tubeView.GetTubePoint(AxisPos, ArcPos);

            transform.SetLocalPositionAndRotation(
                p.position, 
                Quaternion.LookRotation(Vector3.forward, p.up)
            );
        }
    }
}
