using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Scanner.Atomship {

    public static class RuleContext {
        public static RuleRepo Repo { get; } = new RuleRepo();
    }
    
    public class RuleRepo {
        List<Rule> rules = new();
        public IReadOnlyList<Rule> Rules => rules;

        public void AddRule(Rule rule) => rules.Add(rule);

        public T GetRule<T>(string id) where T : Rule {
            foreach (var rule in rules) if (rule is T trule && trule.ID == id) return trule;
            return default;
        }

        public IEnumerable<T> ListRules<T>() where T : Rule {
            foreach (var rule in rules) if (rule is T trule) yield return trule;
        }
    }

    public abstract class Rule { 
        public virtual string ID { get; set; }
    }

    public class StructureModelRepo : Rule {
        public override string ID { get => "STRUCTURE_REPO"; set => throw new System.InvalidOperationException("Cannot rename"); }

        Dictionary<string, StructureModel> models = new();

        public void Register(string key, StructureModel model) => models[key] = model;
        public StructureModel Get(string key) => models[key];
    }

    public class StructureDeclaration: Rule {
        public StructureModel nodeModel;
    }

    public static class Hardcoder {

        static StructureModelRepo structRepo;
        public static StructureDeclaration DeclareStructure(string id, string structuralModelID) {

            var decl = new StructureDeclaration {
                ID = id,
                nodeModel = structRepo.Get(structuralModelID),
            };

            RuleContext.Repo.AddRule(decl);
            return decl;
        }

        public static void Hardcode() {

            LoadRules();

            DeclareStructure("spine", "universal_single");
            DeclareStructure("fusion_reactor", "universal_single");
            DeclareStructure("radiator", "radiator_3");
            DeclareStructure("turbine", "universal_single");
            DeclareStructure("bridge", "universal_single");
            DeclareStructure("habitat", "universal_single");
            DeclareStructure("engineering", "universal_single");
            DeclareStructure("hydroponics", "universal_single");
            DeclareStructure("small_engine", "small_engine");

            // in this order: 
            // spine
            // reactor
            // radiator
            // heat turbine
            // bridge
            // habitat
            // engineering
            // hydroponics
            // engines

            // initial ship: 7 spine segments, infinite building
            
            // Spine transfers Transit, Power, Heat, Life Support, and is Structural
            // CreateStructureDecl("spine");
        }

        private static void LoadRules() {
            var modelRepo = new StructureModelRepo();
            structRepo = modelRepo;
            RuleContext.Repo.AddRule(modelRepo);

            const string folder = "Data\\Structures";
            var dd = Directory.GetCurrentDirectory();
            var finalPath = Path.Combine(dd, folder);
            var di = new DirectoryInfo(finalPath);
            var structFiles = di.EnumerateFiles("*.structure", SearchOption.AllDirectories);

            var ff = structFiles.Select(f => f.FullName).ToList();
            foreach (var f in ff) {
                var blob = File.ReadAllBytes(f);
                var nn = Path.GetFileNameWithoutExtension(f);
                var model = ModelSerializer.Deserialize(blob);
                modelRepo.Register(nn, model);
            }

            UnityEngine.Debug.Log($"Loaded {ff.Count} structrual files");
        }
    }

}

