using System.Collections.Generic;

namespace Void.ColonySim.Model {
    public interface ILogicExt { }

    public struct ModuleDeclaration {
        public string id;
        public string blueprint;
        public Construction construction;
        public Structural structural;
        public Logic logic;
    }

    public struct Construction {
        public Cost cost;
        public int  labour;
    }

    public struct Operation {
        public int labour;
        public int maintenanceComplexity;
    }

    public struct Cost {
        Dictionary<int, ResourceItem> items;

        public ResourceItem Get(int resourceID) {
            items.TryGetValue(resourceID, out var item);
            return item;
        }

        public Cost(IEnumerable<ResourceItem> items) {
            this.items = new Dictionary<int, ResourceItem>();
            foreach (var item in items) this.items.Add(item.resourceID, item);
        }

    }

    public struct ResourceItem {
        static public implicit operator ResourceItem((int resourceID, decimal amount) tuple) {
            return new ResourceItem { resourceID = tuple.resourceID, amount = tuple.amount };
        }

        static public implicit operator ResourceItem((string resourceID, decimal amount) tuple) {
            return new ResourceItem { resourceID = ResourceManager.HandleOf(tuple.resourceID), amount = tuple.amount };
        }

        public int resourceID;
        public decimal amount;
    }

    public struct Structural {
        public int weight;
        public int tensile;
        public int integrity;
    }

    public struct Logic {
        List<ILogicExt> extensions;

        public static Logic WithExtensions(params ILogicExt[] extensions) => new Logic { extensions = new List<ILogicExt>(extensions) };

        public void AddExtension(ILogicExt ext) {
            if (extensions == null) extensions = new List<ILogicExt>();
            extensions.Add(ext);
        }

        public T GetExtension<T>() {
            if (extensions == null) return default;
            foreach (var ext in extensions) { if (ext is T t) return t; }
            return default;
        }
        public IReadOnlyList<ILogicExt> Extensions {  get {
            if (extensions == null) extensions = new List<ILogicExt>();
            return extensions;
        } }
    }

    public struct Habitat : ILogicExt {
        public int capacity;
        public int comfort;
    }

    public struct Reactor : ILogicExt {
        public int heat;
        public Cost burnCost;
    }

    public struct Radiator : ILogicExt {
        public int radiated;
    }

    public struct RadiationSource : ILogicExt {
         public int radiationAmount;
    }

    public struct HeatTurbine : ILogicExt {
        public int conversionFactor; // out of 100
    }

}
