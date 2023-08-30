using Core;
using UnityEngine;

namespace Scanner.ScannerView {
    public static class SceneUtil {
        static Camera uiCamera;
        static Camera scannerCam;
        public static Camera UICamera { get {
            if (uiCamera == null) uiCamera = Camera.main; 
            return uiCamera;
        } }

        public static Camera GetScannerCamera { get {
            if (scannerCam == null) scannerCam = CustomTag.Find(ObjectTags.ScannerCamera).GetComponent<Camera>();
            return scannerCam;
        } }
    }
}