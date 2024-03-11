using System.Collections.Generic;
using System.Linq;
using Core.h3x;

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
        public string ID { get; set; }
    }

    public class StructureDeclaration: Rule {
        internal List<StructureVariant> variants = new List<StructureVariant>();
    }

    public class StructureVariant {
        internal List<Hex3> offsets = new();
    }

    public static class Hardcoder {
        public static StructureDeclaration CreateStructureDecl(string id) {
            var decl = new StructureDeclaration {
                ID = id, 
                variants = new List<StructureVariant> { 
                    new StructureVariant {
                        offsets = new List<Hex3> { }
                    } 
                },
            };

            RuleContext.Repo.AddRule(decl);
            return decl;
        }

        public static void Hardcode() {
            // Spine transfers Transit, Power, Heat, Life Support, and is Structural
            CreateStructureDecl("spine");
        }
    }

}

