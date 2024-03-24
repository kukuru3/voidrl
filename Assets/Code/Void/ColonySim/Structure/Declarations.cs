using System.Collections.Generic;
using System.Linq;
using Core.H3;
using Void.ColonySim.Model;

namespace Void.ColonySim.BuildingBlocks {
    public class ShipNode : IHasH3Coords {
        public ColonyShipStructure Ship { get; }

        public NodularStructure Structure { get; }

        public int       IndexInStructure { get; }

        public H3Pose Pose { get; }
        public H3 WorldPosition => Pose.position;

        public ShipNode(ColonyShipStructure ship, H3Pose pose, NodularStructure structure, int indexInStructure) {
            Ship = ship;
            Pose = pose;
            Structure = structure;
            IndexInStructure = indexInStructure;
        }
    }

    public class NodularStructure {

        public string name;

        List<ShipNode> nodes = new();
        public ModuleDeclaration Declaration { get; }
        public H3Pose Pose { get; }
        public ColonyShipStructure Ship { get; }
        public IReadOnlyList<ShipNode> Nodes => nodes;


        public void AssignNodes(IEnumerable<ShipNode> nodes) => this.nodes = new(nodes);

        public NodularStructure(ColonyShipStructure ship, ModuleDeclaration decl, H3Pose pose) {
            Ship = ship;
            Declaration = decl;
            Pose = pose;
        }
    }

    /// <summary>A tube always represents a connection between two ADJACENT hex coords.</summary>
    public class Tube {
        internal readonly ShipNode moduleFrom;
        internal readonly ShipNode moduleTo;
        internal readonly string decl;

        public Tube(ShipNode from, ShipNode to, string declaration) {
            this.Ship = from.Ship;
            this.moduleFrom = from;
            this.moduleTo = to;
            this.decl = declaration;
        }

        public H3 CrdsFrom => moduleFrom.WorldPosition;
        public H3 CrdsTo => moduleTo.WorldPosition;
        public ColonyShipStructure Ship { get; }
    }

    // a ship consists of any number of modules and tubes
    // a "module" occupies a single hex. 
    // large structures consist of multiple modules positioned in some arrangement.


    /// <summary>A structural representation of a large colony ship. Consists of nodes and tubes.</summary>
    public class ColonyShipStructure {
        H3SparseGrid<ShipNode> nodeLookup = new();
        List<Tube> tubes = new();

        public IEnumerable<ShipNode> Nodes { get {
            foreach (var item in nodeLookup.OccupiedHexes) yield return nodeLookup[item];
        } }

        public ICollection<Tube> Tubes => tubes;
        public IEnumerable<NodularStructure> ListStructures() => Nodes.Select(n => n.Structure).Distinct();

        public ShipNode GetNode(H3 hex) => nodeLookup.At(hex);

        string GenerateDefaultStructureName(ColonyShipStructure ship, ModuleDeclaration decl) {
            var numExisting = ship.ListStructures().Where(s => s.Declaration == decl).Count();
            return $"{decl.id} {numExisting}";
        }

        // does not check for adjacenty or fit concerns. Just plops the hexes there.
        public void BuildStructure(ModuleDeclaration decl, H3 pivot, int rotation) {
            var pose = new H3Pose(pivot, rotation);
            var structure = new NodularStructure(this, decl, pose);

            var l = new List<ShipNode>();

            var hexModel = Game.Rules.HexBlueprints[decl.blueprint];

            if (hexModel == null) throw new System.InvalidOperationException($"No hex model found for {decl.blueprint}");

            foreach (var node in hexModel.nodes) {
                var finalPose = pose * new H3Pose(node.hex, 0);
                var shipNode = new ShipNode(this, finalPose, structure, l.Count);
                l.Add(shipNode);
                nodeLookup.TryInsert(shipNode);
            }

            structure.AssignNodes(l);
            structure.name = GenerateDefaultStructureName(this, decl);
        }

        Tube BuildTube(ShipNode from, ShipNode to, string declaration) {
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

