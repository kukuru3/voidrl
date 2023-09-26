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
        TubeView lastPhantomTube;

        int selectionIndex = -1;        

        TubeView[] allTubes;
        private void Start() {
            allTubes = targetShip.GetComponentsInChildren<TubeView>();
        }

        private void LateUpdate() {
            if (Input.GetKeyDown(KeyCode.Tab)) {
                selectionIndex++; if (selectionIndex >= buildables.Length) selectionIndex = 0;
                lastPhantomTube = null;
            }

            (var tube, var rad, var spn) = GetClosestValidTubeIntersectParams();
            var old = currentBuildPhantom;
            if (tube != null) AdaptBuildPhantomToTube(tube);
            var didRegen = currentBuildPhantom != null && currentBuildPhantom != old;
            if (tube == null) return;

            if (currentBuildPhantom == null) return;

            // if (tube != null) Debug.Log($"{tube} : {rad:F2} / {spn:F2}");
            var prevActive = currentBuildPhantom.activeSelf;
            currentBuildPhantom.SetActive(tube); // this is retarded because Unity's bool overload is retarded.
            
            var offSpn = (buildables[selectionIndex].gridH - 1) * 0.5f;
            var offRad = (buildables[selectionIndex].gridW - 1) * 0.5f;
            
            var spnFinal = Mathf.RoundToInt(spn + offSpn) - offSpn;
            var radFinal = Mathf.RoundToInt(rad + offRad) - offRad;
            var tp = tube.GetUnrolledTubePoint(spnFinal, radFinal, 0f);
            var posWS = tube.transform.TransformPoint(tp.pos);
            var rotWS = tube.transform.rotation * Quaternion.LookRotation(tube.transform.forward, tp.up);

            if (prevActive && !didRegen) { 
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

        private void AdaptBuildPhantomToTube(TubeView tube) {
            if (lastPhantomTube == tube) return;
            if (selectionIndex >= 0 && buildables[selectionIndex].prefab == null) {
                if (currentBuildPhantom != null) { 
                    Destroy(currentBuildPhantom);
                    currentBuildPhantom = null;                
                }
                currentBuildPhantom = RegenerateBuildPhantom(selectionIndex, tube);
            }
            lastPhantomTube = tube;
        }

        Quaternion _qvel = Quaternion.identity;
        Vector3 _vel = Vector3.zero;

        private GameObject RegenerateBuildPhantom(int index, TubeView tube) {
            var b = buildables[index];
            var p = buildables[index].prefab;
            GameObject go;

            if (p == null) {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var mesh = ArcMesh.Solid(Mathf.PI * 2 / tube.ArcSegments * b.gridH, tube.Radius, tube.Radius - 0.05f, 0f, tube.SpinalDistance * b.gridW, 64);
                go.GetComponent<MeshFilter>().sharedMesh = mesh;
            } else {
                go = Instantiate(p);
            }
            go.name = $"BUILD PHANTOM [{b.name}]";
            return go;
        }

        private (TubeView tube, float radial, float spinal) GetClosestValidTubeIntersectParams() {
            var minDist = float.MaxValue;
            TubeView bestTube = null;
            var bestRadial = 0f;
            var bestSpinal = 0f;
            foreach (var tube in allTubes) {
                (var angle, var side, var distFromCenter, var circumDist) = tube.GetTubeDimensionParams();
                
                var (hasResult, radial, spinal, distance) = TubeUtility.RaycastTube(SceneUtil.GetScannerCamera.ScreenPointToRay(Input.mousePosition), tube);
                if (hasResult) {
                    if (distance < minDist) {
                        // var worldspaceToSpinal = spinal / distFromCenter / tube.SpinalDistanceMultiplier / 1.5f;
                        var worldspaceToSpinal = spinal / tube.SpinalDistance;
                        var spinalInt = Mathf.RoundToInt(worldspaceToSpinal);
                        if (spinalInt >= 0 && spinalInt < tube.SpineSegments) {
                            bestTube = tube;
                            bestRadial = radial * tube.ArcSegments;
                            bestSpinal = worldspaceToSpinal;
                        }
                    }
                }
            }
            return (bestTube, bestRadial, bestSpinal);
        }

        public void SelectBuildable(int index) {
             selectionIndex = index;
        }
    }
}
