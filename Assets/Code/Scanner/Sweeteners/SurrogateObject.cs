using K3;
using Scanner.ScannerView;
using UnityEngine;

namespace Scanner.Sweeteners {
    internal class SurrogateObject : MonoBehaviour {
        [SerializeField] Vector2 offset;
        [SerializeField] float scale;
        [SerializeField] Shapes.ShapeRenderer[] targetRenderers;
        

        public float ScaleMultiplier { get; set; } = 1f;

        public bool  Display { get; set; } = true;

        private Transform referenceObject;
        private void Start() {
            gameObject.layer = 5;
            referenceObject = transform.parent;
        }

        public bool InFrustum { get; private set; }

        private void LateUpdate() {
            var screenPos = SceneUtil.GetScannerCamera.WorldToScreenPoint(referenceObject.position);
            var outOfFrustum = (screenPos.z < SceneUtil.GetScannerCamera.nearClipPlane || screenPos.z > SceneUtil.GetScannerCamera.farClipPlane);

            var p = SceneUtil.GetScannerCamera.WorldToViewportPoint(referenceObject.position);
            if (p.x < 0f || p.x > 1 || p.y < 0 || p.y > 1) outOfFrustum = true;
            
            var wp = SceneUtil.UICamera.ScreenToWorldPoint(screenPos + new Vector3(offset.x, offset.y));
            transform.position = wp;
            transform.rotation = SceneUtil.UICamera.transform.rotation;

            InFrustum = !outOfFrustum;
            foreach (var r in targetRenderers) {
                r.enabled = Display && InFrustum;            
            }
            transform.localScale = Vector3.one * (scale * ScaleMultiplier);
            // Vector3.one * screenPos.z.Map(SceneUtil.GetScannerCamera.farClipPlane, SceneUtil.GetScannerCamera.nearClipPlane, scaleMinDist, scaleMaxDist);

        }
    }
}
