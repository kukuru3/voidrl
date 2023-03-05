using UnityEngine;

namespace Core {
    public struct SmoothFloat {
        public float target;

        float _lastTime;
        float _velocity;
        float _value;

        float _limitMin;
        float _limitMax;
        bool  hasLimits;

        readonly float smoothing;

        public void SetLimits(float min, float max) {
            hasLimits = true;
            this._limitMin = min;
            this._limitMax = max;
        }

        public SmoothFloat(float initialValue, float smoothing) {
            this.smoothing = smoothing;
            _value = target = initialValue;
            _velocity = default;
            _lastTime = Time.time;
            hasLimits = false;
            _limitMin = float.MinValue;
            _limitMax = float.MaxValue;
        }

        void UpdateValue() {
            var delta = Time.time - _lastTime;
            _lastTime = Time.time;
            if (delta > float.Epsilon) {
                if (hasLimits) target = Mathf.Clamp(target, _limitMin, _limitMax);                
                _value = Mathf.SmoothDamp(_value, target, ref _velocity, smoothing, float.MaxValue, delta);
            }
        }

        public float SmoothValue { get { 
            UpdateValue(); return _value; 
        } }
    }
}