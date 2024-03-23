using System.Collections.Generic;
using System.Linq;
using Void.Data;

namespace Void {

    public static class ResourceManager {
        static List<string> list = new();
        static Dictionary<string, int> lookup = new();

        public static int HandleOf(string name) {
            name = name.ToLowerInvariant();
            if (lookup.TryGetValue(name, out var idx)) {
                idx = list.Count;
                list.Add(name);
            }
            return idx;
        }
        public static IEnumerable<(int id, string name)> Values => lookup.Select(lookup => (lookup.Value, lookup.Key));
    }


    public class ResourcePool {
        public Dictionary<string, long> resources;

        void EnsureKeyExists(string id) {
            if (!resources.ContainsKey(id)) resources.Add(id, 0);
        }
        
        public long GetResource(string id) {
            resources.TryGetValue(id, out var value);
            return value;
        }

        public void AddResource(string id, long amount) {
           EnsureKeyExists(id);
            resources[id] += amount;
        }

        public enum AffordStatus {
            CannotAfford,
            CanAfford,
            PartiallyAfforded,
        }
        
        public AffordStatus TryRemoveResource(string id, long amount) {
            EnsureKeyExists(id);
            if (amount < 0) throw new System.ArgumentException($"Amount supplied is < 0 for resource `{id}`");
            if (amount == 0) return AffordStatus.CanAfford;
            if (resources[id] >= amount) {
                resources[id] -= amount;
                return AffordStatus.CanAfford;
            }
            return AffordStatus.CannotAfford;
        }

        public (long removed, AffordStatus status) RemoveResourcePartial(string id, long amount) {
            EnsureKeyExists(id);
            if (amount < 0) throw new System.ArgumentException($"Amount supplied is < 0 for resource `{id}`");
            if (amount == 0) return (0, AffordStatus.CanAfford);
            if (resources[id] >= amount) {
                resources[id] -= amount;
                return (amount, AffordStatus.CanAfford);
            } else {
                var r = resources[id];
                resources[id] = 0;
                return (r, AffordStatus.PartiallyAfforded);
            }
        } 

        /// <summary>Expected syntax: "energy: 300, minerals: 100"</summary>
        public static ResourcePool Build(string rawString) {
            var rp = new ResourcePool();
            var items = rawString.Split(',');
            foreach (var item in items) {
                var str = item.Trim();
                if (string.IsNullOrWhiteSpace(str)) continue;
                var arr = str.Split(':');
                if (arr.Length != 2) throw new System.Exception($"Malformed resource string: `{item}`");
            }
            return rp;
        }
    }
    
    public class StockpiledItem {
        public readonly ItemDeclaration declaration;
        public int count;
        
        public StockpiledItem(ItemDeclaration declaration, int count = 1) {
            this.declaration = declaration;
            this.count = count;
        }
    }

    // a CARGO consists of a LOCAL RESOURCE POOL and a list of STOCKPILED ITEMS
    
    public class Cargo {
        public ResourcePool resources = new();
        public List<StockpiledItem> items = new();

        static public implicit operator Cargo(ResourcePool a) {
            return new Cargo { resources = a };
        }

        static public implicit operator Cargo(string resourcePoolString) {
            return new Cargo { resources = ResourcePool.Build(resourcePoolString) };
        }
    }
}
