using Void.ColonySim.BuildingBlocks;
using Void.ColonySim.Model;

namespace Void.ColonySim {
    public class TemperatureNode {
        internal float constantHeatDelta;

        internal float _heatDelta;
        internal float _heatFlowSum;
    }

    public class TemperatureConduit {
        // calculation stuff:
        internal float _potentialHeatFlowAtoB;
        
    }

    public class TemperatureGrid : DistributionSystem<TemperatureNode, TemperatureConduit>, ISimulatedSystem {
        public override TemperatureNode ProvideValue(ShipNode node) {
            var t = new TemperatureNode();
            var decl = node.Structure.Declaration;
            var heatRadiated = decl.logic.GetExtension<Radiator>().radiated;
            var heatProduced = decl.logic.GetExtension<Reactor>().heat;
            t.constantHeatDelta = heatProduced - heatRadiated;
            return t;
        }

        public override TemperatureConduit ProvideEdgeValue(Tube tube) {
            return new TemperatureConduit() {
                // capacity = 1000,  // weight of coolant  , in kg
            };
        }

        public const float specificHeatCapacity = 4186; // J/kgK, for water

        public void Tick() {            
            UnityEngine.Debug.Log("TemperatureGrid tick");


            // for each node, sum the incoming and outgoing heat flows
            foreach (var node in graph.Nodes) { node.Value._heatFlowSum = 0f; node.Value._heatDelta = node.Value.constantHeatDelta; }

            foreach (var edge in graph.pipes) {
                var v = edge.Value;
                var valA = edge.a.Value;
                var valB = edge.b.Value;

                // conductance factor:

                const float CAPACITANCE = 0.1f;

                v._potentialHeatFlowAtoB = CAPACITANCE * (valA.constantHeatDelta - valB.constantHeatDelta);

                valA._heatFlowSum -= v._potentialHeatFlowAtoB;
                valB._heatFlowSum += v._potentialHeatFlowAtoB;
            }

            
        }

    }
}
