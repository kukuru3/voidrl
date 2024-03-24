using System.Collections.Generic;
using System.IO;
using Core.H3;
using Void.ColonySim.Model;

namespace Scanner.Atomship {
    public class ModelIO {
        public const string BasePath = "Data\\Structures";

        public IEnumerable<HexBlueprint> LoadAllModels() {
           var di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), BasePath));
            if (!di.Exists) throw new System.Exception($"No directory {di.FullName}");            
            var ff = di.EnumerateFiles("*.structure");
            foreach (var f in ff) {
                var id = Path.GetFileNameWithoutExtension(f.Name);
                HexBlueprint m = null;
                try { 
                    m = LoadModel(id);
                } catch (System.Exception e) {
                    UnityEngine.Debug.LogException(e);
                }
                if (m != null) yield return m;
            }
        }

        public HexBlueprint LoadModel(string id) {
            var path = Path.Combine(BasePath, id + ".structure");
            var blob = File.ReadAllBytes(path);

            var result = new HexBlueprint();

            using var ms = new MemoryStream(blob);
            using var reader = new BinaryReader(ms);

            result.identity = reader.ReadString();
            var n = reader.ReadInt32();
            for (var i = 0; i < n; i++) {
                var p = reader.ReadInt32();
                var h = HexPack.UnpackSmallH3(p);
                result.nodes.Add(new HexBlueprint.HexNode { index = result.nodes.Count, hex = h });
            }

            n = reader.ReadInt32();

            for (var i = 0; i < n; i++) {
                var p = reader.ReadInt32();
                (var h3, var prism) = HexPack.UnpackSmallH3AndDir(p); 
                var flags = reader.ReadInt32();
                var cd = new HexBlueprint.HexConnector { index = result.connections.Count, sourceHex = h3, direction = prism, flags = flags };
                result.connections.Add(cd);
            }
            return result;
        }

        public void SaveModel(HexBlueprint hmd) {

            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);
            writer.Write(hmd.identity);
            writer.Write(hmd.nodes.Count);
            for (var i = 0; i < hmd.nodes.Count; i++) {
                var p = HexPack.PackSmallH3(hmd.nodes[i].hex);
                writer.Write(p);
            }

            writer.Write(hmd.connections.Count);
            for (var i = 0; i < hmd.connections.Count; i++) {
                var p = HexPack.PackSmallH3AndDir(hmd.connections[i].sourceHex, hmd.connections[i].direction);
                writer.Write(p);
                writer.Write(hmd.connections[i].flags);
            }

            var path = Path.Combine(BasePath, hmd.identity + ".structure");
            File.WriteAllBytes(path, ms.ToArray());
        }

        public IEnumerable<string> EnumerateStructures() {
            var di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), BasePath));
            if (!di.Exists) throw new System.Exception($"No directory {di.FullName}");
            
            var ff = di.EnumerateFiles("*.structure");
            foreach (var f in ff) yield return f.FullName;
        }
    }
}