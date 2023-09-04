using K3;
using K3.UI;
using UnityEngine;

namespace Scanner {
    public class LaggingCameraEffects : MonoBehaviour {
        private ILaggingCamera lagCam;

        [SerializeField] AudioSource sourceRot;
        [SerializeField] AudioSource sourcePos;
        [SerializeField] AudioSource sourceFocus;
        [SerializeField] float thresholdRot;
        [SerializeField] float thresholdPos;
        [SerializeField] bool interrupt;

        float initVolume;
        private void Start() {
            lagCam = GetComponent<ILaggingCamera>();
            lagCam.LagUpdated += HandleLag;
            initVolume = sourceRot.volume;
            if (lagCam is IHasWorldFocus focuser) {
                focuser.NewFocusSet += OnFocusSet;
            }
        }

        private void OnFocusSet() {
            sourceFocus.Play();
        }

        float temperaturePos;
        float temperatureRot;
        
        private void HandleLag(Pose delta) {
            var deltarot = Quaternion.Angle(Quaternion.identity, delta.rotation);
            var deltapos = delta.position;
            temperaturePos += deltapos.magnitude / Time.deltaTime;
            temperatureRot += deltarot / Time.deltaTime;

            if (temperaturePos > thresholdPos) {
                temperaturePos = 0;
                TrySqueak(sourcePos);
            }

            if (temperatureRot > thresholdRot) {
                temperatureRot = 0;
                TrySqueak(sourceRot);
            }
        }

        private void TrySqueak(AudioSource source) {
            if (!source.isPlaying || interrupt) {
                if (interrupt && source.isPlaying) 
                    source.Stop();
                source.Play();
            }
        }
    }
}