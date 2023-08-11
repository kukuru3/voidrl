using UnityEngine;

namespace OldScanner {
    [DefaultExecutionOrder(999)]
    public abstract class OldScanItem : MonoBehaviour {
        protected Camera cam;

        const float SCALE = 1f / 300;

        protected float SizeMultiplier { get; private set; }

        void Start() {
            
            Initialize();
        }

        protected virtual void Initialize() { }
        protected virtual void UpdateGraphics() { }

        protected void FaceCamera() {
            transform.rotation = Quaternion.LookRotation(cam.transform.forward, cam.transform.up);
        }
        
        void LateUpdate() {
            if (cam == null) cam = Camera.main;
            if (cam == null) return;
            SizeMultiplier = cam.orthographicSize * SCALE;

            UpdateGraphics();
        }
    }
}