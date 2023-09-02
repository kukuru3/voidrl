using Scanner.ScannerView;
using UnityEngine;

namespace Scanner {

    public class EffectsController : MonoBehaviour
    {
        [SerializeField] OLDTVFilter3 filter;

        [SerializeField] [Range(1,10)]int scanlineMult;
        [SerializeField] [Range(-10f, 10f)] float staticSpeed;
        [SerializeField] [Range(10, 120)] int frameRate;
        [SerializeField] [Range(0.5f, 1.2f)] float compositeLineRatio;

        private void Start() {

            QualitySettings.vSyncCount = 0;              
        }

        private void Update() {
            //filter.preset.scanlineFilter.lineCount = Screen.height / scanlineMult;

            filter.preset.compositeFilter.lineCount = Mathf.RoundToInt(Screen.height * compositeLineRatio);
            filter.preset.staticFilter.staticOffset += staticSpeed * Time.deltaTime;
            SceneUtil.UICamera.orthographicSize = Screen.height / 2;
            
            Application.targetFrameRate = frameRate;
        }
    }
}