using System;
using System.Collections.Generic;
using K3;
using Scanner.ScannerView;
using UnityEditor.UI;
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

        [SerializeField] TubeshipView targetShip;
        [SerializeField] float damping;
        [SerializeField] Material ghostMaterial;

        [SerializeField] Selector buildablesSelector;
        [SerializeField] Selector symmetrySelector;
        [SerializeField] Buildable[] buildables;

        public int Symmetry => symmetrySelector.CyclerIndex + 1;

        GameObject[] buildPhantoms = new GameObject[0];

        TubeView lastPhantomTube;

        int selectionIndex = -1;   

        class BuildIntent {
            public Buildable buildable;
            public TubeView targetTube;
            public int a0;
            public int s0;
            public int symmetryRepeats;
            internal List<GameObject> buildPhantoms = new();
        }

        TubeView[] allTubes;
        private void Start() {
            allTubes = targetShip.GetComponentsInChildren<TubeView>();
            buildablesSelector.ClearItems();
            foreach (var bb in buildables) {
                buildablesSelector.AddItem(bb.name, bb);
            }
            buildablesSelector.CyclerIndex = 1;
            buildablesSelector.IndexChanged += SelectedNewBuildable;
        }

        private void SelectedNewBuildable(int index) {
            selectionIndex = index;
            lastPhantomTube = null;
        }

        int buildPhantomIndex;

        private void LateUpdate() {
            if (selectionIndex < 0) return;

            (var tube, var rad, var spn) = GetClosestValidTubeIntersectParams();

            if (tube == null) {
                foreach (var p in buildPhantoms) if (p != null) p.SetActive(false);
            }

            var shouldRegeneratePhantoms = lastPhantomTube != tube || buildPhantoms.Length != Symmetry || buildPhantomIndex != selectionIndex;
            
            if (shouldRegeneratePhantoms) RegenerateBuildPhantoms(selectionIndex, tube, Symmetry);

            if (tube == null) return;
            if (buildPhantoms[0] == null) return;
            
            foreach (var p in buildPhantoms) if (p != null) p.SetActive(true);

            var offSpn = (buildables[selectionIndex].gridW - 1) * 0.5f;
            var offRad = (buildables[selectionIndex].gridH - 1) * 0.5f;
            var spnZero = Mathf.RoundToInt(spn - offSpn);
            var radZero = Mathf.RoundToInt(rad - offRad);
            var spnFinal = offSpn + spnZero;
            var arcFinal = offRad + radZero;;
          
            var legalities = CheckLegality(buildables[selectionIndex], tube, spnZero, radZero, Symmetry);

            for (var i = 0; i < Symmetry; i++) { 
                var symmetryOffset = i * tube.ArcSegments / Symmetry;
                var tp = tube.GetUnrolledTubePoint(spnFinal, arcFinal + symmetryOffset, 0f);
                var posWS = tube.transform.TransformPoint(tp.pos);
                var rotWS = tube.transform.rotation * Quaternion.LookRotation(tube.transform.forward, tp.up);

                buildPhantoms[i].transform.SetPositionAndRotation(posWS, rotWS);
                var color = new Color(0.1f, 0.8f, 0f, 0.4f);
                if (legalities[i] == BuildLegality.Occupied) color = new Color(0.8f, 0f, 0f, 0.4f);
                if (legalities[i] == BuildLegality.Illegal)  color = new Color(0.4f, 0f, 0f, 0.4f);
                
                buildPhantoms[i].GetComponent<MeshRenderer>().material.SetColor("_Color", color);
            }
        }
        
        BuildLegality[] CheckLegality(Buildable b, TubeView tube, int spnZero, int arcZero, int symmetry) {
            var result = new BuildLegality[symmetry];

            var symmetryOffset = tube.ArcSegments / symmetry;

            for (var i = 0; i < symmetry; i++) {
                result[i] = BuildLegality.Legal;

                for (var s = 0; s < b.gridH; s++) {
                    for (var a = 0; a < b.gridW; a++) {
                        var tile = tube.GetTile(arcZero + a + symmetryOffset * i, spnZero + s);
                        if (tile == null) {
                            result[i] = BuildLegality.Illegal;
                        } else if (tile.occupiedBy != null) {
                            if (result[i] > BuildLegality.Illegal) result[i] = BuildLegality.Occupied; 
                            goto EXIT_LOOP;
                        }
                    }
                }

                EXIT_LOOP: 
                
                ;
            }
            return result;
        }
        public enum BuildLegality {
            Illegal,
            Occupied,
            Legal,
        }

        Quaternion _qvel = Quaternion.identity;
        Vector3 _vel = Vector3.zero;

        private void RegenerateBuildPhantoms(int buildableIndex, TubeView tube, int symmetry) {
            if (buildPhantoms != null) foreach (var bp in buildPhantoms) Destroy(bp);
            buildPhantoms = new GameObject[0];
            if (buildableIndex == -1) return;
            if (tube == null) return;
            buildPhantoms = new GameObject[symmetry];
            var b = buildables[buildableIndex];
            var p = buildables[buildableIndex].prefab;

            for (var i = 0; i < symmetry; i++) { 
                GameObject go;

                if (p == null) {
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    var mesh = ArcMesh.Solid(Mathf.PI * 2 / tube.ArcSegments * b.gridH, tube.Radius, tube.Radius - 0.05f, 0f, tube.SpinalDistance * b.gridW, 64);
                    go.GetComponent<MeshFilter>().sharedMesh = mesh;
                } else {
                    go = Instantiate(p);
                }
                go.GetComponent<MeshRenderer>().sharedMaterial = this.ghostMaterial;
                go.name = $"BUILD PHANTOM [{b.name}] : {i+1}";
                buildPhantoms[i] = go;
            }

            lastPhantomTube = tube;
            buildPhantomIndex = buildableIndex;            
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
