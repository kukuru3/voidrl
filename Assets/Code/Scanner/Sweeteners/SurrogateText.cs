using Scanner.ScannerView;
using TMPro;
using UnityEngine;

namespace Scanner.Sweeteners {
    // attach to a 3d object
    // when the 3d object is to be rendered by a camera,
    // this label will be rendered in the UI layer
    // at the same position where that object is.
    internal class SurrogateText : MonoBehaviour {
        [SerializeField] Vector2 offset;

        [SerializeField] float textSize;

        public bool  Display { get; set; } = true;

        Color baseColor;

        Transform referenceObject;
        TMP_Text text;
        private void Start() {
            gameObject.layer = 5;
            referenceObject = transform.parent;
            text = GetComponent<TMP_Text>();
            ColorMultiplier = Color.white;
            SizeMultiplier = 1f;
            baseColor = text.color;
        }

        private void LateUpdate() {
            var wp = SceneUtil.GetScannerCamera.WorldToScreenPoint(referenceObject.position);
            var outOfFrustum = (wp.z < SceneUtil.GetScannerCamera.nearClipPlane || wp.z > SceneUtil.GetScannerCamera.farClipPlane);
            
            var sp = SceneUtil.UICamera.ScreenToWorldPoint(wp + new Vector3(offset.x, offset.y));
            
            transform.position = sp;
            transform.rotation = SceneUtil.UICamera.transform.rotation;

            text.fontSize = textSize * SizeMultiplier;
            text.color = baseColor * ColorMultiplier;
            text.enabled = Display && !outOfFrustum;
        }

        public Color ColorMultiplier { get; set; }
        public float SizeMultiplier { get; set; }

    }
}
