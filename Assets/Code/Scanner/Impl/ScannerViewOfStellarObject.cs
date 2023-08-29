using Core;
using Scanner.ScannerView;
using UnityEngine;
using Void.Entities.Components;

namespace Scanner.Impl {
    internal class ScannerViewOfStellarObject : MonoBehaviour {
        StellarObject so;

        CameraController3D camCtrlr;

        [SerializeField] GameObject main;
        [SerializeField] GameObject shadow;
        [SerializeField] Shapes.Line shadowLine;
        [SerializeField] TMPro.TMP_Text label;

        const float SCALE = 2.0f;

        public void Initialize(StellarObject obj) {
            this.so = obj;
            main.transform.position = obj.galacticPosition * SCALE;
            
            var zeroPos = obj.galacticPosition;
            zeroPos.y = 0;
            shadow.transform.position = zeroPos * SCALE;

            var h = obj.galacticPosition.y;
            shadowLine.Start = new Vector3(0,0,0);
            shadowLine.End = new Vector3(0, 0, h * SCALE);

            label.text = obj.name;

            if (Mathf.Abs(obj.galacticPosition.y)< 0.05f) {
                shadow.SetActive(false);
                shadowLine.gameObject.SetActive(false);
            }

            camCtrlr = CustomTag.Find(ObjectTags.ScannerCamera).GetComponent<CameraController3D>();
        }

        private void LateUpdate() {
            Debug.Log($"{camCtrlr.Zoom:F3}");
        }
    }
}
