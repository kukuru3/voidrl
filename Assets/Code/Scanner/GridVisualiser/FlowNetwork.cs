using System;
using System.Collections.Generic;
using System.Linq;
using K3;
using UnityEngine;
using UnityEngine.UIElements;
using Void.ColonySim.BuildingBlocks;

namespace Scanner.GridVisualiser {
    class FlowNetwork {
        internal List<FlowNode> nodes = new();
        internal List<FlowPipe> pipes = new();

        public event Action GraphUpdated;

        bool networkNeedsUpdated;
        Pathfinder pf = new Pathfinder();

        public void CreateNode(Vector2 pos) {
            var n = new FlowNode {  position = pos };
            nodes.Add(n);
            networkNeedsUpdated = true;
            GraphUpdated?.Invoke();
        }

        public void UpdateNetwork() {
            networkNeedsUpdated = true;
        }

        public bool RemoveNode(FlowNode node) {
            var result = nodes.Remove(node);
            if (result) { GraphUpdated?.Invoke(); networkNeedsUpdated = true; }
            return result;
        }

        public const bool BILATERAL_PIPING = true;

        public FlowPipe TryConnect(FlowNode from, FlowNode to) {
            if (GetPipe(from,to,BILATERAL_PIPING) == null) {
                var pipe = new FlowPipe(from, to);
                // pipe.totalCapacity = capacity;
                pipes.Add(pipe);
                GraphUpdated?.Invoke();
                networkNeedsUpdated = true;
                return pipe;
            } 
            return null;
        }

        public bool RemovePipe(FlowPipe e) {
            var result = pipes.Remove(e);
            if (result) { GraphUpdated?.Invoke(); networkNeedsUpdated = true; }
            return result;
        }

        public FlowPipe GetPipe(FlowNode a, FlowNode b, bool bidirectional = true) {
            var foundEdge = pipes.FirstOrDefault(e => e.from == a && e.to == b);
            if (bidirectional) foundEdge ??= pipes.FirstOrDefault(e => e.from == b && e.to == a);
            return foundEdge;
        }
        
        public void ResetNetwork() {
            foreach (var node in nodes) { node.calcTemp = node.productionOrConsumption; }
            foreach (var pipe in pipes) { pipe.currentFlow = 0f; pipe.correction = 0f; }
        }

        const float DIFFUSION_FACTOR = 0.25f;

        enum Stages {
            None,
            TieredUpdate,
            Fixup,
        }
        Stages stage;

        public void TickNetwork() {
            if (networkNeedsUpdated) {                
                ResetNetwork();
                stage = Stages.TieredUpdate;

                networkNeedsUpdated = false;
                pf.RegenerateAdjacencyCache(this);
            

                float biggestDelta = 0f;

                for (var i = 0; i < 100; i++) {
                    // diffusion:
                    foreach (var node in nodes) {
                        node.calcT0 = node.calcTemp;
                    }
                
                    foreach (var node in nodes) {
                        foreach (var pipe in pf.GetNeighbourhood(node).connectedPipes) {
                            if (node == pipe.to) { continue; } // every pipe only once!

                            var isReverse = pipe.to == node;
                            var other = isReverse ? pipe.from : pipe.to;
                            var transferAtoB = (node.calcT0 - other.calcT0) * DIFFUSION_FACTOR;

                            var nextFlow = pipe.currentFlow + transferAtoB;
                            var excess = Mathf.Abs(nextFlow) - pipe.capacity;

                            if (excess > 0) {
                                transferAtoB -= Mathf.Sign(transferAtoB) * excess;
                            }

                            node.calcTemp -= transferAtoB;
                            other.calcTemp += transferAtoB;
                            pipe.currentFlow += transferAtoB;
                            biggestDelta = Mathf.Max(biggestDelta, Mathf.Abs(transferAtoB));
                        }
                    }

                    if (biggestDelta < 0.1f) { 
                        Debug.Log($"After {i} iterations, biggest delta is {0.1f}");
                        break;
                    }
                }
            }

            // correction: reactors that are too hot do not emit.


            //foreach (var pipe in pipes) {
            //    pipe.correction = 0f;
            //    var negative = Mathf.Min(pipe.from.calcTemp, pipe.to.calcTemp); 
            //    if (negative < 0f) pipe.correction = negative;
            //}

        }
    }

    class Pathfinder {

        internal void GetDissipatingTubes(FlowNode node) {
            //if (node.calcTemp > 0f) return;
            //var pipes = GetNeighbourhood(node).connectedPipes;
            //float sumDissipation = 0f;
            //foreach (var pipe in pipes) {
            //    var dissipation = (pipe.from == node) ? pipe.currentFlow : -pipe.currentFlow;
            //    if (dissipation > 0f) {
            //        sumDissipation += dissipation;
            //    }
            //}

            //foreach (var pipe in pipes) {
            //    var dissipation = (pipe.from == node) ? pipe.currentFlow : -pipe.currentFlow;
            //    if (dissipation > 0f) {
            //        var dissipationFraction = dissipation / sumDissipation;
            //    }
            //}
        }

        internal void RegenerateAdjacencyCache(FlowNetwork network) {
            adjacency.Clear();
            foreach (var pipe in network.pipes) {
                GetOrCreateNeighbourhoodObject(pipe.from).connectedPipes.Add(pipe);
                GetOrCreateNeighbourhoodObject(pipe.to).connectedPipes.Add(pipe);
            }
        }

        internal class Neighbourhood {
            public FlowNode node;
            public List<FlowPipe> connectedPipes = new();

            public IEnumerable<FlowNode> ListNeighbours() {
                foreach (var pipe in connectedPipes) {
                    if (pipe.from == node) yield return pipe.to;
                    if (pipe.to == node) yield return pipe.from;
                }
            }
        }

        Neighbourhood GetOrCreateNeighbourhoodObject(FlowNode n) {
            if (!adjacency.TryGetValue(n, out var result)) {                
                adjacency[n] = result = new Neighbourhood() { node = n };
            };
            
            return result;            
        }

        public Neighbourhood GetNeighbourhood(FlowNode n) => adjacency[n];

        Dictionary<FlowNode, Neighbourhood> adjacency = new();
    }

    class Path {
        // a path can be easily represented via a series of pipes.
        internal List<FlowPipe> pipes = new();
        public bool Valid => pipes.Count > 0;
    }

    class FlowNode {
        static int indexCounter;

        public override string ToString() {
            //if (isSuperSourceOrSuperSink) {
            //    if (productionOrConsumption > float.Epsilon) return $"SuperSource";
            //    if (productionOrConsumption < -float.Epsilon) return $"SuperSink";
            //    return "SuperNode???";
            //}
            return $"Node {_index}";
        }
        // consts
        public Vector2 position;

        internal int _index = ++indexCounter;

        public float productionOrConsumption;

        public float calcT0;
        public float maxCapacityOfSurroundingTubes;
        public float calcTemp;


        internal void ClearPathingInfo() {
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

        //public float totalCapacity;

        public float currentFlow;

        public float correction;

        public float capacity = 300;
        //public float EffectiveFlow => Mathf.Max(0f, currentFlow);

        //public float ResidualCapacity => totalCapacity - currentFlow;

    }
}
