using System.Collections.Generic;
using System.Linq;
using Core;
using Core.h3x;

namespace Scanner.Atomship {
    public class Node : IHasHex3Coords {
        public Ship Ship { get; }

        public Structure Structure { get; private set; }

        public int       IndexInStructure { get; private set; }

        public HexPose Pose { get; }
        public Hex3 Coords => Pose.position;

        public Node(Ship ship, HexPose pose) {
            Ship = ship;
            Pose = pose;
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
        internal readonly string decl;

        public Tube(Node from, Node to, string declaration) {
            this.Ship = from.Ship;
            this.moduleFrom = from;
            this.moduleTo = to;
            this.decl = declaration;
        }

        public Hex3 CrdsFrom => moduleFrom.Coords;
        public Hex3 CrdsTo => moduleTo.Coords;
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

        // does not check for adjacenty or fit concerns. Just plops the hexes there.
        public void BuildStructure(StructureDeclaration decl, int variantIndex, Hex3 pivot, int rotation) {
            var pose = new HexPose(pivot, rotation);

            var structure = new Structure(this, decl, variantIndex);
            
            var l = new List<Node>();
            foreach (var feature in decl.nodeModel.features) {
                if (feature.type == FeatureTypes.Part) {
                    var finalPose = pose * new HexPose(feature.localCoords, 0);
                    l.Add(new Node(this, finalPose));
                }
            }

            for (var i = 0; i < l.Count; i++) { var node = l[i]; node.AssignStructure(structure, i); }

            structure.AssignNodes(l);

            foreach (var node in l) nodeLookup.TryInsert(node);
        }

        public Tube BuildTube(Node from, Node to, string declaration) {
            var tube = new Tube(from, to, declaration);
            tubes.Add(tube);
            return tube;
        }
    }
    
}

