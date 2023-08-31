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
}