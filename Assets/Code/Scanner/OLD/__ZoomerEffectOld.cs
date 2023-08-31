using Cysharp.Threading.Tasks;
using Scanner.Impl;
using Scanner.ScannerView;
using Shapes;
using UnityEngine;

namespace Scanner.Sweeteners {
    class __ZoomerEffectOld : MonoBehaviour {
        [SerializeField] Rectangle zoomer;        
        [SerializeField] int cycleDuration;
        [SerializeField] int cycleNum;
        [SerializeField] bool intermediate;
        [SerializeField] bool flash;

        bool animating;

        private void LateUpdate() {
            var cam= SceneUtil.GetScannerCamera.GetComponent<__StellarNavCameraOld>();

            var zoom0 = cam.GetOrbitDistanceNormalized();

            if (Input.mouseScrollDelta.y > 0 && !animating && zoom0 > 0.5f )
                StartZoomAnim(0.25f, 0.98f, 0f);

            if (Input.mouseScrollDelta.y < 0 && !animating && zoom0 < 0.5f)
                StartZoomAnim(0.98f, 0.25f, 1f);
        }
        public void StartZoomAnim(float percentageStart, float percentageEnd, float camD) {
            Zoom(percentageStart, percentageEnd, camD).Forget();
        }

        async UniTask Zoom(float from, float to, float targetZoom) {
            animating = true;

            var cam= SceneUtil.GetScannerCamera.GetComponent<__StellarNavCameraOld>();

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
                    FindObjectOfType<StarmapView>().SetSliderValue(11.5f + targetZoom * 5f * t);
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
            animating = false;
        }
    }
}
