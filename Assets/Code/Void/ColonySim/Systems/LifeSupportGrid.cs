using System.Collections.Generic;
using System.Linq;
using Void.ColonySim.BuildingBlocks;
using Void.ColonySim.Model;

namespace Void.ColonySim {
    public class LifeSupportGrid : DistributionSystem<LifeSupportNode, LifeSupportConduit>, ISimulatedSystem {
        public override LifeSupportConduit ProvideEdgeValue(Tube tube) {
            return new LifeSupportConduit();
        }

        public override LifeSupportNode ProvideValue(ShipNode node) {
            var ls = node.Structure.Declaration.logic.GetExtension<LifeSupport>();
            if (ls.totalCapacity > 0) {
                return new LifeSupportProvider(ls);
            }
            var draw = 100;
            var hab = node.Structure.Declaration.logic.GetExtension<Habitat>();
            if (hab.capacity > 0) draw = 500;

            return new LifeSupportConsumer { drawRequirements = draw };
        }
        

        public override bool HasNode(ShipNode node) => true;
        public override bool HasConduit(Tube tube) => true;


        class ConsumerProviderLink {
            internal LifeSupportProvider provider;
            internal int distance;
            internal int weight;
            internal List<DistroPipe<LifeSupportNode, LifeSupportConduit>> path = new();
        }

        static int[] _weights = new[] { 4000, 1000, 200, 50, 12, 4, 1, 1, 1 };

        static int[] _fulfilment = new[] { 100, 100, 90, 80, 60, 50, 50, 40, 40, 30, 20, 10 };

        public void Tick() {
            var providers = graph.Nodes.Select(n => n.Value).OfType<LifeSupportProvider>();
            var consumers = graph.Nodes.Select(n => n.Value).OfType<LifeSupportConsumer>();

            List<ConsumerProviderLink> foundProviders = new();
            foreach (var provider in providers) provider.ClearDemands();
            foreach (var consumer in consumers) consumer.ClearReceived();

            if (graph.pipes != null) { 
                foreach (var pipe in graph.pipes) pipe.Value.conducted = 0;
            }

            // consumers calculate and distribute interest
            foreach (var consumer in consumers) {
                foundProviders.Clear();
                var fill = graph.pathfinder.DijkstraFloodFill(consumer.GraphNode, maxRange: 5);
                foreach (var tile in fill) if (tile.Value is LifeSupportProvider provider) {
                    var path = graph.pathfinder.DijkstraPath(tile);
                    foundProviders.Add( new ConsumerProviderLink { distance = tile.dijkstance, provider = provider, path = path});
                }

                int totalWeight = 0;
                foreach (var p in foundProviders) {
                    // assign initial weight based on distance
                    p.weight = _weights[p.distance];
                    totalWeight += p.weight;
                }

                if (totalWeight < 0) continue;
                foreach (var p in foundProviders) {
                    var pipes = p.path.Select(p => p.Value);
                    p.provider.SetDemand(consumer, pipes, p.weight);
                }
            }

            const int NUM_ITERS = 2;
            const int COMPENSATION = 100;
                
            for (var i = 0; i < NUM_ITERS; i++) {

                foreach (var consumer in consumers) consumer.ResolveDemands();

                foreach (var provider in providers) {
                    provider.SummateDemands();
                    provider.PercentageDemanded = 100 * provider.SumOfDemands / provider.ls.totalCapacity;

                    foreach (var demand in provider.Demands) {
                        var nextWeight = demand.weight * 100 / provider.PercentageDemanded;
                        demand.weight = BringCloser(demand.weight, demand.weight * 100 / provider.PercentageDemanded, COMPENSATION);
                    }
                }
            }

            foreach (var consumer in consumers) consumer.ResolveDemands();

            foreach (var provider in providers) { 
                provider.SummateDemands();
                provider.PercentageDemanded = 100 * provider.SumOfDemands / provider.ls.totalCapacity;
                foreach (var d in provider.Demands) {
                    var finalSentAmount = d.demandedDraw;
                    if (provider.PercentageDemanded > 100) finalSentAmount = finalSentAmount * 100 / provider.PercentageDemanded;
                    d.amountPushed = finalSentAmount;
                    foreach (var pipe in d.pipes) pipe.conducted += finalSentAmount;
                }
            }
            foreach (var consumer in consumers) consumer.ResolveDraw();
            

            // if amount is 100, return b;
            static int BringCloser(int a, int b, int amount) {
                return a * amount / 100 + b * (100 - amount) / 100;
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

    internal class Link {
        public LifeSupportConsumer consumer;
        public LifeSupportProvider provider;
        public List<LifeSupportConduit> pipes = new();
        public int demandedDraw;
        public int amountPushed;
        public int weight; // used in some calculations
    }

    public class LifeSupportProvider : LifeSupportNode {
        public LifeSupport ls { get; private set; }

        public LifeSupportProvider(LifeSupport ls) => this.ls = ls;

       

        Dictionary<LifeSupportConsumer, Link> demandLookup = new Dictionary<LifeSupportConsumer, Link>();

        internal IReadOnlyCollection<Link> Demands => demandLookup.Values;

        internal Link GetDemand(LifeSupportConsumer consumer) => demandLookup[consumer];

        public int PercentageDemanded { get; internal set; }

        public void ClearDemands() { demandLookup.Clear(); }
        public void SetDemand(LifeSupportConsumer consumer, IEnumerable<LifeSupportConduit> pipes, int weight) {
            if (!demandLookup.TryGetValue(consumer, out var demand)) {
                demand = new Link { consumer = consumer, provider = this, pipes = new List<LifeSupportConduit>(pipes) };
                demandLookup[consumer] = demand;
                consumer.Link(demand);
            }
            demand.weight = weight;
        }

        internal void SummateDemands() {
            SumOfDemands = 0;
            foreach (var d in Demands) SumOfDemands += d.demandedDraw;
        }

        public int SumOfDemands { get; private set; }
    }

    public class LifeSupportConsumer : LifeSupportNode  {
        public int drawRequirements;
        public int totalReceived;
        internal Dictionary<LifeSupportProvider, Link> links = new();

        internal void ClearReceived() {
            links.Clear();
        }


        internal void Link(Link demand) {
            links[demand.provider] = demand;
        }
        
        internal void ResolveDemands() {
            var weightsum = 0;
            foreach (var d in links.Values) { weightsum += d.weight; }
            foreach (var d in links.Values) { d.demandedDraw = drawRequirements * d.weight / weightsum; }
        }

        internal void ResolveDraw() {
            totalReceived = 0;
            foreach (var d in links.Values) totalReceived += d.amountPushed;
        }
    }

    public class LifeSupportConduit : IGraphPipeAware<LifeSupportNode, LifeSupportConduit> {
        public DistroPipe<LifeSupportNode, LifeSupportConduit> GraphPipe { get; set; }

        public int conducted;
    }
}
