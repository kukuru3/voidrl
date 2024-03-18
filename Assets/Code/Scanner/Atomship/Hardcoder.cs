using System;
using System.IO;
using System.Linq;

namespace Scanner.Atomship {
    public static class Hardcoder {
        internal static void HardcodeRules() { 
        }
        
        internal static Ship GenerateInitialShip() { 
            var ship = new Ship();
            ship.BuildStructure(default, default, 0);
            return ship;

            //var sdecls = RuleContext.Repo.ListRules<StructureDeclaration>();
            //var srepo = RuleContext.Repo.GetRule<StructureModelRepo>("STRUCTURE_REPO");
            //var pose = new H3Pose(H3.zero, 0, 0);
            //var variant = 0;
            //foreach (var sd in sdecls) {
            //    var model = srepo.Get(sd.ID);
            //    var structure = new Structure(ship, sd, variant, pose);
            //    structure.AssignNodes(model.features.Select(f => new Node(ship, pose)));
            //    ship.AddStructure(structure);
            //    variant++;
            //}
            //return ship;
        }
        
    }

}

