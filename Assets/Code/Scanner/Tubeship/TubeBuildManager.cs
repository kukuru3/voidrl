using System;
using Scanner.ScannerView;
using UnityEngine;

namespace Scanner.TubeShip.View {

    [Serializable]
    internal struct Buildable {
        [SerializeField] internal string name;
        [SerializeField] internal int gridW;
        [SerializeField] internal int gridH;
        [SerializeField] internal GameObject prefab;

    }

    internal class TubeBuildManager : MonoBehaviour {
        [SerializeField] Buildable[] buildables;

        [SerializeField] TubeshipView targetShip;
        [SerializeField] float damping;

        GameObject currentBuildPhantom;

        TubeView[] allTubes;
        private void Start() {
            allTubes = targetShip.GetComponentsInChildren<TubeView>();
        }

        private void LateUpdate() {
            if (currentBuildPhantom == null) currentBuildPhantom = CreateBuildPhantom();
            (var tube, var rad, var spn) = GetClosestValidTubeIntersectParams();
            // if (tube != null) Debug.Log($"{tube} : {rad:F2} / {spn:F2}");
            var prevActive = currentBuildPhantom.activeSelf;
            currentBuildPhantom.SetActive(tube); // this is retarded because Unity's bool overload is retarded.
            if (tube == null) return;
            
            var spnInt = Mathf.RoundToInt(spn);
            var spnRad = Mathf.RoundToInt(rad);
            var tp = tube.GetUnrolledTubePoint(spnInt, spnRad, 0f);
            var posWS = tube.transform.TransformPoint(tp.pos);
            var rotWS = tube.transform.rotation * Quaternion.LookRotation(tube.transform.forward, tp.up);

            if (prevActive) { 
                var p = currentBuildPhantom.transform.position;
                var r = currentBuildPhantom.transform.rotation;
                currentBuildPhantom.transform.SetPositionAndRotation(
                    Vector3.SmoothDamp(p, posWS, ref _vel, damping), 
                    K3.TransformUtility.SmoothDamp(r, rotWS, ref _qvel, damping)
                );
            } else { 
                currentBuildPhantom.transform.SetPositionAndRotation(posWS, rotWS);
            }
        }

        Quaternion _qvel = Quaternion.identity;
        Vector3 _vel = Vector3.zero;

        private GameObject CreateBuildPhantom() {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.localScale = Vector3.one * 0.3f;
            return go;
        }

        private (TubeView tube, float radial, float spinal) GetClosestValidTubeIntersectParams() {
            var minDist = float.MaxValue;
            TubeView bestTube = null;
            var bestRadial = 0f;
            var bestSpinal = 0f;
            foreach (var tube in allTubes) {

                var alpha = Mathf.PI / tube.ArcSegments;
                var b = Mathf.Sin(alpha) * 2 * tube.Radius;
                var d = b / Mathf.Sqrt(3);

                var (hasResult, radial, spinal, distance) = TubeUtility.RaycastTube(SceneUtil.GetScannerCamera.ScreenPointToRay(Input.mousePosition), tube);
                if (hasResult) {
                    if (distance < minDist) {
                        var spinalZed = spinal / d / tube.ZSquash / 1.5f;
                        var spinalInt = Mathf.RoundToInt(spinalZed);
                        if (spinalInt >= 0 && spinalInt < tube.SpineSegments) {
                            bestTube = tube;
                            bestRadial = radial * tube.ArcSegments;
                            bestSpinal = spinalZed;
                        }
                    }
                }
            }
            return (bestTube, bestRadial, bestSpinal);
        }

        int selectionIndex = -1;

        public void SelectBuildable(int index) {
             selectionIndex = index;
        }
    }
}
