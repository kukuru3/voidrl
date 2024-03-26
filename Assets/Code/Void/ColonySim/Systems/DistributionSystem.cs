using System.Collections.Generic;
using System.Linq;
using Void.ColonySim.BuildingBlocks;

namespace Void.ColonySim {
    public class DistroNode<TNode, TEdge> {
        public DistroNode(DistributionGraph<TNode, TEdge> g, string name) {
            graph = g;
            this.name = name;
        }
        internal readonly string name;
        internal readonly DistributionGraph<TNode, TEdge> graph;

        internal List<DistroPipe<TNode, TEdge>> connectedPipes = new();
        internal object dijkstraparent;
        internal int dijkstance;

        public TNode Value { get; set; }
    }

    public class DistroPipe<TNode, TEdge> {
        public DistroPipe(DistributionGraph<TNode, TEdge> g, DistroNode<TNode, TEdge> a, DistroNode<TNode, TEdge> b) {
            graph = g;
            this.a = a;
            this.b = b;
        }

        internal readonly DistributionGraph<TNode, TEdge> graph;
        internal readonly DistroNode<TNode, TEdge> a;
        internal readonly DistroNode<TNode, TEdge> b;

        public TEdge Value { get; set; }
    }

    public class DistributionGraph<TNode, TEdge> {

        List<DistroNode<TNode, TEdge>> nodes = new();
        public List<DistroPipe<TNode, TEdge>> pipes = new();

        internal Pathfinder<TNode, TEdge> pathfinder;

        public DistributionGraph() {
            pathfinder = new Pathfinder<TNode, TEdge>(this);
        }

        public IReadOnlyList<DistroNode<TNode, TEdge>> Nodes => nodes;

        public DistroPipe<TNode, TEdge> GetLine(DistroNode<TNode, TEdge> a, DistroNode<TNode, TEdge> b, bool bidirectional = true) {
            var found = pipes.FirstOrDefault(l => l.a == a && l.b == b);
            if (bidirectional) found ??= pipes.FirstOrDefault(l => l.a == b && l.b == a);
            return found;
        }

        public DistroPipe<TNode, TEdge> CreatePipe(DistroNode<TNode, TEdge> a, DistroNode<TNode, TEdge> b) {
            var existing = GetLine(a,b);
            if (existing != null) throw new System.ArgumentException("Line already exists");
            var line = new DistroPipe<TNode, TEdge>(this, a, b);
            pipes.Add(line);
            return line;
        }

        public DistroNode<TNode, TEdge> CreateNode(string name) {
            var n = new DistroNode<TNode, TEdge>(this, name);
            nodes.Add(n);
            return n;
        }

        public class Path {
            internal List<DistroPipe<TNode, TEdge>> pipes;
        }
    }

    public interface IGraphNodeAware<TNode, TEdge> {
        public DistroNode<TNode, TEdge> GraphNode { get; set; }
    }

    public interface IGraphPipeAware<TNode, TEdge> {
        public DistroPipe<TNode, TEdge> GraphPipe { get; set; }
    }

    public class Pathfinder<TNode, TEdge> {
        private readonly DistributionGraph<TNode, TEdge> graph;

        public Pathfinder(DistributionGraph<TNode, TEdge> graph)
        {
            this.graph = graph;
            Prepare();
        }

        private void Prepare() {
            foreach (var item in graph.Nodes) item.connectedPipes.Clear();
            foreach (var pipe in graph.pipes) {
                pipe.a.connectedPipes.Add(pipe);
                pipe.b.connectedPipes.Add(pipe);
            }
        }

        public delegate bool PathFilter(DistroNode<TNode, TEdge> node);

        public ISet<DistroNode<TNode, TEdge>> DijkstraFloodFill(DistroNode<TNode, TEdge> seed, PathFilter filter = null, int maxRange = int.MaxValue) {
            Prepare();
            foreach (var node in graph.Nodes) {
                node.dijkstraparent = null;
                node.dijkstance = -1;
            }

            var closedSet = new HashSet<DistroNode<TNode, TEdge>> { seed };
            var q = new Queue<DistroNode<TNode, TEdge>>();

            while (q.Count > 0) {
                var item = q.Dequeue();
                foreach (var pipe in item.connectedPipes) {
                    var neighbour = pipe.a == item ? pipe.b : pipe.a;
                    if (filter != null && !filter(neighbour)) continue;

                    if (closedSet.Add(neighbour)) {
                        neighbour.dijkstance = item.dijkstance + 1;
                        neighbour.dijkstraparent = item;
                        if (neighbour.dijkstance <= maxRange) q.Enqueue(neighbour);
                    }
                }   
            }
            return closedSet;
        }
    }

    
    public class DistributionSystem<TNode, TEdge> {

        public DistributionGraph<TNode, TEdge> graph;
        Dictionary<ShipNode, DistroNode<TNode, TEdge>> dict = new();

        public virtual TNode ProvideValue(ShipNode node) => default;

        public virtual TEdge ProvideEdgeValue(Tube tube) => default;

        public virtual bool HasNode(ShipNode node) => true;
        public virtual bool HasConduit(Tube tube) => true;

        public void RegenerateGraph(Colony colony) {
            // naively generates a graph wher all nodes and tubes are taken into account.
            var graph = new DistributionGraph<TNode, TEdge>();

            foreach (var node in colony.ShipStructure.Nodes) {
                if (!HasNode(node)) continue;
                if (dict.ContainsKey(node)) continue;

                var name = node.Structure.name;
                if (node.Structure.Nodes.Count > 1) name += $"({node.IndexInStructure})";                
                var nn = graph.CreateNode(name);
                nn.Value = ProvideValue(node);

                if (nn.Value is IGraphNodeAware<TNode, TEdge> nodeAware) {
                    nodeAware.GraphNode = nn;
                }

                dict[node] = nn;                
            }

            // tubes should be safe to clear and regenerate.
            graph.pipes.Clear();

            foreach (var tube in colony.ShipStructure.Tubes) {
                if (!HasConduit(tube)) continue;
                var nodeA = tube.moduleFrom;
                var nodeB = tube.moduleTo;
                var a = dict[nodeA];
                var b = dict[nodeB];
                var line = graph.CreatePipe(a, b);
                line.Value = ProvideEdgeValue(tube);

                if (line.Value is IGraphPipeAware<TNode, TEdge> pipeAware) {
                    pipeAware.GraphPipe = line;
                }
            }
            this.graph = graph;
        }
    }
}
