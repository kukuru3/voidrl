using Scanner.Atomship;
using UnityEngine;
using Void;
using Void.ColonySim;
using Void.ColonySim.BuildingBlocks;

namespace Scanner.GameUtils {

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

            var mio = new ModelIO();
            var models = mio.LoadAllModels();
            ruleRepo.HexBlueprints.Include(models);

            var colonyS = new ColonyShipStructure();
            var colony = new Colony(colonyS);

            Void.Game.CreateContext(ruleRepo, colony);
            colonyS.BuildModule(ruleRepo.Modules["spine"], default, default);
        }
    }
}
