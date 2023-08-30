using Cysharp.Threading.Tasks;
using Scanner.Impl;
using Scanner.ScannerView;
using Shapes;
using UnityEngine;

namespace Scanner.Sweeteners {
    class ZoomerEffect : MonoBehaviour {
        [SerializeField] Rectangle zoomer;        
        [SerializeField] int cycleDuration;
        [SerializeField] int cycleNum;
        [SerializeField] bool intermediate;
        [SerializeField] bool flash;

        private void LateUpdate() {
            if (Input.GetKeyDown(KeyCode.Z)) StartZoomAnim(0.3f, 0.98f, 0f);
            if (Input.GetKeyDown(KeyCode.X)) StartZoomAnim(0.98f, 0.3f, 1f);
        }
        public void StartZoomAnim(float percentageStart, float percentageEnd, float camD) {
            Zoom(percentageStart, percentageEnd, camD).Forget();
        }

        async UniTask Zoom(float from, float to, float targetZoom) {
            var cam= SceneUtil.GetScannerCamera.GetComponent<StellarNavCamera>();

            var deltaInCycle = (to - from) / (cycleNum-1);

            zoomer.Type = Rectangle.RectangleType.HardBorder;
            var zoom0 = cam.GetOrbitDistanceNormalized();
            
            zoomer.enabled = true;
            
            for (var c = 0; c < cycleNum; c++) {
                var t = (float)c / (cycleNum - 1 );
                // t = Mathf.Pow(t, 4);

                var x = Mathf.Lerp(from, to, t);
                zoomer.Width = Screen.width * x;
                zoomer.Height = Screen.height * x;
                
                if (intermediate) { 
                    
                    cam.OverrideOrbitDistance(Mathf.Lerp(zoom0, targetZoom, Mathf.Pow(t, 0.5f)));
                }

                if (flash && c == cycleNum - 1) zoomer.Type = Rectangle.RectangleType.HardSolid;
                for (var f = 0; f < cycleDuration; f++) {
                    await UniTask.DelayFrame(1);
                    zoomer.enabled = false;
                    await UniTask.DelayFrame(2);
                    zoomer.enabled = true;
                }
            }
            cam.OverrideOrbitDistance(targetZoom);
            await UniTask.DelayFrame(2);
            zoomer.enabled = false;
            FindObjectOfType<StarmapView>().SetSliderValue(11.5f + targetZoom * 5f);
        }
    }
}
