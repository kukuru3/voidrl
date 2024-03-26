using System;
using K3;
using Shapes;
using TMPro;
using UnityEngine;
using Void;
using Void.ColonySim;

namespace Scanner.Atomship {
    class LifeSupportInspector : MonoBehaviour {
        [SerializeField] ShipbuilderView view;
        [SerializeField] TMP_Text tooltip;

        [SerializeField] GameObject providerPrefab;
        [SerializeField] GameObject consumerPrefab;
        [SerializeField] GameObject pipePrefab;

        [SerializeField] GameObject root;

        private void Start() {
            
        }

        private void Update() {
            var h = view.lastHitHex;
            
            var node = Game.Colony.ShipStructure.GetNode(h);
            if (node == null) return;
            
            var lifeSupportGrid = Game.Colony.GetSystem<LifeSupportGrid>();

            var lsNode = lifeSupportGrid.GetShipNode(node);

            if (lsNode == null) {
                tooltip.text = "Does not participate in life support";
            } else if (lsNode.Value is LifeSupportProvider provider) {
                tooltip.text = $"PROVIDER; demand = {provider.SumDemands()} / {provider.ls.totalCapacity}";
            } else if (lsNode.Value is LifeSupportConsumer consumer) {
                var pct = 100 * consumer.totalReceived / consumer.drawRequirements;
                if (consumer.drawRequirements == 0) pct = 0;
                tooltip.text = $"CONSUMER; received: {consumer.totalReceived} ({pct}% satisfied)";
            }

            if (Input.GetKeyDown(KeyCode.L)) {
                lifeSupportGrid.Tick();
                RegenerateGridView();
            }
        }

        private void RegenerateGridView() {
            foreach (Transform t in root.transform) GameObject.Destroy(t.gameObject);
            var lifeSupportGrid = Game.Colony.GetSystem<LifeSupportGrid>();
            foreach (var node in lifeSupportGrid.graph.Nodes) {
                if (node.Value is LifeSupportProvider provider && providerPrefab != null) {
                    var providerGO = Instantiate(providerPrefab, root.transform);
                    providerGO.transform.AssumePose(node.ShipNode.Pose.CartesianPose());
                    providerGO.name = "Provider";
                } else if (node.Value is LifeSupportConsumer consumer && consumerPrefab != null) {
                    var consumerGO = Instantiate(consumerPrefab, root.transform);
                    consumerGO.transform.AssumePose(node.ShipNode.Pose.CartesianPose());
                    consumerGO.name = "Consumer";
                }
            }

            foreach (var pipe in lifeSupportGrid.graph.pipes) {
                if (pipePrefab != null) {
                    var pipeGO = Instantiate(pipePrefab, root.transform);
                    pipeGO.name = "Pipe";

                    var centerA = pipe.a.ShipNode.Pose.CartesianPose().position;
                    var centerB = pipe.b.ShipNode.Pose.CartesianPose().position;
                    var l = pipeGO.GetComponent<Line>();
                    if (l != null) {
                        l.Start = centerA; l.End = centerB;
                        var c = l.Color;

                        var v = ((float)pipe.Value.conducted).Map(0f, 50f, 0.01f, 0.4f);

                        c.a = v;
                        l.Color = c;
                    }
                }
            }
        }
    }
}