using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanner.GridVisualiser {
    class FlowNetwork {
        internal List<FlowNode> nodes = new();
        internal List<FlowPipe> pipes = new();

        public event Action GraphUpdated;

        FlowNode supersource;
        FlowNode supersink;

        bool needsRecalc;
        Pathfinder pf = new Pathfinder();

        void UpdateSuperSourceAndSuperSink() {
            foreach (var pipe in pipes.ToList()) {
                if (pipe.from.isSuperSourceOrSuperSink || pipe.to.isSuperSourceOrSuperSink) {
                    pipes.Remove(pipe);
                }
            }
            nodes.RemoveAll(n => n.isSuperSourceOrSuperSink);

            supersource = new FlowNode { isSuperSourceOrSuperSink = true };
            supersink = new FlowNode { isSuperSourceOrSuperSink = true };

            nodes.Add(supersource);
            nodes.Add(supersink);

            foreach (var node in nodes) {
                if (node.productionOrConsumption > float.Epsilon) { // source
                    var pipe = TryConnect(supersource, node);
                    pipe.totalCapacity = node.productionOrConsumption;
                } else if (node.productionOrConsumption < -float.Epsilon) { // sink
                    var pipe = TryConnect(node, supersink);
                    pipe.totalCapacity = -node.productionOrConsumption;
                }
            }

            supersource.productionOrConsumption = Mathf.Infinity;
            supersink.productionOrConsumption = Mathf.NegativeInfinity;
        }

        public void CreateNode(Vector2 pos) {
            var n = new FlowNode {  position = pos };
            nodes.Add(n);
            needsRecalc = true;
            GraphUpdated?.Invoke();
        }

        public void Reset() {
            needsRecalc = true;
        }

        public bool RemoveNode(FlowNode node) {
            var result = nodes.Remove(node);
            if (result) { GraphUpdated?.Invoke(); needsRecalc = true; }
            return result;
        }

        public FlowPipe TryConnect(FlowNode from, FlowNode to, float capacity = 1000) {
            if (GetPipe(from,to,false) == null) {
                var pipe = new FlowPipe(from, to);
                pipe.totalCapacity = capacity;
                pipes.Add(pipe);
                GraphUpdated?.Invoke();
                needsRecalc = true;
                return pipe;
            } 
            return null;
        }

        public bool RemovePipe(FlowPipe e) {
            var result = pipes.Remove(e);
            if (result) { GraphUpdated?.Invoke(); needsRecalc = true; }
            return result;
        }

        public FlowPipe GetPipe(FlowNode a, FlowNode b, bool bidirectional = true) {
            var foundEdge = pipes.FirstOrDefault(e => e.from == a && e.to == b);
            if (bidirectional) foundEdge ??= pipes.FirstOrDefault(e => e.from == b && e.to == a);
            return foundEdge;
        }

        public void NextIteration() {

            if (needsRecalc) { 
                foreach (var pipe in pipes) { pipe.currentFlow = 0; }
                UpdateSuperSourceAndSuperSink();
                needsRecalc = false;
            }

            foreach (var item in nodes) { item.ClearPathingInfo(); }

            for (var i = 0 ; i < 20; i++) { 
                // find augmenting path:
                // var path = pf.FindPathDFS(this, supersource, supersink);
                var path = pf.BreadthFirstSearch(this, supersource, supersink);
                
                if (path?.Valid ?? false) {
                    var bottleneckResidualCapacity = path.pipes.Min(p => p.ResidualCapacity);
                    foreach (var pipe in path.pipes) {
                        pipe.currentFlow += bottleneckResidualCapacity;
                        var reversePipe = GetPipe(pipe.to, pipe.from, false);
                        if (reversePipe != null)
                            reversePipe.currentFlow -= bottleneckResidualCapacity;
                    }
                } else {
                    break;
                }
            }
        }
    }

    class Pathfinder {

        public Path BreadthFirstSearch(FlowNetwork network, FlowNode from, FlowNode to) {
            neighbourhoodCache.Clear();
            PrepareDijkstraCache(network);
            foreach (var node in network.nodes) node.ClearPathingInfo();

            var queue = new Queue<FlowNode>();
            queue.Enqueue(from);

            while (queue.Count > 0) {
                var node = queue.Dequeue();
                node.closedList = true;
                foreach (var pipe in neighbourhoodCache[node].outPipes) {
                    if (pipe.ResidualCapacity > float.Epsilon) {
                        if (!pipe.to.closedList) {
                            pipe.to.pathing_backflowingPipe = pipe;
                            if (pipe.to == to) {
                                var path = new Path();
                                var n = to;
                                while (n.pathing_backflowingPipe != null) {
                                    path.pipes.Add(n.pathing_backflowingPipe);
                                    n = n.pathing_backflowingPipe.from;
                                }
                                return path;
                            } else {
                                queue.Enqueue(pipe.to);
                            }
                        }
                    }
                }
            }
            return null;
        }   

        public Path FindPathDFS(FlowNetwork network, FlowNode from, FlowNode to) {
            neighbourhoodCache.Clear();
            PrepareDijkstraCache(network);
            foreach (var node in network.nodes) node.ClearPathingInfo();
            return DFS(from, to, false);
        }

        Path DFS(FlowNode node, FlowNode target, bool pathFound) {
            if (node == null) {
                Debug.Log("lolwut");
            }
            node.closedList = true;

            foreach (var pipe in neighbourhoodCache[node].outPipes) {

                if  (pipe.ResidualCapacity > float.Epsilon) {

                    if (!pipe.to.closedList) {
                        pipe.to.pathing_backflowingPipe = pipe;
                        if (pipe.to == target) {
                            pathFound = true;
                            var path = new Path();
                            var n = target;
                            while (n.pathing_backflowingPipe != null) {
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
        static int indexCounter;

        public override string ToString() {
            if (isSuperSourceOrSuperSink) {
                if (productionOrConsumption > float.Epsilon) return $"SuperSource";
                if (productionOrConsumption < -float.Epsilon) return $"SuperSink";
                return "SuperNode???";
            }
            return $"Node {_index}";
        }
        // consts
        public Vector2 position;

        internal int _index = ++indexCounter;

        public bool isSuperSourceOrSuperSink;

        public float productionOrConsumption;

        internal FlowPipe pathing_backflowingPipe;
        internal bool closedList;

        internal void ClearPathingInfo() {
            pathing_backflowingPipe = null;
            closedList = false;
        }
    }

    class FlowPipe {
        public readonly FlowNode from;
        public readonly FlowNode to;

        public override string ToString() => $"{from._index} => {to._index}";

        public FlowPipe(FlowNode from, FlowNode to) {
            this.from = from;
            this.to = to;
        }

        public float totalCapacity;

        public float currentFlow;

        public float EffectiveFlow => Mathf.Max(0f, currentFlow);

        public float ResidualCapacity => totalCapacity - currentFlow;

    }
}
