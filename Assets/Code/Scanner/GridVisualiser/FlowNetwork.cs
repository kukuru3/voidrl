using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanner.GridVisualiser {
    class FlowNetwork {
        internal List<FlowNode> nodes = new();
        internal List<FlowPipe> pipes = new();

        public event System.Action GraphUpdated;

        FlowNode supersource;
        FlowNode supersink;

        public FlowNetwork() {

        }

        void UpdateSuperSourceAndSuperSink() {
            foreach (var pipe in pipes.ToList()) {
                if (pipe.from.isSuperSourceOrSuperSink || pipe.to.isSuperSourceOrSuperSink) {
                    pipes.Remove(pipe);
                }
            }
            nodes.RemoveAll(n => n.isSuperSourceOrSuperSink);

            supersource = new FlowNode { isSuperSourceOrSuperSink = true };
            supersink = new FlowNode { isSuperSourceOrSuperSink = true };
        }

        public void CreateNode(Vector2 pos) {
            var n = new FlowNode {  position = pos };
            nodes.Add(n);
            GraphUpdated?.Invoke();
        }

        public bool RemoveNode(FlowNode node) {
            var result = nodes.Remove(node);
            if (result) GraphUpdated?.Invoke();
            return result;
        }

        public FlowPipe TryConnect(FlowNode from, FlowNode to, float capacity = 1000) {
            if (GetPipe(from,to,false) == null) {
                var pipe = new FlowPipe(from, to);
                pipe.totalCapacity = capacity;
                pipes.Add(pipe);
                GraphUpdated?.Invoke();
                return pipe;
            } 
            return null;
        }

        public bool RemovePipe(FlowPipe e) {
            var result = pipes.Remove(e);
            if (result) GraphUpdated?.Invoke();
            return result;
        }

        public FlowPipe GetPipe(FlowNode a, FlowNode b, bool bidirectional = true) {
            var foundEdge = pipes.FirstOrDefault(e => e.from == a && e.to == b);
            if (bidirectional) foundEdge ??= pipes.FirstOrDefault(e => e.from == b && e.to == a);
            return foundEdge;
        }

        public void ResetCalculation() {
            UpdateSuperSourceAndSuperSink();
            ConnectSuperSourceAndSupersink();
            // for each source: 
        }

        private void ConnectSuperSourceAndSupersink() {
            foreach (var node in nodes) {
                if (node.productionOrConsumption > float.Epsilon) { // source
                    var pipe = TryConnect(supersource, node);
                    pipe.totalCapacity = supersource.productionOrConsumption;
                } else if (node.productionOrConsumption < -float.Epsilon) { // sink
                    var pipe = TryConnect(node, supersink);
                    pipe.totalCapacity = -supersink.productionOrConsumption;
                }
            }
        }

        Pathfinder pf = new Pathfinder();

        public void UpdateIteration() {
            // find augmenting path:
            var path = pf.FindPathDFS(this, supersource, supersink);

            if (path?.Valid ?? false) { 
                var bottleneckResidualCapacity = path.pipes.Min(p => p.ResidualCapacity);
                foreach (var pipe in path.pipes) {
                    pipe.currentFlow += bottleneckResidualCapacity;
                }
            } else {
                Debug.Log("No path found - flow saturated?");
            }
            
        }
    }

    class Pathfinder {

        public Path FindPathDFS(FlowNetwork network, FlowNode from, FlowNode to) {
            PrepareDijkstraCache(network);
            foreach (var node in network.nodes) node.ClearPathingInfo();
            return DFS(from, to, false);
        }

        Path DFS(FlowNode node, FlowNode target, bool pathFound) {
            node.visited = true;

            foreach (var pipe in neighbourhoodCache[node].outPipes) {

                if  (pipe.ResidualCapacity > float.Epsilon) {

                    if (!pipe.to.visited) {
                        pipe.to.pathing_backflowingPipe = pipe;
                        if (pipe.to == target) {
                            pathFound = true;
                            var path = new Path();
                            var n = target;
                            while (n != null) {
                                path.pipes.Add(n.pathing_backflowingPipe);
                                n = n.pathing_backflowingPipe.from;
                            }
                            return path;
                        } else {
                            return DFS(pipe.to, target, pathFound);
                        }
                    }

                }
            }
            return null;
        }

        private void PrepareDijkstraCache(FlowNetwork network) {
            neighbourhoodCache.Clear();
            foreach (var pipe in network.pipes) {
                var n = GetOrCreateNeighbourhoodObject(pipe.from);
                n.outPipes.Add(pipe);
                n.neighbours.Add(pipe.to);
            }
        }

        class Neighbourhood {
            public FlowNode node;
            public List<FlowNode> neighbours = new();
            public List<FlowPipe> outPipes = new();
        }

        Neighbourhood GetOrCreateNeighbourhoodObject(FlowNode n) {
            if (!neighbourhoodCache.TryGetValue(n, out var result)) {                
                neighbourhoodCache[n] = result = new Neighbourhood();
            };
            
            return result;            
        }

        Dictionary<FlowNode, Neighbourhood> neighbourhoodCache = new();
    }

    class Path {
        // a path can be easily represented via a series of pipes.
        internal List<FlowPipe> pipes = new();
        public bool Valid => pipes.Count > 0;
    }

    class FlowNode {
        // consts
        public Vector2 position;

        public bool isSuperSourceOrSuperSink;

        public float productionOrConsumption;


        internal FlowPipe pathing_backflowingPipe;
        internal bool visited;

        internal void ClearPathingInfo() {
            pathing_backflowingPipe = null;
            visited = false;
        }
    }

    class FlowPipe {
        public readonly FlowNode from;
        public readonly FlowNode to;

        public FlowPipe(FlowNode from, FlowNode to) {
            this.from = from;
            this.to = to;
        }

        public float totalCapacity;

        public float currentFlow;

        public float ResidualCapacity => totalCapacity - currentFlow;

    }
}
