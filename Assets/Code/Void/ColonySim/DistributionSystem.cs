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

        public TNode Value { get; set; }
    }

    public class DistroLine<TNode, TEdge> {
        public DistroLine(DistributionGraph<TNode, TEdge> g, DistroNode<TNode, TEdge> a, DistroNode<TNode, TEdge> b) {
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
        public List<DistroLine<TNode, TEdge>> lines = new();

        public IReadOnlyList<DistroNode<TNode, TEdge>> Nodes => nodes;

        public DistroLine<TNode, TEdge> GetLine(DistroNode<TNode, TEdge> a, DistroNode<TNode, TEdge> b, bool bidirectional = true) {
            var found = lines.FirstOrDefault(l => l.a == a && l.b == b);
            if (bidirectional) found ??= lines.FirstOrDefault(l => l.a == b && l.b == a);
            return found;
        }

        public DistroLine<TNode, TEdge> CreateLine(DistroNode<TNode, TEdge> a, DistroNode<TNode, TEdge> b) {
            var existing = GetLine(a,b);
            if (existing != null) throw new System.ArgumentException("Line already exists");
            var line = new DistroLine<TNode, TEdge>(this, a, b);
            lines.Add(line);
            return line;
        }

        public DistroNode<TNode, TEdge> CreateNode(string name) {
            var n = new DistroNode<TNode, TEdge>(this, name);
            nodes.Add(n);
            return n;
        }
    }


    public class DistributionSystem<TNode, TEdge> {

        public DistributionGraph<TNode, TEdge> graph;
        Dictionary<ShipNode, DistroNode<TNode, TEdge>> dict = new();

        public virtual TNode ProvideValue(ShipNode node) => default;

        public virtual TEdge ProvideEdgeValue(Tube tube) => default;

        public void RegenerateGraph(Colony colony) {
            // naively generates a graph wher all nodes and tubes are taken into account.
            var graph = new DistributionGraph<TNode, TEdge>();

            foreach (var node in colony.ShipStructure.Nodes) {
                if (dict.ContainsKey(node)) continue;

                var name = node.Structure.name;
                if (node.Structure.Nodes.Count > 1) name += $"({node.IndexInStructure})";                
                var nn = graph.CreateNode(name);
                nn.Value = ProvideValue(node);
                dict[node] = nn;                
            }

            // tubes should be safe to clear and regenerate.
            graph.lines.Clear();

            foreach (var tube in colony.ShipStructure.Tubes) {
                
                var nodeA = tube.moduleFrom;
                var nodeB = tube.moduleTo;
                var a = dict[nodeA];
                var b = dict[nodeB];
                var line = graph.CreateLine(a, b);
                line.Value = ProvideEdgeValue(tube);
            }
            this.graph = graph;
        }
    }
}
