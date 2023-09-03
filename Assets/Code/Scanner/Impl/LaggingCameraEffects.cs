using UnityEngine;

namespace Scanner {
    public class LaggingCameraEffects : MonoBehaviour {
        private ILaggingCamera lagCam;

        [SerializeField] AudioSource target;
        [SerializeField] float multiplierPos;
        [SerializeField] float multiplierRot;
        [SerializeField] float threshold;

        private void Start() {
            lagCam = GetComponent<ILaggingCamera>();
            lagCam.LagUpdated += HandleLag;
        }

        float temperature;

        private void HandleLag(Pose delta) {
            var deltarot = Quaternion.Angle(Quaternion.identity, delta.rotation) * multiplierRot;
            var deltapos = delta.position * multiplierPos;
            var d = (deltarot + deltapos.magnitude) / Time.deltaTime;
            if (d > threshold) {
                TrySqueak();
            }
        }

        private void TrySqueak() {
            if (!target.isPlaying) target.Play();
        }

        private void LateUpdate() {
            
        }
    }
}