using K3.Hex;
using UnityEngine;

namespace Scanner.Atomship {
    class AtomshipShipyardView : MonoBehaviour {
        [SerializeField] GameObject nodePrefab;
        [SerializeField] GameObject tubePrefab;

        [SerializeField] Transform root;


        Ship ship;

        private void Start() {
            Hardcoder.HardcodeRules();
            this.ship = Hardcoder.GenerateInitialShip();
            RegenerateShipView();
        }

        void RegenerateShipView() {
            foreach (Transform child in root) Destroy(child.gameObject);

            foreach (var node in ship.Nodes) {
                var p = node.Pose;
                var cartesianPose = p.Cartesian();
                cartesianPose.position.z *= 1.73f;
                var go = Instantiate(nodePrefab, root);
                go.transform.SetLocalPositionAndRotation(cartesianPose.position, cartesianPose.rotation);
                go.name = $"Node:{node.Structure.Declaration.ID}[{node.IndexInStructure}]";
            }

            foreach (var tube in ship.Tubes) {
                var from = tube.moduleFrom.Pose.Cartesian().position;
                var to = tube.moduleTo.Pose.Cartesian().position;
                from.z *= 1.73f;
                to.z *= 1.73f;
                var go = Instantiate(tubePrefab, root);
                go.transform.SetPositionAndRotation((from + to) / 2, Quaternion.LookRotation(to - from));
                // go.transform.localScale = new Vector3(0.1f, 0.1f, Vector3.Distance(from, to));
                go.name = $"Tube:{tube.moduleFrom}=>{tube.moduleTo}";
            }   
        }
    }
}