using UnityEngine;

namespace OldScanner {

    public class EffectsController : MonoBehaviour
    {
        [SerializeField] Camera finalDigitalCamera;
        [SerializeField] Camera geometryCamera;
        [SerializeField] OLDTVFilter3 filter;

        [SerializeField] [Range(1,10)]int scanlineMult;
        [SerializeField] [Range(-10f, 10f)] float staticSpeed;
        [SerializeField] [Range(10, 120)] int frameRate;

        private void Start() {

            QualitySettings.vSyncCount = 0;  // VSync must be disabled
            
        }

        private void Update() {
            filter.preset.scanlineFilter.lineCount = Screen.height / scanlineMult;
            filter.preset.staticFilter.staticOffset += staticSpeed * Time.deltaTime;
            finalDigitalCamera.orthographicSize = Screen.height / 2;

            Application.targetFrameRate = frameRate;
        }
    }
}