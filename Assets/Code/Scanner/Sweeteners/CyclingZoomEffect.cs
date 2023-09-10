using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Shapes;
using UnityEngine;

namespace Scanner {
    
    public class CyclingZoomEffect : MonoBehaviour {
        [SerializeField] int cycles;
        [SerializeField] int cycleDuration;
        [SerializeField] Rectangle rectangle;
        // [SerializeField] bool intermediate;
        [SerializeField] bool flashEnd;

        [SerializeField] AudioClip clipCycle;
        [SerializeField] AudioClip playWhenDone;

        [SerializeField] int audioHeadMS;

        public event ProgressChanged OnProgressChanged;
        public event Action OnEnded;

        internal bool IsAnimating { get; private set; }

        public void StartZoomEffect(IOrbitCamera camera, float targetOrbitDist) {
            if (IsAnimating) return;

            var initialOrbitDist = camera.GetOrbitDistanceNormalized();
            var delta = targetOrbitDist - initialOrbitDist;
            if (delta < 0f) {
                // target > initial, we are ZOOMING IN.
                StartAnim(camera, 0.3f, 0.95f).Forget();
            } else {
                StartAnim(camera, 0.95f, 0.3f).Forget();
            }
        }

        async UniTask StartAnim(IOrbitCamera camera, float screenFrom, float screeenTo) {
            IsAnimating = true;
            rectangle.Type = Rectangle.RectangleType.HardBorder;

            var initialZoom = camera.GetOrbitDistanceNormalized();
            
            rectangle.enabled = true;

            var sources = GetComponents<AudioSource>();
            
            for (var c = 0; c < cycles; c++) {
                var t = (float)c / (cycles - 1);
                OnProgressChanged?.Invoke(t);
                sources[0].PlayOneShot(clipCycle, 1f);
                var isLastCycle = c == cycles-1;
                
                if (isLastCycle) sources[1].PlayOneShot(playWhenDone, 1f);

                await UniTask.Delay(audioHeadMS);

                var x = Mathf.Lerp(screenFrom, screeenTo, t);
                rectangle.enabled = true;
                rectangle.Width = UnityEngine.Screen.width * x;
                rectangle.Height = UnityEngine.Screen.height * x;
                
                //if (intermediate) {                     
                //    camera.SetOrbitDistanceNormalized(Mathf.Lerp(initialZoom, targetZoom, t), true);
                //    // FindObjectOfType<StarmapView>().SetSliderValue(11.5f + targetZoom * 5f * t);
                //}


                if (isLastCycle && flashEnd) {
                    rectangle.Type = Rectangle.RectangleType.HardSolid;
                    rectangle.Width = UnityEngine.Screen.width;
                    rectangle.Height = UnityEngine.Screen.height;
                }

                for (var f = 0; f < cycleDuration; f++) {
                    await UniTask.DelayFrame(1);
                    rectangle.enabled = false;
                    await UniTask.DelayFrame(2);
                    rectangle.enabled = true;
                }
            }

            await UniTask.DelayFrame(2);
            rectangle.enabled = false;
            IsAnimating = false;

            OnEnded?.Invoke();


        }
    }
}