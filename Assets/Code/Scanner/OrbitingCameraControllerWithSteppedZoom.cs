using UnityEngine;

namespace Scanner {
    public class OrbitingCameraControllerWithSteppedZoom: OrbitingCameraController {

        [SerializeField] CyclingZoomEffect zoomEffect;
        [SerializeField] bool intermediateZoom;

        protected override void Start() {
            base.Start();
            zoomEffect.OnProgressChanged += HandleZoomProgressChanged;
            zoomEffect.OnEnded += HandleZoomEffectEnded;
        }

        private void HandleZoomEffectEnded() {
            
        }

        private void HandleZoomProgressChanged(float t) {
            if (intermediateZoom || t >= 1f) { 
                var cameraZoom = Mathf.Lerp(initialZoom, targetZoom, t);
                this.targetCam.SetOrbitDistanceNormalized(cameraZoom, true);
            }
        }

        float initialZoom;
        float targetZoom;

        protected override void Update() {
            base.Update();

            if (!zoomEffect.IsAnimating) {
                var z = targetCam.GetOrbitDistanceNormalized();
                initialZoom = z;
                if (Input.mouseScrollDelta.y > 0 && z > 0.4f) { // we want to zoom in (get zoom to 0)
                    targetZoom = 0f;
                    zoomEffect.StartZoomEffect(targetCam, 0f);
                } else if (Input.mouseScrollDelta.y < 0 && z < 0.6f) { // we want to zoom out (get zoom to 1)
                    targetZoom = 1f;
                    zoomEffect.StartZoomEffect(targetCam, 1f);
                }
            }
        }
    }
}