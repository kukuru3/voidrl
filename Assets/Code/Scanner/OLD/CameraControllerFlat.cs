using UnityEngine;
using Core;

namespace Scanner {
    public class CameraControllerFlat : MonoBehaviour {
        [SerializeField] Camera cameraProper;
        [SerializeField] Transform pivotPoint;

        [SerializeField] float minZoom;
        [SerializeField] float maxZoom;

        [SerializeField] float zoomPower;

        [SerializeField] float mouseWheelZoomMult;

        [SerializeField] float panPow;

        const float minZoomRaw = 1.0f;
        const float maxZoomRaw = 10.0f;
        SmoothFloat zoomRaw;

        SmoothFloat x;
        SmoothFloat y;

        private void ApplyZoom(float zoom) {
            cameraProper.orthographicSize = UnityEngine.Screen.height * 0.5f * zoom;
        }

        Vector3 prevMouse;
        Vector3 mouseDelta;

        private void Start() {
            zoomRaw = new SmoothFloat(minZoomRaw, 0.1f);
            x = new SmoothFloat(0, 0.02f);
            y = new SmoothFloat(0, 0.02f);
            zoomRaw.SetLimits(minZoomRaw, maxZoomRaw);
        }

        private void LateUpdate() {
            zoomRaw.target += Input.mouseScrollDelta.y * mouseWheelZoomMult;
            
            var zoomT = (zoomRaw.SmoothValue - minZoomRaw) / (maxZoomRaw - minZoomRaw);
            zoomT = Mathf.Pow(zoomT, zoomPower);
            var zoom = Mathf.Lerp(minZoom, maxZoom, zoomT);
            ApplyZoom(zoom);

            var pos = Input.mousePosition;
            mouseDelta = pos - prevMouse;
            prevMouse = pos;
            
            if (Input.GetMouseButton(2)) { 
                x.target += mouseDelta.x * panPow * zoom ;
                y.target += mouseDelta.y * panPow * zoom ;
            }

            pivotPoint.transform.rotation = Quaternion.Euler(90, 0, 0);
            pivotPoint.transform.position = new Vector3(x.SmoothValue, 0, y.SmoothValue);
        }
    }
}