using Core;
using UnityEngine;

namespace Scanner.Sweeteners {
    internal class TextBillboard : MonoBehaviour {
        private GameObject scanCam;

        private void Start() {
            scanCam = CustomTag.Find(ObjectTags.ScannerCamera);
        }

        private void LateUpdate() {
            transform.rotation = Quaternion.LookRotation(scanCam.transform.forward, scanCam.transform.up);
        }
    }
}
