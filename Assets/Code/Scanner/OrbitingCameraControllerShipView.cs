using UnityEngine;

namespace Scanner {
    public class OrbitingCameraControllerShipView : OrbitingCameraController {
        [SerializeField] bool  limitPhi;
        [SerializeField] float minPhi;
        [SerializeField] float maxPhi;

        [SerializeField] bool limitTheta;
        [SerializeField] float minTheta;
        [SerializeField] float maxTheta;

        [SerializeField] float orbitDistanceWheelFactor;

        protected override void Update() {
            base.Update();
            if (limitPhi)
                targetCam.Phi = Mathf.Clamp(targetCam.Phi, minPhi, maxPhi);
            if (limitTheta)
                targetCam.Theta = Mathf.Clamp(targetCam.Theta, minTheta, maxTheta);
            var d = targetCam.GetOrbitDistanceNormalized();
            d += Input.mouseScrollDelta.y * orbitDistanceWheelFactor;
            targetCam.SetOrbitDistanceNormalized(d, false);
        }
    }
}