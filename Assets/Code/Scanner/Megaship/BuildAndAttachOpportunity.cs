using System.Text;
using UnityEngine;

namespace Scanner.Megaship {
    public class BuildAndAttachOpportunity : ModificationOpportunity {
        internal OpportunityTypes type;
        internal Linkage targetContact;
        internal int symmetry;
        internal int orientation; // bonus, can be anything.

        public override string Print() {
            var sb = new StringBuilder();
            sb.Append("Build/Attach;");
            foreach (var (a, b) in targetContact.pairings) {
                sb.Append('(');
                sb.Append(DisplayHelper.PrettyPrintPairing(a, b));
                sb.Append(')');
            }
            return sb.ToString();
        }
    }

    public static class DisplayHelper {
        public static string PrettyPrintPairing(IPlug a, IPlug b) {
            // if (a.Module.IsPhantom) (a,b) = (b,a);
            return $"Slot {PrintModule(b.Module)}:{b.Name} => {PrintModule(a.Module)}:{a.Name}";
        }

        static string PrintModule(Module m) {
            if (m.IsPhantom) return $"PHANTOM {m.Name}";
            else return $"Ship's {m.Name}";
        }
    }
}
