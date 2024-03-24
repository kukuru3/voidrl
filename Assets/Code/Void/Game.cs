using System.Collections;
using System.Collections.Generic;
using Void.ColonySim.Model;

namespace Void {
    public class GameRun {
        public RulesRepository Rules { get; }
        public ColonySim.Colony Colony { get; }
        internal GameRun(RulesRepository rules, Void.ColonySim.Colony colony) {
            Rules = rules;
            Colony = colony;
        }
    }

    public static class Game {
        static GameRun currentRun;
        public static void CreateContext(RulesRepository repo, ColonySim.Colony colony) {
            currentRun = new GameRun(repo, colony);
        }

        public static void ReplaceColony(ColonySim.Colony colony) {
            currentRun = new GameRun(currentRun.Rules, colony);
        }

        public static RulesRepository Rules => currentRun.Rules;
        public static ColonySim.Colony Colony => currentRun.Colony;
    }

    // rules need to contain stuff like: structure declarations.
    public class RulesRepository {
        public SafeList<ModuleDeclaration> Modules { get; } = new(m => m.id);
        public SafeList<HexBlueprint> HexBlueprints { get; } = new(s => s.identity);
    }

    public class SafeList<T> : IReadOnlyList<T> {

        public delegate string KeyAccessor(T item);

        KeyAccessor lookupDelegate;

        public SafeList(KeyAccessor withLookupDelegate = null) {
            lookupDelegate = withLookupDelegate;
        }

        List<T> _list = new List<T>();
        Dictionary<string, T> _lookup = new Dictionary<string, T>();

        public T this[int index] => ((IReadOnlyList<T>)_list)[index];

        public T this[string id] { get {
            if (lookupDelegate == null) throw new System.Exception("No lookup delegate provided, cannot index by string");
            _lookup.TryGetValue(id, out var value); return value; 
        } }

        public int Count => ((IReadOnlyCollection<T>)_list).Count;

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_list).GetEnumerator();

        public void Include(T item) { 
            _list.Add(item); 
            if (lookupDelegate != null) {
                var key = lookupDelegate(item);
                _lookup.Add(key, item);
            }
        }
        public void Include(IEnumerable<T> items) { foreach (var item in items) Include(item); }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();
    }
}
