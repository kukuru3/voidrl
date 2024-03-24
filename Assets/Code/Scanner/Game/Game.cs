using System.Collections;
using System.Collections.Generic;
using Void.ColonySim.Model;

namespace Scanner.Game {
    public class GameRun {
        public RulesRepository Rules { get; }
        public Void.ColonySim.Colony Colony { get; }
        internal GameRun(RulesRepository rules, Void.ColonySim.Colony colony) {
            Rules = rules;
            Colony = colony;
        }

    }

    internal static class Game {
        static GameRun currentRun;
        internal static void CreateContext(RulesRepository repo, Void.ColonySim.Colony colony) {
            currentRun = new GameRun(repo, colony);
        }

        public static RulesRepository Rules => currentRun.Rules;
        public static Void.ColonySim.Colony Colony => currentRun.Colony;
    }

    // rules need to contain stuff like: structure declarations.
    public class RulesRepository {
        public SafeList<ModuleDeclaration> Modules { get; } = new();
    }

    public class SafeList<T> : IReadOnlyList<T> {
        List<T> _list = new List<T>();

        public T this[int index] => ((IReadOnlyList<T>)_list)[index];

        public int Count => ((IReadOnlyCollection<T>)_list).Count;

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_list).GetEnumerator();

        public void Include(T item) { _list.Add(item); }
        public void Include(IEnumerable<T> items) { _list.AddRange(items); }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();
    }
}
