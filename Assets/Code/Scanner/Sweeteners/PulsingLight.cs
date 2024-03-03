using K3;
using UnityEngine;

namespace Scanner.Sweeteners {
    internal class PulsingLight : MonoBehaviour {
        Light lght;
        [SerializeField] float period;
        [SerializeField] float minIntensity;
        [SerializeField] float maxIntensity;
        private void Start() {
            lght = GetComponent<Light>();
        }

        private void Update() {
            var t = Mathf.Sin(Time.time * Mathf.PI * 2 / period);
            lght.intensity = t.Map(-1f, 1f, minIntensity, maxIntensity);
        }
    }
}
