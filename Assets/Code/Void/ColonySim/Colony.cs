using Void.ColonySim.BuildingBlocks;

namespace Void.ColonySim {
    public class Colony {

        public ColonyShipStructure ShipStructure { get; }

        public Colony(ColonyShipStructure structure) {
            ShipStructure = structure;
        }
    }
}
