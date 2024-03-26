using System.Collections.Generic;
using System.Linq;

namespace Scanner.GridVisualiser {
    using Subgraph = ISet<FlowNode>;

    class Pathfinder {
        FlowNetwork network;
        bool _cacheNeedsRegeneration = true;
        public Pathfinder(FlowNetwork network)
        {
            this.network = network;
            network.GraphUpdated += () => _cacheNeedsRegeneration = true;
        }

        void EnsureCacheUpToDate() {
            if (_cacheNeedsRegeneration) {
                _cacheNeedsRegeneration = false;
                RegenerateAdjacencyCache();
            }
        }

        internal List<Subgraph> AllSubgraphs(FlowNetwork network) {
            EnsureCacheUpToDate();
            var remainingNodes = new HashSet<FlowNode>(network.nodes);

            List<Subgraph> result = new();

            while (remainingNodes.Count > 0) {
                var seed = remainingNodes.First();
                var subgraph = DijkstraFloodFill(network, new HashSet<FlowNode>{ seed });
                result.Add(subgraph);
                remainingNodes.ExceptWith(subgraph);
            }
            return result;
        }

        internal Subgraph DijkstraFloodFill(FlowNetwork network, Subgraph initialNodes) {
            EnsureCacheUpToDate();
            foreach (var node in network.nodes) {
                node.dijkstance = -1;
                node.dijkparent = null;
            }
            var closedSet = new HashSet<FlowNode>(initialNodes);
            var q = new Queue<FlowNode>(initialNodes);
            while (q.Count > 0) {
                var item = q.Dequeue();
                foreach (var neighbour in GetNeighbourhood(item).ListNeighbours()) {
                    if (closedSet.Add(neighbour)) {
                        q.Enqueue(neighbour);
                        neighbour.dijkstance = item.dijkstance + 1;
                        neighbour.dijkparent = item;
                    }
                }   
            }
            return closedSet;
        }

        internal void RegenerateAdjacencyCache() {
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
}
