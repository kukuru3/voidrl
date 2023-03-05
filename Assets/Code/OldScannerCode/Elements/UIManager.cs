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

            var mouse = Input.mousePosition;
            var ray = uiCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hitinfo, 1000, 1<<5, QueryTriggerInteraction.Collide)) {
                var hilit = hitinfo.collider.gameObject.GetComponentInParent<Element>();
                SetHighlight(hilit);
            } else {
                SetHighlight(null);
            }
        }

        private void LateUpdate() {
            var pos = uiCamera.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0;
            if (cursor != null) cursor.position = pos;
        }
        private void SetHighlight(Element newHilite) {
            if (highlighted == newHilite) return;
            highlighted?.OnLostHilite();
            highlighted = newHilite;
            highlighted?.OnGainedHilite();
        }
    }
}