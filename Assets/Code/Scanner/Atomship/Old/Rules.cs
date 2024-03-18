using System.Collections.Generic;
using System.Diagnostics;

namespace Scanner.Atomship.Old {

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

}

