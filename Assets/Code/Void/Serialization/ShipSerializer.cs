using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.H3;
using Void.ColonySim.BuildingBlocks;

namespace Void.Serialization {
    public class ShipSerializer {

        public const int VERSION = 1;
        public byte[] SerializeStructure(ColonyShipStructure shipStructure) {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            
            writer.Write(VERSION);

            var frames = shipStructure.Frames.ToList();            
            var strcts = shipStructure.ListModules().ToList();

            if (strcts.Count == 0) throw new Exception($"Num structures 0, corrupted file");
            var fc = frames.Count();
            var sc = strcts.Count;

            writer.Write(fc);
            writer.Write(sc);

            var _temp = ms.ToArray();

            // write all structures
            foreach (var s in strcts) {
                var fid = -1;
                if (s.Frame != null) fid = frames.IndexOf(s.Frame);

                writer.Write(fid);
                writer.Write(s.Declaration.id);
                writer.Write(s.name);
                writer.Write(s.Pose);
            }

            // writer.Write(shipStructure.Nodes.Count());
            // write all nodes, with structure indices:
            //foreach (var n in shipStructure.Nodes) {
            //    writer.Write(strcts.IndexOf(n.Structure));
            //    writer.Write(n.IndexInStructure);
            //    // pose is inferrable from structure and index in structure. For sake of data norm, do not write it.
            //}

            Dictionary<ShipNode, int> nodeIndices = shipStructure.Nodes.Select((n, i) => (n, i)).ToDictionary(x => x.n, x => x.i);

            writer.Write(shipStructure.Tubes.Count());
            foreach (var t in shipStructure.Tubes) {
                var a = nodeIndices[t.moduleFrom];
                var b = nodeIndices[t.moduleTo];
                writer.Write(a);
                writer.Write(b);
                writer.Write(t.decl);
            }
            var arr = ms.ToArray();
            return arr;
        }

        public ColonyShipStructure DeserializeStructure(byte[] data) {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);

            var result = new ColonyShipStructure();

            var version = reader.ReadInt32();
            if (version != VERSION) throw new Exception($"Version mismatch! savefile: {version}, algo={VERSION}");
            var numFrames = reader.ReadInt32();
            var numStructures = reader.ReadInt32();
            if (numStructures == 0) throw new Exception($"Num structures 0, corrupted file");
            for (var i = 0; i < numStructures; i++) {
                var frameID = reader.ReadInt32();
                var declID = reader.ReadString();
                var name = reader.ReadString();
                var pose = reader.ReadH3Pose();
                var declaration = Game.Rules.Modules[declID];
                // var frame = (frameID == -1) ? null : result.Frames[frameID];
                var structure = result.BuildModule(declaration, pose.position, pose.rotation, null);
                structure.name = name;
            }

            var nodes = result.Nodes.ToArray();

            var numTubes = reader.ReadInt32();
            for (var i = 0; i < numTubes; i++) {
                var a = reader.ReadInt32();
                var b = reader.ReadInt32();
                var decl = reader.ReadString();
                result.BuildTube(nodes[a].WorldPosition, nodes[b].WorldPosition, decl);
            }

            return result;
        }
    }

    public static class ReadWriteExtensions {
        public static void Write(this BinaryWriter writer, K3.Hex.Hex hex) {
            writer.Write(hex.q);
            writer.Write(hex.r);
        }
        public static void Write(this BinaryWriter writer, H3 h) {
            writer.Write(h.hex);
            writer.Write(h.zed);
        }

        public static void Write(this BinaryWriter writer, H3Pose pose) {
            writer.Write(pose.position);
            writer.Write(pose.rotation);
        }

        public static K3.Hex.Hex ReadHex(this BinaryReader reader) {
            return new K3.Hex.Hex(reader.ReadInt32(), reader.ReadInt32());
        }

        public static H3 ReadH3(this BinaryReader reader) {
            return new H3(reader.ReadHex(), reader.ReadInt32());
        }

        public static H3Pose ReadH3Pose(this BinaryReader reader) {
            return new H3Pose(reader.ReadH3(), reader.ReadInt32());
        }
    }
}
