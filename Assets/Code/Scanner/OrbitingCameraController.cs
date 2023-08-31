using UnityEngine;
using UnityEngine.UIElements;

namespace Scanner {
    public class  OrbitingCameraController : MonoBehaviour
    {
        [SerializeField] float panMultiplier;
        [SerializeField] float mousePhiMultiplier;
        [SerializeField] float mouseThetaMultiplier;

        protected IOrbitCamera targetCam;

        protected virtual void Start() {
            targetCam = GetComponent<IOrbitCamera>();
        }

        protected virtual void Update() {
            var mouseDelta = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);

            if (Input.GetMouseButton(1)) {
                targetCam.Theta += mouseDelta.x * mouseThetaMultiplier;
                targetCam.Phi   += mouseDelta.y * mousePhiMultiplier;
            }

            Vector3 delta = default;
            if (Input.GetMouseButton(2)) {
                delta.x = mouseDelta.x * panMultiplier;
                delta.y = mouseDelta.y * panMultiplier;
            }
            targetCam.ApplyPan(delta);
        }
        
    }

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
                Mathf.Lerp(initialZoom, targetZoom, t);
            }
        }

        float initialZoom;
        float targetZoom;

        protected override void Update() {
            base.Update();

            if (!zoomEffect.IsAnimating) {
                var z = targetCam.GetOrbitDistanceNormalized();
                initialZoom = z;
                if (Input.mouseScrollDelta.y > 0 && z < 0.99f) {
                    targetZoom = 1f;
                    zoomEffect.StartZoomEffect(targetCam, 1f);
                } else if (Input.mouseScrollDelta.y < 0 && z > 0.01f) {
                    targetZoom = 0f;
                    zoomEffect.StartZoomEffect(targetCam, 0f);
                }
            }
        }
    }
}