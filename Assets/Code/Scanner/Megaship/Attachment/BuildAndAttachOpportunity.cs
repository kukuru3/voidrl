using System.Linq;
using System.Text;

namespace Scanner.Megaship {

    public class BuildAndAttachOpportunity : ModificationOpportunity {
        internal Module phantomModule;
        internal Linkage targetContact;

        public override string Print() {
            var sb = new StringBuilder();
            sb.Append("Build/Attach [");
            foreach (var (a, b) in targetContact.pairings) { 
                sb.Append(DisplayHelper.PrettyPrintPairing(a, b));
                sb.Append(',');
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(']');
            return sb.ToString();
        }

        internal BuildAndAttachOpportunity WithModuleTransfer(Module to) {
            var opp = new BuildAndAttachOpportunity {
                name = this.name,
                targetContact = this.targetContact,
                phantomModule = to,
            };

            opp.targetContact = new Linkage() {
                pairings = opp.targetContact.pairings.Select(p => { 
                    var newA = p.a;
                    if (p.a.Module.IsPhantom) newA = ModuleUtilities.FindSurrogate(p.a, p.a.Module, to);
                    var newB = p.b;
                    if (p.b.Module.IsPhantom) newB = ModuleUtilities.FindSurrogate(p.b, p.b.Module, to);
                    return (newA, newB);
                }).ToList()
            };

            return opp;
        }
    }

    public static class DisplayHelper {
        public static string PrettyPrintPairing(IPlug a, IPlug b) {
            // if (a.Module.IsPhantom) (a,b) = (b,a);
            return $"{b.Name} <- {a.Name}";
        }

        static string PrintModule(Module m) {
            if (m.IsPhantom) return $"PHANTOM {m.Name}";
            else return $"Ship's {m.Name}";
        }
    }
}
