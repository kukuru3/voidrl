﻿using System.Collections.Generic;
using System.Linq;
using K3;
using UnityEngine;

namespace Scanner.GridVisualiser {
    internal class FlowNetworkView : MonoBehaviour {
        [SerializeField] FlowNodeView nodePrefab;
        [SerializeField] FlowGridPipeView linePrefab;
        [SerializeField] Camera editorCamera;

        [SerializeField] Transform root;

        [SerializeField] TMPro.TMP_Text tooltip;

        FlowNetwork network;
        private bool _needRegenerateGraph;

        private void Start() {
            network = new FlowNetwork();
            network.GraphUpdated += HandleGraphUpdated;
            network.CreateNode(new Vector2(0,0));
            network.CreateNode(new Vector2(1,0));
            network.CreateNode(new Vector2(1,1));

            network.TryConnect(network.nodes[0], network.nodes[1]);
            network.TryConnect(network.nodes[1], network.nodes[2]);
            if (FlowNetwork.BILATERAL_PIPING) {
                network.TryConnect(network.nodes[1], network.nodes[0]);
                network.TryConnect(network.nodes[2], network.nodes[1]);
            }
        }

        private void LateUpdate() {
            if (_needRegenerateGraph) {
                RegenerateGraphView();
                _needRegenerateGraph = false;
            }

            var plane = new Plane(Vector3.up, 0);
            var ray = editorCamera.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out var enter)) {
                var pos = ray.GetPoint(enter);
                var closest = ClosestUnderCursor(pos);

                if (closest is FlowNode node) {
                    OnNodeHover(node);
                } else if (closest is FlowPipe edge) {
                    OnEdgeHover(edge);
                } else {
                    OnNoneHover((pos / FlowNodeView.SCALE).Flatten());
                }
            }

            // network.UpdateNetworkDiscreteIfNeeded();

