using UnityEngine;
using Void.ColonySim;
using Void.ColonySim.BuildingBlocks;

namespace Scanner.Game {

    [DefaultExecutionOrder(-1000)]
    class HardcodedLoadGameOnInit : MonoBehaviour {
        private void Awake() {
            _HardcodedLoader.HardcodedCreateGame();
        }
    }

    public static class _HardcodedLoader {
        public static void HardcodedCreateGame() {
            var ruleRepo = new RulesRepository();
            var hc = new RulesHardcoder();
            var modules = hc.HardcodeModuleDeclarations();
            ruleRepo.Modules.Include(modules);

            var colonyS = new ColonyShipStructure();
            var colony = new Colony(colonyS);
            
            Game.CreateContext(ruleRepo, colony);
        }
    }
}
