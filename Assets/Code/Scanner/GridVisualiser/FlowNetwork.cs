using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Scanner.GridVisualiser {
    class FlowNetwork {
        internal List<FlowNode> nodes = new();
        internal List<FlowPipe> pipes = new();

        public event Action GraphUpdated;

        bool networkPropertiesChanged;
        Pathfinder pf = new Pathfinder();

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

        public const bool BILATERAL_PIPING = true;

        public FlowPipe TryConnect(FlowNode from, FlowNode to) {
            if (GetPipe(from,to,BILATERAL_PIPING) == null) {
                var pipe = new FlowPipe(from, to);
                // pipe.totalCapacity = capacity;
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
            foreach (var pipe in pipes) { pipe.currentFlow = 0f; pipe.correction = 0f; }
        }

        const float DIFFUSION_FACTOR = 0.25f;

        const float ENERGY_TRANSFERRENCE = 0.001f;

        const float STEFFAN_BOlTZMANN = 5.670373e-8f;

        public void UpdateNetworkContinuous() {
            if (networkPropertiesChanged) {
                pf.RegenerateAdjacencyCache(this);
                

                foreach (var node in nodes) {
                    var generated = node.thermalGenerationPower;
                    var temperature = node.calcThermalEnergy / node.thermalCapacity;
                    var dissipation = Mathf.Pow(temperature, 4f) * STEFFAN_BOlTZMANN * node.dissipationSurface;
                    if (generated > 0f) {
                        Debug.Log($"[{node._index}]: Generated: {generated}, dissipated {dissipation}");
                    }
                    
                    node.calcThermalEnergy += (generated - dissipation) / 50;
                    if (node.calcThermalEnergy < 0f) node.calcThermalEnergy = 0f;
                }

                foreach (var node in nodes) { 
                    node.calcThermalEnergy0 = node.calcThermalEnergy;
                    node.calcT0 = node.calcThermalEnergy0 / node.thermalCapacity;
                }
                
                // diffusion step:
                foreach (var node in nodes) {
                    foreach (var pipe in pf.GetNeighbourhood(node).connectedPipes) {
                        if (node == pipe.to) { continue; } // every pipe only once!

                        var isReverse = pipe.to == node;
                        var other = isReverse ? pipe.from : pipe.to;

                        var temperatureDifferential = (node.calcT0 - other.calcT0);
                        if (temperatureDifferential > 0f) {
                            var transferredEnergy = temperatureDifferential * node.thermalCapacity * ENERGY_TRANSFERRENCE;
                            node.calcThermalEnergy -= transferredEnergy;
                            other.calcThermalEnergy += transferredEnergy;
                        } else {
                            var transferredEnergy = temperatureDifferential * other.thermalCapacity * ENERGY_TRANSFERRENCE;
                            node.calcThermalEnergy += transferredEnergy;
                            other.calcThermalEnergy -= transferredEnergy;
                        }
                    }
                }

                foreach (var node in nodes) node.calcTemp = node.calcThermalEnergy / node.thermalCapacity;
            }
        }

        public void UpdateNetworkDiscreteIfNeeded() {
            if (networkPropertiesChanged) {
                ResetNetwork();

                networkPropertiesChanged = false;
                pf.RegenerateAdjacencyCache(this);

                for (var i = 0; i < 100; i++) {
                    var biggestDelta = 0f;

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

            var delta = 0f;
            foreach (var node in nodes) {
                delta += node.calcTemp;
            }
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
            return $"Node {_index}";
        }
        // consts
        public Vector2 position;

        internal int _index = ++indexCounter;

        public float productionOrConsumption;

        public float calcT0;
        public float calcTemp;

        public float calcThermalEnergy0;
        public float calcThermalEnergy;

        public float thermalGenerationPower = 0f;
        public float thermalCapacity = 100f;
        public float dissipationSurface = 2f;

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
