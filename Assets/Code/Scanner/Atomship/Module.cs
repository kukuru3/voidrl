using System.Collections.Generic;
using System.Linq;
using Core.h3x;

namespace Scanner.Atomship {
    // a MODULE is a single hexnode. 
    public class Node : IHasHex3Coords {
        public Hex3 HexCoords { get; }
       
        public Ship Ship { get; }

        public Structure Structure { get; private set; }

        public int       IndexInStructure { get; private set; }

        Hex3 IHasHex3Coords.Coords => HexCoords;

        public Node(Ship ship, Hex3 coords) {
            this.Ship = ship;
            this.HexCoords = coords;
        }

        public void AssignStructure(Structure structure, int index = 0) {
            this.Structure = structure;
            this.IndexInStructure = index;
        }
    }

    public class Structure {
        List<Node> nodes = new();
        public StructureDeclaration Declaration { get; }
        public int VariantID { get; }

        public Ship Ship { get; }
        public IReadOnlyList<Node> Nodes => nodes;

        public void AssignNodes(IEnumerable<Node> nodes) => this.nodes = new(nodes);

        public Structure(Ship ship, StructureDeclaration decl, int variant) {
            this.Ship = ship;
            this.Declaration = decl;
            this.VariantID = variant;
        }
    }


    /// <summary>A tube always represents a connection between two ADJACENT hex coords.</summary>
    public class Tube {
        internal readonly Node moduleFrom;
        internal readonly Node moduleTo;

        public Tube(Node from, Node to) {
            this.Ship = from.Ship;
            this.moduleFrom = from;
            this.moduleTo = to;
        }

        public Hex3 CrdsFrom => moduleFrom.HexCoords;
        public Hex3 CrdsTo => moduleTo.HexCoords;
        public Ship Ship { get; }
    }

    // a ship consists of any number of modules and tubes
    // a "module" occupies a single hex. 
    // large structures consist of multiple modules positioned in some arrangement.
    public class Ship {
        Hex3SparseGrid<Node> nodeLookup = new();
        List<Tube> tubes = new();
        public IEnumerable<Node> Nodes { get {
            foreach (var item in nodeLookup.OccupiedHexes) yield return nodeLookup[item];
        } }
        public ICollection<Tube> Tubes => tubes;
        public IEnumerable<Structure> ListStructures() => Nodes.Select(n => n.Structure).Distinct();

        public Node GetNode(Hex3 hex) => nodeLookup.At(hex);

        public void BuildStructure(StructureDeclaration decl, int variantIndex, Hex3 initialHex) {
            var hexes = ShipBuildUtilities.GetHexes(decl.variants[variantIndex], initialHex);
            foreach (var hex in hexes)
                if (GetNode(hex) != null) 
                    throw new System.Exception("Node already exists at " + hex);
            
            var generatedNodes = hexes.Select(h => new Node(this, h)).ToList();
            var structure = new Structure(this, decl, variantIndex);
            foreach (var node in generatedNodes) nodeLookup.TryInsert(node);

            structure.AssignNodes(generatedNodes);

            for (var i = 0; i < generatedNodes.Count; i++) {
                var node = generatedNodes[i];
                node.AssignStructure(structure, i);
            }
        }
    }

    public static class ShipBuildUtilities
    {
        public static IList<Hex3> GetHexes(StructureVariant variant, Hex3 initialHex) {
            var n = variant.offsets.Count + 1;

            var list = new List<Hex3>(n);             
            return list;
        }
    }
}