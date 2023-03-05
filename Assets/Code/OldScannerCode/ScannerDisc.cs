using UnityEngine;

namespace Scanner {
    [DefaultExecutionOrder(999)]
    public class ScannerDisc : OldScanItem {
        Shapes.Disc disc;
        [SerializeField] bool planarCircle;
        [SerializeField][Range(1f, 100f)]float discRadius;
        [SerializeField][Range(0f,10f)]float discThickness;
        protected override void Initialize() {
            disc = GetComponent<Shapes.Disc>();
        }
        protected override void UpdateGraphics() {
            if (planarCircle) transform.rotation = Quaternion.Euler(90,0,0);
            else FaceCamera();
            disc.Radius = discRadius;
            disc.Thickness = discThickness * SizeMultiplier;
        }
    }
}