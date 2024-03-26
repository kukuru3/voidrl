using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Void.ColonySim.BuildingBlocks;
using Void.ColonySim.Model;

namespace Void.ColonySim {
    public class LifeSupportGrid : DistributionSystem<LifeSupportNode, LifeSupportConduit>, ISimulatedSystem {
        public override LifeSupportConduit ProvideEdgeValue(Tube tube) {
            return base.ProvideEdgeValue(tube);
        }

        public override LifeSupportNode ProvideValue(ShipNode node) {
            var ls = node.Structure.Declaration.logic.GetExtension<LifeSupport>();
            if (ls.totalCapacity > 0) {
                return new LifeSupportProvider(ls);
            }
            var draw = 10;
            var hab = node.Structure.Declaration.logic.GetExtension<Habitat>();
            if (hab.capacity > 0) draw = 50;

            return new LifeSupportConsumer { drawRequirements = draw };
        }

        public override bool HasNode(ShipNode node) => true;
        public override bool HasConduit(Tube tube) => true;


        class FoundProvider {
            internal LifeSupportProvider provider;
            internal int distance;
            internal int weight;
        }

        public void Tick() {
            var providers = graph.Nodes.Select(n => n.Value).OfType<LifeSupportProvider>();
            var consumers = graph.Nodes.Select(n => n.Value).OfType<LifeSupportConsumer>();

            List<FoundProvider> foundProviders = new();
            foreach (var provider in providers) provider.ClearDemands();
            foreach (var consumer in consumers) consumer.ClearCalc();

            // consumers calculate and distribute interest
            foreach (var consumer in consumers) {
                foundProviders.Clear();
                var fill = graph.pathfinder.DijkstraFloodFill(consumer.GraphNode, maxRange: 5);
                foreach (var tile in fill) if (tile.Value is LifeSupportProvider provider) {
                    foundProviders.Add( new FoundProvider { distance = tile.dijkstance, provider = provider});
                }

                int totalWeight = 0;
                foreach (var p in foundProviders) {
                    if (p.distance == 0) p.weight = 3000;
                    else p.weight = 1000 / p.distance;
                    totalWeight += p.weight;
                }

                foreach (var p in foundProviders) {
                    p.provider.SetDemand(consumer, p.weight * consumer.drawRequirements / totalWeight);
                }
            }

            foreach (var provider in providers) {
                var allDemands = provider.SumDemands();
                var percentageCapacityOccupied = 100 * provider.SumDemands() / provider.ls.totalCapacity;                
                bool overCapacity = percentageCapacityOccupied > 100;
                foreach (var d in provider.Demands) {
                    var finalAmount = d.demandedAmount;
                    if (overCapacity) finalAmount = finalAmount * 100 / percentageCapacityOccupied;
                    d.consumer.Receive(provider, finalAmount);
                }
            }
        }
    }


    // pump-crpmodel:
    // each consumer: does a dijkstra fill towards pumps in a certain radius.
    // a consumer can have variable draws.
    // pumps located by the consumer are scored via a weighting algorithm, but distance is the biggest factor
    // pumps are notified of the consumer's interest and maintain a list of registered consumers.

    // pumps calculate the total demand, including inefficiency.
    // pumps advertise their target capacity / demand / saturation

    // OPTIONAL INTERIM OPTIMIZATION STEP: 
    //  - the pumps can notify the consumers of their inability to keep up with the demand
    //  - the consumers are then allowed to modify or redistribute their draw amount
    //  - you can iterate once or twice. Since this does no pathfinding, it should be fast.

    // pumps push the requested amount to the consumers, or a proportion of it if they can't keep up.


    public abstract class LifeSupportNode : IGraphNodeAware<LifeSupportNode, LifeSupportConduit> {
        public DistroNode<LifeSupportNode, LifeSupportConduit> GraphNode { get; set; }
    }

    public class LifeSupportProvider : LifeSupportNode {
        public LifeSupport ls { get; private set; }

        public LifeSupportProvider(LifeSupport ls) => this.ls = ls;

        internal class Demand {
            public LifeSupportConsumer consumer;
            public int demandedAmount;
        }

        Dictionary<LifeSupportConsumer, Demand> demandLookup = new Dictionary<LifeSupportConsumer, Demand>();

        internal IReadOnlyCollection<Demand> Demands => demandLookup.Values;

        public void ClearDemands() { demandLookup.Clear(); }
        public void SetDemand(LifeSupportConsumer consumer, int drawPreference) {
            if (!demandLookup.TryGetValue(consumer, out var demand)) {
                demand = demandLookup[consumer] = new Demand { consumer = consumer };
            }
            demand.demandedAmount = drawPreference;
        }

        public int SumDemands() {
            var sum = 0;
            foreach (var demand in demandLookup.Values) sum += demand.demandedAmount;
            return sum;
        }
    }

    public class LifeSupportConsumer : LifeSupportNode  {
        public int drawRequirements;

        internal void ClearCalc() {
            totalReceived = 0;
            received.Clear();
        }

        public int totalReceived;

        public Dictionary<LifeSupportProvider, int> received = new();

        internal void Receive(LifeSupportProvider provider, int amount) {
            received[provider] = amount;
        }
    }

    public class LifeSupportConduit : IGraphPipeAware<LifeSupportNode, LifeSupportConduit> {
        public DistroPipe<LifeSupportNode, LifeSupportConduit> GraphPipe { get; set; }
    }
}
