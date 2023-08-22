using UnityEngine;

namespace Scanner.Windows {
    internal class NumberSlider : MonoBehaviour {
        [SerializeField] internal float min;
        [SerializeField] internal float max;
        [SerializeField] internal string suffix;
        [SerializeField] internal string format;
        [SerializeField] internal bool   logarithmic;

        [SerializeField] TMPro.TMP_Text text;
        protected Slider slider;

        private void Start() {
            slider = GetComponent<Slider>();
            slider.ValueChanged += OnSliderVC;
            OnSliderVC(slider.Value);
        }

        private void OnSliderVC(float value) {
            SyncText();
        }

        public float NumericValue { get {
            if (logarithmic) {
                var delta = max - min; if (delta <= 0) return min;
                var R = Mathf.Log(max, 2f) - Mathf.Log(min, 2f);
                var e0 = Mathf.Log(min, 2f);
                return Mathf.Pow(2, e0 + R * slider.Value);
            } else { 
                return Mathf.Lerp(min, max, slider.Value);
            }
        } }

        protected virtual void SyncText() {
            var num = NumericValue;
            var formatted = num.ToString(format);
            text.text = $"{formatted}{suffix}";
        }
    }
}
