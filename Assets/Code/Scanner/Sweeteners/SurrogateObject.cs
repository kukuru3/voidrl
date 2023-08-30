using K3;
using Scanner.ScannerView;
using UnityEngine;

namespace Scanner.Sweeteners {
    internal class SurrogateObject : MonoBehaviour {
        [SerializeField] Vector2 offset;
        [SerializeField] MonoBehaviour[] toDisable;

        [SerializeField] float scaleMinDist;
        [SerializeField] float scaleMaxDist;

        private Transform referenceObject;
        private void Start() {
            gameObject.layer = 5;
            referenceObject = transform.parent;
        }

        private void LateUpdate() {
            var screenPos = SceneUtil.GetScannerCamera.WorldToScreenPoint(referenceObject.position);
            var outOfFrustum = (screenPos.z < SceneUtil.GetScannerCamera.nearClipPlane || screenPos.z > SceneUtil.GetScannerCamera.farClipPlane);            
            var wp = SceneUtil.UICamera.ScreenToWorldPoint(screenPos + new Vector3(offset.x, offset.y));
            transform.position = wp;
            transform.rotation = SceneUtil.UICamera.transform.rotation;
            foreach (var c in toDisable) {
                c.enabled = referenceObject.gameObject.activeInHierarchy && !outOfFrustum;
            }

            transform.localScale = Vector3.one * screenPos.z.Map(SceneUtil.GetScannerCamera.farClipPlane, SceneUtil.GetScannerCamera.nearClipPlane, scaleMinDist, scaleMaxDist);
        }
    }
}
