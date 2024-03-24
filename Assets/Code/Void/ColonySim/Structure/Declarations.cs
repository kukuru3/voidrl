using System.Collections.Generic;
using System.Linq;
using Core.H3;
using UnityEngine;
using Void.ColonySim.Model;
using K3;

namespace Void.ColonySim.BuildingBlocks {

    /// <summary>A "frame of reference" for the ship. All nodes must be in at least one frame of reference.
    /// Frames of reference form a tree, not a graph.</summary>
    public class ShipFrame {
        public ShipFrame parent;
        public List<ShipNode> nodes = new();
        public List<Tube> tubes = new();
        public List<ShipFrame> childFrames = new();

        /// <summary>relative to parent, if any</summary>
        public Pose localCartesianRoot = Pose.identity;
        public Pose CartesianRoot => parent == null ? localCartesianRoot : parent.CartesianRoot.Mul(localCartesianRoot);
    }

    public class ShipNode : IHasH3Coords {
        public ColonyShipStructure Ship { get; }

        public Module Structure { get; }

        public int       IndexInStructure { get; }

        public H3Pose Pose { get; }
        public H3 WorldPosition => Pose.position;

        public ShipNode(ColonyShipStructure ship, H3Pose pose, Module structure, int indexInStructure) {
            Ship = ship;
            Pose = pose;
            Structure = structure;
            IndexInStructure = indexInStructure;
        }
    }

    public class Module {
        public string name;

        List<ShipNode> nodes = new();
        public ModuleDeclaration Declaration { get; }
        public H3Pose Pose { get; }
        public ColonyShipStructure Ship { get; }
        public ShipFrame Frame { get; }
        public IReadOnlyList<ShipNode> Nodes => nodes;

        public void AssignNodes(IEnumerable<ShipNode> nodes) => this.nodes = new(nodes);

        public Module(ColonyShipStructure ship, ModuleDeclaration decl, H3Pose pose, ShipFrame targetFrame) {
            Ship = ship;
            Declaration = decl;
            Pose = pose;
            Frame = targetFrame;
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
        List<Module> moduleList = new();
        List<Tube> tubes = new();
        List<ShipFrame> frames = new();

        //public IEnumerable<ShipNode> Nodes { get {
        //    foreach (var item in nodeLookup.OccupiedHexes) yield return nodeLookup[item];
        //} }

        public IEnumerable<ShipNode> Nodes { get {
            foreach (var m in moduleList) foreach (var n in m.Nodes) yield return n;
        } }

        public ICollection<Tube> Tubes => tubes;

        public IReadOnlyList<ShipFrame> Frames => frames;

        public IReadOnlyList<Module> ListModules() => moduleList;

        public ShipNode GetNode(H3 hex) => nodeLookup.At(hex);

        string GenerateDefaultModuleName(ColonyShipStructure ship, ModuleDeclaration decl) {
            var numExisting = ship.ListModules().Where(s => s.Declaration == decl).Count();
            return $"{decl.id} {numExisting}";
        }

        // does not check for adjacenty or fit concerns. Just plops the hexes there.
        public Module BuildModule(ModuleDeclaration decl, H3 pivot, int rotation, ShipFrame targetFrame = null) {
            if (targetFrame != null) { 
                if (!frames.Contains(targetFrame)) throw new System.ArgumentException("provided frame not in ship");
            }

            var pose = new H3Pose(pivot, rotation);
            var module = new Module(this, decl, pose, targetFrame);

            var l = new List<ShipNode>();

            var hexModel = Game.Rules.HexBlueprints[decl.blueprint];

            if (hexModel == null) throw new System.InvalidOperationException($"No hex model found for {decl.blueprint}");

            foreach (var node in hexModel.nodes) {
                var finalPose = pose * new H3Pose(node.hex, 0);
                var shipNode = new ShipNode(this, finalPose, module, l.Count);
                l.Add(shipNode);
                nodeLookup.TryInsert(shipNode);
            }

            module.AssignNodes(l);
            module.name = GenerateDefaultModuleName(this, decl);

            moduleList.Add(module);

            return module;
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

