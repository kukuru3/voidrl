using System;
using System.Linq;
using Core;
using Core.h3x;
using K3.Hex;
using UnityEngine;

namespace Scanner.Atomship {
    class AtomshipShipyardView : MonoBehaviour {
        [SerializeField] GameObject nodePrefab;
        [SerializeField] GameObject tubePrefab;

        [SerializeField] GameObject buttonPrefab;
        [SerializeField] Transform uiRoot;

        [SerializeField] Transform root;


        Ship ship;

        private void Start() {
            Hardcoder.HardcodeRules();
            this.ship = Hardcoder.GenerateInitialShip();
            GenerateUI();
            RegenerateShipView();
            ComputeAttachmentData();
        }

        private void ComputeAttachmentData() { 
            foreach (var @struct in ship.ListStructures()) {
                var pose0 = @struct.Pose;
                var connectors = @struct.Declaration.nodeModel.features.Where(f => f.type == FeatureTypes.Connector).ToList();
                foreach (var conn in connectors) {

                    // a connector does not have a local hex rotation, but it does have a direction
                    // and we can infer its "rotation" from that.
                    var offZ = conn.localDirection.Offset().zed;
                    var connPose = new HexPose(conn.localCoords, conn.localDirection.ToHexRotation());
                    var finalPose = pose0 * connPose;

                    // the final worldspace hextile of the connector, and its final worldspace direction

                    var finalWorldspaceDirection = Hex3Utils.FromParameters(finalPose.rotation, offZ);
                };
            }
        }

        private void GenerateUI() { 
            var sdecls = RuleContext.Repo.ListRules<StructureDeclaration>();
            var y = 0;
            foreach (var sd in sdecls) {
                var button = Instantiate(buttonPrefab, uiRoot);
                var bc = button.GetComponent<Button>();
                bc.Clicked += () => SelectDeclaration(sd);
                bc.Label = sd.ID;
                bc.transform.localPosition = new Vector3(0, y, 0);
                y+=30;
            }
        }

        StructureDeclaration currentTool;

        private void SelectDeclaration(StructureDeclaration sd) {
            currentTool = sd;
        }

        void RegenerateShipView() {
            foreach (Transform child in root) Destroy(child.gameObject);

            foreach (var node in ship.Nodes) {
                var p = node.Pose;
                var cartesianPose = p.Cartesian();
                cartesianPose.position.z *= 1.73f;
                var go = Instantiate(nodePrefab, root);
                go.transform.SetLocalPositionAndRotation(cartesianPose.position, cartesianPose.rotation);
                go.GetComponent<NodeView>().Node = node;
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

        private void Update() {
            var mousePos = Input.mousePosition;
            var ray = Camera.main.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out var hit, 200f, 1 << 20, QueryTriggerInteraction.Collide)) {
                var go = hit.collider.gameObject;
                var nv = go.GetComponentInParent<NodeView>();
                var node = nv.Node;
                var worldspaceDirection = Hex3Utils.ComputeDirectionFromNormal(hit.normal);

                // "node" and "dir" are sufficient for us

                TryFitStructure(currentTool, node.Pose, worldspaceDirection);
            }
        }

        private void TryFitStructure(StructureDeclaration currentTool, HexPose pose, Hex3Dir dir) { 
            var primaryConnectors = currentTool.nodeModel.features.Where(f => f.type == FeatureTypes.Connector && f.connType == ConnectionTypes.Primary);
        }
    }
}