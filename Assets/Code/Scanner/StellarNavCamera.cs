using System;
using K3;
using UnityEngine;

namespace Scanner {

    public interface IHasWorldFocus {
        Vector3 WorldFocus { get; }
    }

    public class StellarNavCamera : MonoBehaviour, IHasWorldFocus {
        [SerializeField] float orbitDistanceMin;
        [SerializeField] float orbitDistanceMax;

        [SerializeField] float minPhi;
        [SerializeField] float maxPhi;

        [SerializeField] float panMultiplier;
        [SerializeField] float mousePhiMultiplier;
        [SerializeField] float mouseThetaMultiplier;
        [SerializeField] float mouseWheelZoomMult;

        [SerializeField] float constRotation;

        [Header("Smoothing")]

        float theta;
        float phi;
        float orbitD;

        float targetOrbitD;
        float _orbitDVel;
        [SerializeField][Range(0.01f, 0.8f)] float centerSmoothTime;
        [SerializeField] [Range(0.01f, 0.8f)]float orbitDSmoothTime;

        Vector3 center;

        Vector3 _vel;
        Vector3 targetVel;

        Vector3 IHasWorldFocus.WorldFocus => center;

        internal void Focus(Vector3 newCenter) {
            targetVel = newCenter;
        }

        internal void OverrideOrbitDistance(float normalizedValue) {
            var d = Mathf.Lerp(orbitDistanceMin, orbitDistanceMax, normalizedValue);
            orbitD = targetOrbitD = d;
        }

        internal float GetOrbitDistanceNormalized() => orbitD.Map(orbitDistanceMin, orbitDistanceMax, 0f, 1f);

        private void LateUpdate() {
            var mouseDelta = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);

            if (Input.GetMouseButton(1)) {
                theta += mouseDelta.x * mouseThetaMultiplier;
                phi += mouseDelta.y * mousePhiMultiplier;
            }

            Vector3 delta = default;
            if (Input.GetMouseButton(2)) {
                delta.x = mouseDelta.x * panMultiplier;
                delta.y = mouseDelta.y * panMultiplier;
            }

            theta += Time.deltaTime * constRotation;

            targetOrbitD += Input.mouseScrollDelta.y * mouseWheelZoomMult;
             targetOrbitD = Mathf.Clamp(targetOrbitD, orbitDistanceMin, orbitDistanceMax);

            var worldspacePan = transform.TransformVector(delta);

            center += worldspacePan;
            targetVel += worldspacePan;

            UpdateInterpolators();

            UpdatePosition();
        }

        private void UpdateInterpolators() {
            center = Vector3.SmoothDamp(center, targetVel, ref _vel, centerSmoothTime);
            orbitD = Mathf.SmoothDamp(orbitD, targetOrbitD, ref _orbitDVel, orbitDSmoothTime);
        }

        private void UpdatePosition() { 

           
            phi = Mathf.Clamp(phi, minPhi, maxPhi);
            transform.rotation = Quaternion.Euler(phi, theta, 0);
            transform.position = center - transform.forward * orbitD;
        }
    }
}