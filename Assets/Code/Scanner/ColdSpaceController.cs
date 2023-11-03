using Scanner.ScannerView;
using UnityEngine;



namespace Scanner {
    [ExecuteAlways]
    public class ColdSpaceController : MonoBehaviour {
        [SerializeField] Camera explicitUIcamera;

        private void Start() {
            QualitySettings.vSyncCount = 0; 
            Application.targetFrameRate = 60;
        }

        private void Update() {
            var cam = explicitUIcamera ?? SceneUtil.UICamera;
            if (cam != null) cam.orthographicSize = Screen.height / 2;
        }
    }
}