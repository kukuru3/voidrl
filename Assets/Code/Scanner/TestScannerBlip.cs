using System;
using Scanner.AppContext;
using UnityEngine;

namespace Scanner.ScannerView {

    enum Displays {
        Original,
        Screen,
        ScreenWithFacing,
    }

    internal class TestScannerBlip : MonoBehaviour {

        const float SCALE_COMPENSATION = 0.3f;
        [SerializeField] Displays display;
        [SerializeField] float size;
        [SerializeField] bool adaptiveScale;

        [SerializeField] Transform worldspaceFacingTarget;

        // static CameraController3D scanCam;

        CameraController3D scanCam;

        private void LateUpdate() { 
            return;
            if (scanCam == null) scanCam = Void.App.Context.SceneReferences.Find<CameraController3D>();
            // scanCam ??= ;
            
            UpdateFacing(display, scanCam);
            
            if (adaptiveScale) { 
                var scale = size * Mathf.Pow(scanCam.Zoom, SCALE_COMPENSATION);            
                transform.localScale = Vector3.one * scale;
            }
            
        }

        private void UpdateFacing(Displays facing, CameraController3D camera ) {
            switch (facing) {
                case Displays.Screen:
                    transform.rotation = camera.transform.rotation;
                    break;
                case Displays.ScreenWithFacing:
                    if (worldspaceFacingTarget != null) {
                        var facingRight = worldspaceFacingTarget.position - transform.position;
                        var facingForward = camera.transform.forward;
                        var facingUp = Vector3.Cross(facingForward, facingRight);
                        transform.rotation = Quaternion.LookRotation(facingForward, facingUp);
                    }
                    break;
                
            }
        }
    }
}
