using K3;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Scanner.ModularShip {
    internal class ShipbuilderCamera : MonoBehaviour {
        [SerializeField] Transform camParent;
        [SerializeField] Transform camTransform;

        [SerializeField] Vector3 deltas;

        [SerializeField] float panDelta;

        [SerializeField] float distMin;
        [SerializeField] float distMax;

        [SerializeField] PostProcessVolume targetVolume;

        public float CamDistanceFactor { get; set; }

        float effectiveCamDist;
        float _cdVel;

        public float Theta { get; set; }
        public float Phi { get; set; }


        public float AxisPan { get; set; }

        private void LateUpdate() {

            var mouseAxis = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            if (Input.GetMouseButton(1)) { 
                var dTheta = mouseAxis.x * deltas.x;
                var dPhi   = mouseAxis.y * deltas.y;

                Theta += dTheta;
                Phi += dPhi;
            }

            // alright bois and gurls, here is our panning algo:
            // project screenspace delta to worldspace
            if (Input.GetMouseButton(2)) {
                var worldspaceMotion = camParent.TransformVector(new Vector3(mouseAxis.x, mouseAxis.y, 0));
                AxisPan += worldspaceMotion.z * panDelta * effectiveCamDist.Map(0f, 1f, distMin, distMax);
            }

            var dz = Input.GetAxis("Mouse ScrollWheel") * deltas.z;
            CamDistanceFactor += dz;
            
            CamDistanceFactor = Mathf.Clamp01(CamDistanceFactor);
            effectiveCamDist = Mathf.SmoothDamp(effectiveCamDist, CamDistanceFactor, ref _cdVel, 0.2f);
            camParent.localPosition = new Vector3(0, 0, AxisPan);

            Phi = Mathf.Clamp(Phi, -45, 80);
            //Theta = Mathf.Clamp(Theta, -35, 125);
            
            camParent.localRotation = Quaternion.Euler(Phi, Theta, 0);
            var dist = effectiveCamDist.Map(0f, 1f, distMin, distMax);
            camTransform.localPosition = new Vector3(0, 0, -dist);

            var distParam = targetVolume.profile.GetSetting<DepthOfField>().focusDistance;
            
            distParam.Override(dist);
        }
    }
}
