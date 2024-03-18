using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Collections.Generic;
using System.IO;

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
            var blob = File.ReadAllText(path);
            var hmd = deserializer.Deserialize<HexModelDefinition>(blob);
            return hmd;
        }

        public void SaveModel(HexModelDefinition hmd) {
            var path = Path.Combine(BasePath, hmd.identity + ".structure");
            var blob = serializer.Serialize(hmd);
            File.WriteAllText(path, blob);
        }

        public IEnumerable<string> EnumerateFiles(string path, string pattern) {
            var di = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), BasePath, path));
            if (!di.Exists) throw new System.Exception($"No directory {path}");
            
            var ff = di.EnumerateFiles(pattern);
            foreach (var f in ff) yield return f.FullName;
        }
    }
}