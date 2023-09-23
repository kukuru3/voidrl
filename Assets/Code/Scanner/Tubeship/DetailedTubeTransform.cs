using UnityEngine;

namespace Scanner.TubeShip.View {


    [ExecuteAlways]
    internal class DetailedTubeTransform : TubeTransform {
        [field:SerializeField] public float ArcOffset { get; set; }
        [field:SerializeField] public float AxisOffset { get; set; }

        [field:SerializeField] public float UpOffset { get; set; }

        [field:SerializeField] public float AddedRotation { get; set; }

        protected override void LateUpdate() {
            base.LateUpdate();
            var p = tubeView.GetUnrolledTubePoint(AxisOffset + AxisPos, ArcOffset + ArcPos, tubeView.Unroll);
            transform.SetLocalPositionAndRotation(
                p.pos + p.up * UpOffset, 
                Quaternion.LookRotation(Vector3.forward, p.up) * Quaternion.Euler(0, 0, AddedRotation)
            );
        }
    }
}
