using System;
using K3;
using UnityEngine;

namespace Scanner {

    public interface IHasWorldFocus {
        Vector3 WorldFocus { get; }
        void Focus(Vector3 newCenter, bool immediate);

    }

    public interface IOrbitCamera {
        float Theta { get; set; }
        float Phi { get; set; }

        void ApplyPan(Vector3 cameraspaceDelta);
        
        float GetOrbitDistanceNormalized();
        void SetOrbitDistanceNormalized(float normalizedValue, bool immediate);
    }
    

    public class  SteppedPerspectiveCamera : MonoBehaviour, IHasWorldFocus, IOrbitCamera {
        [Header("Limits")]
        [SerializeField] float orbitDistanceMin;
        [SerializeField] float orbitDistanceMax;

        [SerializeField] float minPhi;
        [SerializeField] float maxPhi;

        [Header("Smoothing")]
        [SerializeField][Range(0.01f, 0.8f)] float centerSmoothTime;
        [SerializeField] [Range(0.01f, 0.8f)]float orbitDSmoothTime;

        [Header("Steps")]
        [SerializeField][Range(0f, 20f)] float angleStep;
        [SerializeField][Range(0f, 1f)] float posStep;

        public float Theta { get; set; }
        public float Phi { get; set; }

        float orbitD;

        float targetOrbitD;
        float _orbitDVel;


        Vector3 center;
        Vector3 _vel;
        Vector3 target;

        Vector3 IHasWorldFocus.WorldFocus => center;

        public void Focus(Vector3 newCenter, bool immediate) {
            target = newCenter;
            if (immediate) center = target;
        }

        public void SetOrbitDistanceNormalized(float normalizedValue, bool immediate) {
            var d = Mathf.Lerp(orbitDistanceMin, orbitDistanceMax, normalizedValue);
            targetOrbitD = d;
            if (immediate) orbitD = d;
        }

        public float GetOrbitDistanceNormalized() => orbitD.Map(orbitDistanceMin, orbitDistanceMax, 0f, 1f);
        
        public void ApplyPan(Vector3 cameraspaceDelta) {
            var worldspacePan = transform.TransformVector(cameraspaceDelta);
            
            center += worldspacePan;
            target += worldspacePan;
        }

        private void LateUpdate() {
            targetOrbitD = Mathf.Clamp(targetOrbitD, orbitDistanceMin, orbitDistanceMax);
            UpdateInterpolators();
            UpdatePosition();
        }

        private void UpdateInterpolators() {
            center = Vector3.SmoothDamp(center, target, ref _vel, centerSmoothTime);
            orbitD = Mathf.SmoothDamp(orbitD, targetOrbitD, ref _orbitDVel, orbitDSmoothTime);
        }
        

        private void UpdatePosition() { 
            Phi = Mathf.Clamp(Phi, minPhi, maxPhi);

            var finalTheta = Theta;
            var finalPhi = Phi;

            var trueCenter = center;

            if (posStep > 0f) { 
                trueCenter = stepize(center, posStep);
            }

            if (angleStep > float.Epsilon) { 
                finalTheta = Mathf.Floor(Theta / angleStep) * angleStep;
                finalPhi   = Mathf.Floor(Phi/ angleStep) * angleStep;
            }
            transform.rotation = Quaternion.Euler(finalPhi, finalTheta, 0);
            transform.position = trueCenter - transform.forward * orbitD;
        }

        Vector3 stepize(Vector3 coords, float step) {
            coords.x = Mathf.Floor(coords.x / step) * step;
            coords.y = Mathf.Floor(coords.y / step) * step;
            coords.z = Mathf.Floor(coords.z / step) * step;
            return coords;
        }
    }
}