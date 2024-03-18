using System.Collections.Generic;
using System.Linq;
using Core.H3;

namespace Scanner.Atomship {
    public class Node : IHasH3Coords {
        public Ship Ship { get; }

        public Structure Structure { get; private set; }

        public int       IndexInStructure { get; private set; }

        public H3Pose Pose { get; }
        public H3 Coords => Pose.position;

        public Node(Ship ship, H3Pose pose) {
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
        public H3Pose Pose { get; }
        public int VariantID { get; }

        public Ship Ship { get; }
        public IReadOnlyList<Node> Nodes => nodes;


        public void AssignNodes(IEnumerable<Node> nodes) => this.nodes = new(nodes);

        public Structure(Ship ship, StructureDeclaration decl, int variant, H3Pose pose) {
            Ship = ship;
            Declaration = decl;
            VariantID = variant;
            Pose = pose;
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

        public H3 CrdsFrom => moduleFrom.Coords;
        public H3 CrdsTo => moduleTo.Coords;
        public Ship Ship { get; }
    }

    // a ship consists of any number of modules and tubes
    // a "module" occupies a single hex. 
    // large structures consist of multiple modules positioned in some arrangement.
    public class Ship {
        H3SparseGrid<Node> nodeLookup = new();
        List<Tube> tubes = new();

        public IEnumerable<Node> Nodes { get {
            foreach (var item in nodeLookup.OccupiedHexes) yield return nodeLookup[item];
        } }

        public ICollection<Tube> Tubes => tubes;
        public IEnumerable<Structure> ListStructures() => Nodes.Select(n => n.Structure).Distinct();

        public Node GetNode(H3 hex) => nodeLookup.At(hex);

        // does not check for adjacenty or fit concerns. Just plops the hexes there.
        public void BuildStructure(StructureDeclaration decl, H3 pivot, int rotation) {
            throw new System.NotImplementedException("Reimplement this");
            //var pose = new H3Pose(pivot, rotation);

            //var structure = new Structure(this, decl, variantIndex, new H3Pose(pivot, rotation));
            
            //var l = new List<Node>();
            //foreach (var feature in decl.nodeModel.features) {
            //    if (feature.type == FeatureTypes.Part) {
            //        var finalPose = pose * new H3Pose(feature.localCoords, 0);
            //        l.Add(new Node(this, finalPose));
            //    }
            //}

            //for (var i = 0; i < l.Count; i++) { var node = l[i]; node.AssignStructure(structure, i); }

            //structure.AssignNodes(l);

            //foreach (var node in l) nodeLookup.TryInsert(node);
        }

        Tube BuildTube(Node from, Node to, string declaration) {
            var tube = new Tube(from, to, declaration);
            tubes.Add(tube);
            return tube;
        }

        public Tube BuildTube(H3 from, H3 to, string declaration) {
            var fromNode = GetNode(from);
            var toNode = GetNode(to);
            return BuildTube(fromNode, toNode, declaration);
        }
    }
    
}

