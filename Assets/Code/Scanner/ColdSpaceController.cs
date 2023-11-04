using System;
using Scanner.ScannerView;
using UnityEngine;



namespace Scanner {
    [ExecuteAlways]
    public class ColdSpaceController : MonoBehaviour {
        [SerializeField] Camera explicitUIcamera;

        private void Start() {
            QualitySettings.vSyncCount = 0; 
            Application.targetFrameRate = 60;

            if (Application.isPlaying) { 
                mdCam = GenerateRenderMetadataCamera();
            }
        }

        Camera mdCam;

        RenderTexture GetAspectPreservingTexture(RenderTexture existingTexture, Camera templateCam, float multiplier) {
            var targetWidth = (int)(templateCam.pixelWidth * multiplier);
            var targetHeight = (int)(templateCam.pixelHeight * multiplier);
            if (existingTexture != null && existingTexture.width == targetWidth && existingTexture.height == targetHeight) {
                return existingTexture;
            } else {
                existingTexture.DiscardContents();
                Debug.Log($"Regenerating {targetWidth}x{targetHeight}");
                return new RenderTexture(targetWidth, targetHeight, 16, RenderTextureFormat.Default) {
                    name = "[GENERATED render metadata texture]"
                };
            }
        }

        private Camera GenerateRenderMetadataCamera() {
            var cloneCamera = Instantiate(explicitUIcamera);
            cloneCamera.name = "Render metadata";
            cloneCamera.clearFlags = CameraClearFlags.SolidColor;
            cloneCamera.cullingMask = LayerMask.GetMask("Rendering Metadata");
            var rt = new RenderTexture(512, 512, 16, RenderTextureFormat.Default) {
                name = "[GENERATED render metadata texture]"
            };
            cloneCamera.targetTexture = rt;
            foreach (var c in cloneCamera.GetComponents<Component>()) {
                if (c is Camera) continue;
                if (c is Transform) continue;
                Destroy(c);
            }
            cloneCamera.transform.parent = explicitUIcamera.transform;
            Shader.SetGlobalTexture("_RenderMetadata", rt);
            return cloneCamera.GetComponent<Camera>();
        }

        private void OnDestroy() {
            if (Application.isPlaying) {
                Shader.SetGlobalTexture("_RenderMetadata", null);
                if (mdCam != null) DestroyImmediate(mdCam.gameObject);
            }
        }

        Camera UICamera => explicitUIcamera ?? SceneUtil.UICamera;

        private void Update() {
            if (UICamera != null) { 
                UICamera.orthographicSize = Screen.height / 2;
            }
            if (mdCam != null) {
                mdCam.orthographicSize = UICamera.orthographicSize;
                mdCam.targetTexture = GetAspectPreservingTexture(mdCam.targetTexture, UICamera, 0.5f);
                Shader.SetGlobalTexture("_RenderMetadata", mdCam.targetTexture);
            }
        }
    }
}