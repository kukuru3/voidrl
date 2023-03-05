using UnityEngine;
using Core;

namespace Scanner {
    
    public class CameraController3D : MonoBehaviour {
        [SerializeField] Camera cameraProper;
        [SerializeField] Transform pivotPoint;

        [SerializeField] float minPhi;
        [SerializeField] float maxPhi;

        [SerializeField] float mousePhiMultiplier;
        [SerializeField] float mouseThetaMultiplier;
        [SerializeField] float mouseWheelZoomMult;

        [SerializeField] float minZoom;
        [SerializeField] float maxZoom;

        const float minZoomRaw = 1.0f;
        const float maxZoomRaw = 10.0f;

        [SerializeField] float zoomPower;
        [SerializeField] float panPow;
        
        [SerializeField] bool lockTheta;
        [SerializeField] bool lockPhi;

        SmoothFloat theta;
        SmoothFloat phi;
        SmoothFloat zoomRaw;

        SmoothFloat x;
        SmoothFloat y;

        [SerializeField] float smoothingFactor;

        private void Start() {
            theta = new SmoothFloat(45, smoothingFactor);
            phi = new SmoothFloat(30, smoothingFactor);
            zoomRaw = new SmoothFloat(1, smoothingFactor);

            phi.SetLimits(minPhi, maxPhi);
            zoomRaw.SetLimits(minZoomRaw, maxZoomRaw);

            x = new SmoothFloat(0, 0.02f);
            y = new SmoothFloat(0, 0.02f);
        }

        private void LateUpdate() {
            var pos = Input.mousePosition;
            mouseDelta = pos - prevMouse;
            prevMouse = pos;

            if (Input.GetMouseButton(1)) {
                theta.target += mouseDelta.x * mouseThetaMultiplier;
                phi.target += mouseDelta.y * mousePhiMultiplier;
            }

            zoomRaw.target += Input.mouseScrollDelta.y * mouseWheelZoomMult;
            
            var t = theta.SmoothValue;

            var p = phi.SmoothValue;

            if (lockTheta) t = 0;
            if (lockPhi) p = 0;
            pivotPoint.transform.rotation = Quaternion.Euler(p, t, 0);

            var zoomT = (zoomRaw.SmoothValue - minZoomRaw) / (maxZoomRaw - minZoomRaw);
            zoomT = Mathf.Pow(zoomT, zoomPower);
            var zoom = Mathf.Lerp(minZoom, maxZoom, zoomT);

            ApplyZoom(zoom);

            Vector3 RaycastOnDefaultVerticalPlane(Transform transform, Vector3 offset) {
                var plane = new Plane(Vector3.up, 0);
                var ray = new Ray(transform.TransformPoint(offset), transform.forward);
                if (!plane.Raycast(ray , out var q))
                    throw new System.InvalidCastException("Haha get it, invalid (ray)CAST");
                return ray.GetPoint(q);
            }

            if (Input.GetMouseButton(2)) { 
                // there's probably easier ways to project unit vectors onto the plane
                // I normally do it using sines and cosines.
                
                var tt = cameraProper.transform;
                var a = RaycastOnDefaultVerticalPlane(tt, default);
                var unitX = RaycastOnDefaultVerticalPlane(tt, Vector3.right) - a;
                var unitY = RaycastOnDefaultVerticalPlane(tt, Vector3.up)    - a;
                
                var offset = mouseDelta.x * unitX
                           + mouseDelta.y * unitY;

                offset *= panPow * zoom;

                x.target += offset.x;
                y.target += offset.z;
            }

            pivotPoint.transform.position = new Vector3(x.SmoothValue, 0, y.SmoothValue);
        }

        private void ApplyZoom(float zoom) {
            cameraProper.orthographicSize = Screen.height * 0.5f * zoom;
        }

        Vector3 prevMouse;
        Vector3 mouseDelta;
    }
}