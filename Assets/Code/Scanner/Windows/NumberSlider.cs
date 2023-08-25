using K3;
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

        private void Awake() {
            slider = GetComponent<Slider>();
            slider.ValueChanged += OnSliderVC;
            OnSliderVC(slider.Value);
        }

        private void OnSliderVC(float value) {
            SyncText();
        }

        public void SetSliderTFromValue(float newNumericValue) {
            if (logarithmic) {
                var s = Mathf.Log10(min);
                var D = Mathf.Log10(max) - s;
                var v01 = (Mathf.Log10(newNumericValue) - s) / D;
                slider.SetValueExternal(v01);
            } else {
                slider.SetValueExternal(newNumericValue.Map(min, max, 0f, 1f));
            }
        }

        public float NumericValue    { get {
            if (logarithmic) {
                var s = Mathf.Log10(min);
                var D = Mathf.Log10(max) - s;
                return Mathf.Pow(10, s + slider.Value * D);
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
