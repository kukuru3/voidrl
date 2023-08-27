using System.IO.IsolatedStorage;
using UnityEngine;

namespace Scanner {
    class UIManager : MonoBehaviour {
        [SerializeField] Camera uiCamera;
        [SerializeField] bool hideCursor;
        [SerializeField] Transform cursor;

        Element highlighted;

        private void Start() {
            
        }

        private void OnDestroy() {
            Cursor.visible = true;
        }

        private void Update() {
            if (uiCamera == null) return;

            if (hideCursor) Cursor.visible = false;

            // var mouse = Input.mousePosition;

            var distortedMouse = GetDistortedCursorPos();
            var ray = uiCamera.ScreenPointToRay(distortedMouse);

            if (Physics.Raycast(ray, out var hitinfo, 1000, 1<<5, QueryTriggerInteraction.Collide)) {
                var hilit = hitinfo.collider.gameObject.GetComponentInParent<Element>();
                var localPosition = hitinfo.collider.transform.InverseTransformPoint(hitinfo.point);
                var x = 0f; var y = 0f;
                if (hitinfo.collider is BoxCollider bc) {                 
                    x = localPosition.x / bc.size.x * 2;
                    y = localPosition.y / bc.size.y * 2;
                }

                // Debug.Log($"Hitloc: {hitinfo.collider.name} : {x:F3}, {y:F3}");
                SetHighlight(hilit, new Vector2(x, y));
            } else {
                SetHighlight(null);
            }
        }

        public static Vector3 GetDistortedCursorPos() {
            var p = Input.mousePosition;
            var distortion = 0.2f;
            var uv = new Vector2(p.x / Screen.width, p.y / Screen.height);
            var uv2 = Distort(uv, distortion);

            var sx = uv2.x * Screen.width; 
            var sy = uv2.y * Screen.height;

            return new Vector3(sx, sy, 0);
        }

        static Vector2 Distort(Vector2 uv, float distortion) {
            var c = uv - Vector2.one * 0.5f;
            var dt = Vector2.Dot(c, c) * distortion;
            dt -= 0.2f * distortion;
            return uv + c * ((1f + dt) * dt);
        }

        private void LateUpdate() {
            var pos = uiCamera.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0;
            if (cursor != null) cursor.position = pos;
        }
        private void SetHighlight(Element newHilite, Vector2 localPosition = default) {
            if (newHilite != null) newHilite.LastCursorLocalPos = localPosition;
            if (highlighted == newHilite) return;
            highlighted?.OnLostHilite();
            highlighted = newHilite;
            highlighted?.OnGainedHilite();
        }
    }
}