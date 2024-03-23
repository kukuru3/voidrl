using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Collections.Generic;
using System.IO;
using Core.H3;
using Void.Model;

namespace Scanner.Atomship {
    public class ModelIO {
        public const string BasePath = "Data\\Structures";
        private ISerializer serializer;
        private IDeserializer deserializer;

        public ModelIO() {
            PrepareSerializers();
        }

        void PrepareSerializers() {
            serializer = new SerializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();

            deserializer = new DeserializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .Build();
        }

        public HexModelDefinition LoadModel(string id) {
            var path = Path.Combine(BasePath, id + ".structure");
            var blob = File.ReadAllBytes(path);

            var result = new HexModelDefinition();

            using var ms = new MemoryStream(blob);
            using var reader = new BinaryReader(ms);

            result.identity = reader.ReadString();
            var n = reader.ReadInt32();
            for (var i = 0; i < n; i++) {
                var p = reader.ReadInt32();
                var h = HexPack.UnpackSmallH3(p);
                result.nodes.Add(new HexModelDefinition.HexNode { index = result.nodes.Count, hex = h });
            }

            n = reader.ReadInt32();

            for (var i = 0; i < n; i++) {
                var p = reader.ReadInt32();
                (var h3, var prism) = HexPack.UnpackSmallH3AndDir(p); 
                var flags = reader.ReadInt32();
                var cd = new HexModelDefinition.HexConnector { index = result.connections.Count, sourceHex = h3, direction = prism, flags = flags };
                result.connections.Add(cd);
            }
            return result;
        }

        public void SaveModel(HexModelDefinition hmd) {

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