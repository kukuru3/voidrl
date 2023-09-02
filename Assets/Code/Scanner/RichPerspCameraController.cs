using UnityEngine;

namespace Scanner {

    public delegate void ProgressChanged(float t);
    public class  RichPerspCameraController : MonoBehaviour
    {
        [SerializeField] float panMultiplier;
        [SerializeField] float mousePhiMultiplier;
        [SerializeField] float mouseThetaMultiplier;

        [SerializeField] float mouseWheelZoomMult;

        [SerializeField] float constRotation;

        private IOrbitCamera targetCam;

        private void Start() {
            targetCam = GetComponent<IOrbitCamera>();
        }

        private void Update() {
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

            targetCam.ApplyPan(delta, false);
            targetCam.Theta += Time.deltaTime * constRotation;

            var orbitChange = Input.mouseScrollDelta.y * mouseWheelZoomMult;

            var t = targetCam.GetOrbitDistanceNormalized();
            t = Mathf.Clamp01(t + orbitChange);
            targetCam.SetOrbitDistanceNormalized(t, true);
        }
    }
}