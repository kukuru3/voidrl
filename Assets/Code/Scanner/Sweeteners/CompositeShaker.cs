using System;
using UnityEngine;

namespace Scanner.Sweeteners {
    internal class CompositeShaker : MonoBehaviour {

        [Serializable] internal struct Factor {
            [SerializeField]public float frequency;
            [SerializeField]public float amplitude;

            internal float randomStart;
        }

        [SerializeField] internal Factor[] xRot;
        [SerializeField] internal Factor[] yRot;
        [SerializeField] internal Factor[] zRot;

        [SerializeField] float amplitude = 1f;
        [SerializeField] float frequency = 1f;

        Vector3 initialPos;
        private void Start() {
            initialPos = transform.localPosition;
        }

        private void LateUpdate() {
            var x = Resolve(xRot);
            var y = Resolve(yRot);
            var z = Resolve(zRot);
            transform.localPosition = initialPos + new Vector3(x,y,z); // Quaternion.Euler(x, y, z);
        }

        float Resolve(Factor[] factors) {
            var sum = 0f;
            foreach (var f in factors) {
                var local = amplitude * f.amplitude * Mathf.Sin(Time.time * f.frequency * frequency * Mathf.PI * 2 );
                sum += local;
            }
            return sum;
        }
    }
}
