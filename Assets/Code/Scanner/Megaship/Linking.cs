using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanner.Megaship {
    public enum Polarities {
        Male,
        Female,
        TwoWay,
    }

    // a group of one or more LINKS, connecting any number of modules in between
    public class Linkage {
        public List<(IPlug a, IPlug b)> pairings = new();
        public IEnumerable<IPlug> ASidePlugs => pairings.Select(p => p.a);
        public IEnumerable<IPlug> BSidePlugs => pairings.Select(p => p.b);
        public IEnumerable<IPlug> AllPlugs { get { foreach (var item in pairings) { yield return item.a; yield return item.b; } } }

        internal static Linkage FromGroups(PlugGroup a, PlugGroup b) {
            var l = new Linkage();
            for (var i = 0; i < a.plugs.Count; i++) {
                l.pairings.Add((a.plugs[i], b.plugs[i]));
            }
            return l;
        }

        internal static Linkage FromCollections(IEnumerable<IPlug> a, IEnumerable<IPlug> b) {
            var l = new Linkage();
            using (var e1 = a.GetEnumerator()) 
            using (var e2 = b.GetEnumerator())
                while (e1.MoveNext() && e2.MoveNext()) l.pairings.Add((e1.Current, e2.Current));
                
            return l;
        }
    }

    public interface IPlug {
        Module Module { get; }
        string Tag { get; }
        int    GroupID { get; }
        Linkage ActiveContact { get; set; }
        Pose    RelativePose { get; }
        Polarities Polarity { get; }
        string Name { get; }
        int SymmetryGroup { get; }
    }
}
