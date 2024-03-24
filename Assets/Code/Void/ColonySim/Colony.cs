using System.Collections.Generic;
using Void.ColonySim.BuildingBlocks;

namespace Void.ColonySim {
    public class Colony {

        public ColonyShipStructure ShipStructure { get; }

        List<ISimulatedSystem> allSystems = new();
        public T GetSystem<T>() where T : ISimulatedSystem {
            foreach (var s in allSystems) if (s is T ts) return ts;
            return default;
        }

        public void AddSystem(ISimulatedSystem system) {
            allSystems.Add(system);
        }

        public Colony(ColonyShipStructure structure) {
            ShipStructure = structure;
        }

        public void SimTick() {
            foreach (var s in allSystems) s.Tick();
        }
    }
}
