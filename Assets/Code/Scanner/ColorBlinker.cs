using UnityEngine;

namespace OldScanner {

    public abstract class Blinker : MonoBehaviour {
        public int framesOn;
        public int framesOff;

        protected Shapes.ShapeRenderer rend;

        private void Start() {
            rend = GetComponent<Shapes.ShapeRenderer>();
            Initialize();
        }

        private void Update() {
            
            var id = 0;
            unchecked { 
                id = System.Math.Abs(Time.frameCount);
            };
            
            var phase = id % (framesOn + framesOff) < framesOn;
            UpdateGraphics(phase);

        }

        protected abstract void Initialize();

        protected abstract void UpdateGraphics(bool phase);
    }
    public class ColorBlinker : Blinker {
        public Color other;
        Color initial;
        
        protected override void Initialize() {            
            initial = rend.Color;
        }
        protected override void UpdateGraphics(bool phase) {
            rend.Color = phase ? other : initial;
        }
    }
}