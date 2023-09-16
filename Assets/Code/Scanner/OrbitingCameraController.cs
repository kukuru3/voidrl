using UnityEngine;

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

            Vector3 RaycastOntoPlane(Transform transform, Vector3 offset) {
                var plane = new Plane(Vector3.up, targetCam.WorldFocus);
                var ray = new Ray(transform.TransformPoint(offset), transform.forward);
                if (!plane.Raycast(ray, out var q)) throw new System.InvalidCastException("Haha get it, invalid (ray)CAST");
                return ray.GetPoint(q);
            }

            Vector3 delta = default;
            if (Input.GetMouseButton(2)) {
                
                var a = RaycastOntoPlane(transform, default);
                var unitX = RaycastOntoPlane(transform, Vector3.right) - a;
                var unitY = RaycastOntoPlane(transform, Vector3.up) - a;
                var d = targetCam.GetOrbitDistance();
                delta =       mouseDelta.x * unitX * panMultiplier * d / UnityEngine.Screen.height
                            + mouseDelta.y * unitY * panMultiplier * d / UnityEngine.Screen.height;

                
                // project on plane:
                //delta.x = mouseDelta.x * panMultiplier;
                //delta.y = mouseDelta.y * panMultiplier;
            }
            targetCam.ApplyPan(delta, true);
        }
        
    }
}