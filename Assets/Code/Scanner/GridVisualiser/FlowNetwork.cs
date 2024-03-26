using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanner.GridVisualiser {
    class FlowNetwork {
        internal List<FlowNode> nodes = new();
        internal List<FlowPipe> pipes = new();

        public event Action GraphUpdated;

        bool networkPropertiesChanged;
        Pathfinder pathFinder;

        public FlowNetwork() {
            pathFinder = new Pathfinder(this);
        }

        public void CreateNode(Vector2 pos) {
            var n = new FlowNode {  position = pos };
            nodes.Add(n);
            networkPropertiesChanged = true;
            GraphUpdated?.Invoke();
        }

        public void UpdateNetwork() {
            networkPropertiesChanged = true;
        }

        public bool RemoveNode(FlowNode node) {
            var result = nodes.Remove(node);
            if (result) { GraphUpdated?.Invoke(); networkPropertiesChanged = true; }
            return result;
        }

        public FlowPipe TryConnect(FlowNode from, FlowNode to) {
            if (GetPipe(from,to,true) == null) {
                var pipe = new FlowPipe(from, to);
                pipes.Add(pipe);
                GraphUpdated?.Invoke();
                networkPropertiesChanged = true;
                return pipe;
            } 
            return null;
        }

        public bool RemovePipe(FlowPipe e) {
            var result = pipes.Remove(e);
            if (result) { GraphUpdated?.Invoke(); networkPropertiesChanged = true; }
            return result;
        }

        public FlowPipe GetPipe(FlowNode a, FlowNode b, bool bidirectional = true) {
            var foundEdge = pipes.FirstOrDefault(e => e.from == a && e.to == b);
            if (bidirectional) foundEdge ??= pipes.FirstOrDefault(e => e.from == b && e.to == a);
            return foundEdge;
        }
        
        public void ResetNetwork() {
            foreach (var node in nodes) { node.calcTemp = node.productionOrConsumption; }
            foreach (var pipe in pipes) { pipe.currentFlow = 0f; }
        }

        const float DIFFUSION_FACTOR = 0.1f;
        const float TOLERANCE_THRESHOLD = 0.1f;
        const int MAX_ITERS = 10;
        
        public void UpdateNetworkDiscreteIfNeeded() {
            if (networkPropertiesChanged) {
                ResetNetwork();

                networkPropertiesChanged = false;
                pathFinder.RegenerateAdjacencyCache();

                for (var i = 0; i < MAX_ITERS; i++) {
                    var biggestDelta = 0f;

                    // diffusion:
                    foreach (var node in nodes) {
                        node.calcT0 = node.calcTemp;
                    }
                    foreach (var node in nodes) {
                        foreach (var pipe in pathFinder.GetNeighbourhood(node).connectedPipes) {
                            if (node == pipe.to) { continue; } // every pipe only once!

                            var isReverse = pipe.to == node;
                            var other = isReverse ? pipe.from : pipe.to;
                            var transferAtoB = (node.calcT0 - other.calcT0) * DIFFUSION_FACTOR;

                            var nextFlow = pipe.currentFlow + transferAtoB;

                            node.calcTemp -= transferAtoB;
                            other.calcTemp += transferAtoB;
                            pipe.currentFlow += transferAtoB;
                            biggestDelta = Mathf.Max(biggestDelta, Mathf.Abs(transferAtoB));
                        }
                    }

                    if (biggestDelta < TOLERANCE_THRESHOLD) break;
                }
            }

            var delta = 0f;
            foreach (var node in nodes) {
                delta += node.calcTemp;
            }
        }
    }

    class Path {
        // a path can be easily represented via a series of pipes.
        internal List<FlowPipe> pipes = new();
        public bool Valid => pipes.Count > 0;
    }

    class FlowNode {
        static int indexCounter;

        public override string ToString() {
            return $"Node {_index}";
        }
        // consts
        public Vector2 position;

        internal int _index = ++indexCounter;

        public float productionOrConsumption;

        public float calcT0;
        public float calcTemp;
        internal int dijkstance;
        internal object dijkparent;
    }

    class FlowPipe {
        public readonly FlowNode from;
        public readonly FlowNode to;

        public override string ToString() => $"{from._index} => {to._index}";

        public FlowPipe(FlowNode from, FlowNode to) {
            this.from = from;
            this.to = to;
        }

        public float currentFlow;

        public float capacity = float.MaxValue;
    }
}
