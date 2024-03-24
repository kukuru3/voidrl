using System.Collections.Generic;
using System.Linq;
using Void.ColonySim.BuildingBlocks;
using Void.ColonySim.Model;

namespace Void.ColonySim {

    public class Temperature {
        public float output;
    }

    public class TemperatureGrid : DistributionSystem<Temperature>, ISimulatedSystem {
        public override Temperature ProvideValue(ShipNode node) {
            var t = new Temperature();
            var decl = node.Structure.Declaration;
            var heatRadiated = decl.logic.GetExtension<Radiator>().radiated;
            var heatProduced = decl.logic.GetExtension<Reactor>().heat;
            t.output = heatProduced - heatRadiated;
            return t;
        }

        public void Tick() {
            
        }
    }

    public class DistroNode<T> {
        public DistroNode(DistributionGraph<T> g, string name) {
            graph = g;
            this.name = name;
        }
        internal readonly string name;
        internal readonly DistributionGraph<T> graph;

        public T Value { get; set; }
    }

    public class DistroLine<T> {
        public DistroLine(DistributionGraph<T> g, DistroNode<T> a, DistroNode<T> b) {
            graph = g;
            this.a = a;
            this.b = b;
        }

        internal readonly DistributionGraph<T> graph;
        internal readonly DistroNode<T> a;
        internal readonly DistroNode<T> b;
    }

    public class DistributionGraph<T> {

        List<DistroNode<T>> nodes = new List<DistroNode<T>>();
        public List<DistroLine<T>> lines = new List<DistroLine<T>>();

        IReadOnlyList<DistroNode<T>> Nodes => nodes;

        public DistroLine<T> GetLine(DistroNode<T> a, DistroNode<T> b, bool bidirectional = true) {
            var found = lines.FirstOrDefault(l => l.a == a && l.b == b);
            if (bidirectional) found ??= lines.FirstOrDefault(l => l.a == b && l.b == a);
            return found;
        }

        public DistroLine<T> CreateLine(DistroNode<T> a, DistroNode<T> b) {
            var existing = GetLine(a,b);
            if (existing != null) throw new System.ArgumentException("Line already exists");
            var line = new DistroLine<T>(this, a, b);
            lines.Add(line);
            return line;
        }

        public DistroNode<T> CreateNode(string name) {
            var n = new DistroNode<T>(this, name);
            nodes.Add(n);
            return n;
        }
    }


    public class DistributionSystem<T> {

        public DistributionGraph<T> graph;
        Dictionary<ShipNode, DistroNode<T>> dict = new();

        public virtual T ProvideValue(ShipNode node) => default;

        public void RegenerateGraph(Colony colony) {
            // naively generates a graph wher all nodes and tubes are taken into account.
            var graph = new DistributionGraph<T>();

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
                graph.CreateLine(a, b);
            }
            this.graph = graph;
        }
    }
}
