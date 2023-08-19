using UnityEngine;

namespace OldScanner {
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

            var mouse = Input.mousePosition;
            var ray = uiCamera.ScreenPointToRay(Input.mousePosition);

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