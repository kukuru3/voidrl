using UnityEngine;

namespace Scanner {
    [DefaultExecutionOrder(999)]
    public class ScannerBlip : OldScanItem {
        float initialScale;
        protected override void Initialize() {
            initialScale = transform.localScale.x;
        }

        protected override void UpdateGraphics() {            
            FaceCamera();
            transform.localScale = Vector3.one * (initialScale * SizeMultiplier);
        }
    }
}