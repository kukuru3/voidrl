using System.Collections.Generic;

namespace Void.ColonyModel {

    static class Extensions {
        public static ModuleDeclaration With(this ModuleDeclaration decl, ILogicExt logic) {
            decl.logic.AddExtension(logic);
            return decl;
        }
    }

    public class ModelHardcoder {

        Construction defaultConstruction = new Construction { cost = new Cost(new ResourceItem[] {  
            ("resources", 1000) 
        }), labour = 1000 };

        Structural defaultStructure = new Structural {
            integrity = 100,
            tensile = 10,
            weight = 100,
        };
        
        public IEnumerable<ModuleDeclaration> Hardcode() {
            var sp = CreateDeclaration("spine", "spine");
            sp.structural = new Structural { integrity = 300, tensile = 100, weight = 80 };
            yield return sp;

            yield return CreateDeclaration("habitat", "omni")
                .With(new Habitat { capacity = 50000, comfort = 10 });

            yield return CreateDeclaration("reactor-core", "omni")
                .With(new Reactor { heat = 1000, burnCost = new() })
                .With(new RadiationSource {  radiationAmount = 1000 });

            yield return CreateDeclaration("radiator", "radiator3")
                .With(new Radiator { radiated = 1000 });

            yield return CreateDeclaration("heat-turbine", "omni") 
                .With(new HeatTurbine { conversionFactor = 100 });
        }

        ModuleDeclaration CreateDeclaration(string id, string blueprint) {
            return new ModuleDeclaration {
                id = id,
                blueprint = blueprint,
                construction = defaultConstruction,
                structural = defaultStructure,
                logic = new Logic(),
            };
        }
    }

}
