using System;
using K3;
using UnityEngine;

namespace Scanner {

    public interface IHasWorldFocus {
        Vector3 WorldFocus { get; }
        void Focus(Vector3 newCenter, bool immediate);
        
        event Action NewFocusSet;

    }

    public interface IOrbitCamera : IHasWorldFocus {
        float Theta { get; set; }
        float Phi { get; set; }

        void ApplyPan(Vector3 cameraspaceDelta, bool preTransformed);
        float GetOrbitDistance();
        float GetOrbitDistanceNormalized();
        void SetOrbitDistanceNormalized(float normalizedValue, bool immediate);
    }

    public interface ILaggingCamera {
        event Action<Pose> LagUpdated;
    }

    [DefaultExecutionOrder(1000)]
    public class  SteppedPerspectiveCamera : MonoBehaviour, IHasWorldFocus, IOrbitCamera, ILaggingCamera {
        [Header("Limits")]
        [SerializeField] float orbitDistanceMin;
        [SerializeField] float orbitDistanceMax;

        [SerializeField] float minPhi;
        [SerializeField] float maxPhi;

        [Header("Smoothing")]
        [SerializeField][Range(0.01f, 0.8f)] float centerSmoothTime;
        [SerializeField] [Range(0.01f, 0.8f)]float orbitDSmoothTime;

        [Header("Steps")]        
        [SerializeField][Range(0f, 0.5f)] float centerUpdateTime;
        [SerializeField][Range(0f, 10f)] float angleStep;

        public float Theta { get; set; }
        public float Phi { get; set; }

        float orbitD;

        float targetOrbitD;
        float _orbitDVel;


        Vector3 center;

        Vector3 _vel;
        Vector3 target;

        Vector3 laggedCenter;

        Vector3 IHasWorldFocus.WorldFocus => center;

        public void Focus(Vector3 newCenter, bool immediate) {
            target = newCenter;
            if (immediate) center = target;
            NewFocusSet?.Invoke();
        }

        public void SetOrbitDistanceNormalized(float normalizedValue, bool immediate) {
            var d = Mathf.Lerp(orbitDistanceMin, orbitDistanceMax, normalizedValue);
            targetOrbitD = d;
            if (immediate) orbitD = d;
        }

        public float GetOrbitDistance() => orbitD;

        public float GetOrbitDistanceNormalized() => orbitD.Map(orbitDistanceMin, orbitDistanceMax, 0f, 1f);
        
        public void ApplyPan(Vector3 cameraspaceDelta, bool preTransformed) {
            
            var worldspacePan = preTransformed ? cameraspaceDelta : transform.TransformVector(cameraspaceDelta);
            
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
        
        float centerTimeLeft;

        public event Action<Pose> LagUpdated;
        public event Action NewFocusSet;

        private void UpdatePosition() { 
            Phi = Mathf.Clamp(Phi, minPhi, maxPhi);

            var finalTheta = Theta;
            var finalPhi = Phi;

            if (angleStep > float.Epsilon) {
                finalTheta = Mathf.Floor(Theta / angleStep) * angleStep;
                finalPhi = Mathf.Floor(Phi / angleStep) * angleStep;
            }

            var targetRot = transform.rotation;
            var prevLagC = laggedCenter;
            var prevRot = transform.rotation;

            centerTimeLeft -= Time.deltaTime;
            if (centerTimeLeft <= 0f) {
                centerTimeLeft = centerUpdateTime;
                laggedCenter = center;
                transform.rotation = Quaternion.Euler(finalPhi, finalTheta, 0);
            }

            var prevPos = transform.position;
            

            transform.position = laggedCenter - transform.forward * orbitD;

            var deltaPos = laggedCenter - prevLagC;
            var deltaRot = Quaternion.Inverse(prevRot) * transform.rotation;
            var p = new Pose(deltaPos, deltaRot);
            LagUpdated?.Invoke(p);
        }

        //Vector3 stepize(Vector3 coords, float step) {
        //    coords.x = Mathf.Floor(coords.x / step) * step;
        //    coords.y = Mathf.Floor(coords.y / step) * step;
        //    coords.z = Mathf.Floor(coords.z / step) * step;
        //    return coords;
        //}
    }
}