            if (Input.GetKeyDown(KeyCode.R)) {
                network.ResetNetwork();
                network.UpdateNetwork();
                _needRegenerateGraph = true;
            }
        }

        private void FixedUpdate() {
            network.UpdateNetworkContinuous();
        }

        private void OnEdgeHover(FlowPipe pipe) {            
            tooltip.text = $"Pipe {pipe}";
            tooltip.text += $"\r\nFlow:{Mathf.Abs(pipe.currentFlow + pipe.correction):F0}\r\n Capacity:{pipe.capacity:F0} ({Mathf.Abs(pipe.currentFlow/pipe.capacity):P0})";
            // tooltip.text += $"\r\n:Flow {pipe.currentFlow:F0}/{pipe.totalCapacity:F0}   {pipe.currentFlow/pipe.totalCapacity:P0}";
            if (Input.GetKeyDown(KeyCode.Mouse1)) {
                network.RemovePipe(pipe);
                var rp = ReversePipe(pipe);
                network.RemovePipe(rp);
            }

            var delta = 0;
            if (Input.GetKeyDown(KeyCode.LeftBracket))  delta  -= 100;
            if (Input.GetKeyDown(KeyCode.RightBracket)) delta += 100;

            if (delta != 0) {
                pipe.capacity += delta;
                if (pipe.capacity < 0) pipe.capacity = 0;
                network.UpdateNetwork();
            }
        }

        FlowPipe ReversePipe(FlowPipe pipe) {
            return network.GetPipe(pipe.to, pipe.from, false);
        }

        FlowNode preselectedNode;

        private void OnNodeHover(FlowNode node) {
            tooltip.text =  $"{node}";
            tooltip.text += $"\r\nTemp: {node.calcTemp:F0}K";
            if (node.thermalGenerationPower > 0) tooltip.text += $"\r\nHeat source: {node.thermalGenerationPower:F0}";
            if (node.dissipationSurface > 5f) tooltip.text += $"\r\nDissipation surface: {node.dissipationSurface:F0}";
            //if (node.productionOrConsumption > float.Epsilon) tooltip.text += $"\r\nSource: {node.productionOrConsumption:F0}";
            //else if (node.productionOrConsumption < float.Epsilon) tooltip.text += $"\r\nSink: {node.productionOrConsumption:F0}";

            if (Input.GetKeyDown(KeyCode.E)) {
                node.dissipationSurface += 10f;
            } 
            if (Input.GetKeyDown(KeyCode.Q)) {
                node.dissipationSurface -= 10f;
                if (node.dissipationSurface < 0f) node.dissipationSurface = 0f;
            } 

            if (Input.GetKey(KeyCode.S)) {
                node.thermalGenerationPower += Time.deltaTime * 1000f;
            } 

            if (Input.GetKey(KeyCode.A)) {
                node.thermalGenerationPower -= Time.deltaTime * 1000f;
                node.thermalGenerationPower = Mathf.Max(0f, node.thermalGenerationPower);
            } 

            if (Input.GetKeyDown(KeyCode.Mouse0)) { 
                preselectedNode = node;
            }

            if (Input.GetKeyUp(KeyCode.Mouse0)) {
                if (preselectedNode != null) { 
                    network.TryConnect(preselectedNode, node);
                    if (!FlowNetwork.BILATERAL_PIPING)
                        network.TryConnect(node, preselectedNode);
                }
                preselectedNode = null;
            }

            if (Input.GetKeyDown(KeyCode.Mouse1)) {
                network.RemoveNode(node);
                foreach (var edge in network.pipes.ToList()) {
                    if (edge.from == node || edge.to == node) network.pipes.Remove(edge);
                }
            }

            var delta = 0;
            if (Input.GetKeyDown(KeyCode.LeftBracket))  delta  -= 100;
            if (Input.GetKeyDown(KeyCode.RightBracket)) delta += 100;

            if (delta !=  0) {
                node.productionOrConsumption += delta;
                network.UpdateNetwork();
            }
        }

        void OnNoneHover(Vector2 pos) {
            tooltip.text = "";
            if (Input.GetKeyDown(KeyCode.Mouse2)) {
                network.CreateNode(pos);
            }
        }

        private void HandleGraphUpdated() {
            _needRegenerateGraph = true;
        }

        private void RegenerateGraphView() {
            foreach (Transform t in root) Destroy(t.gameObject);

            foreach (var edge in network.pipes) {
                // if (edge.from.isSuperSourceOrSuperSink || edge.to.isSuperSourceOrSuperSink) continue;
                var lineView = Instantiate(linePrefab, root);
                lineView.Bind(edge);
                lineView.name = $"Pipe [{edge}]";
            }

            foreach (var node in network.nodes) {
                // if (node.isSuperSourceOrSuperSink) continue;
                var nodeView = Instantiate(nodePrefab, root);
                nodeView.Bind(node);
                nodeView.name = $"{node}";
            }
        }

        object ClosestUnderCursor(Vector3 pos) {

            object bestCandidate = null;
            float closestDist = 1f; // threshold

            foreach (Transform transform in root) {
                var nodeView = transform.gameObject.GetComponent<FlowNodeView>();
                if (nodeView != null) { 
                    var d = Vector3.Distance(nodeView.transform.position, pos);
                    if (d < closestDist ) {
                        closestDist = d;
                        bestCandidate = nodeView.Node;
                    }
                }
            }

            closestDist = Mathf.Min(closestDist, 0.12f);

            foreach (Transform transform in root) {
                
                var pipeView = transform.gameObject.GetComponent<FlowGridPipeView>();
                if (pipeView != null) {
                    var d = Geometry.DistancePointFromSegment(pos, pipeView.line.Start, pipeView.line.End);
                    if  (d < closestDist) {
                        closestDist = d;
                        bestCandidate = pipeView.Pipe;
                    }
                }
            }
            return bestCandidate;
        }
    }
}