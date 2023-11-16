using System;
using System.Collections.Generic;
using System.Linq;
using K3;
using UnityEngine;

namespace Scanner.Megaship {
    // a Point Linkable
    internal class Plug : MonoBehaviour, IPlug {
        [field:SerializeField] public Polarities Polarity { get; private set; }
        [field:SerializeField] public string Tag { get; private set; }
        public Module Module { get { module = module != null ? module : GetComponentInParent<Module>(); return module; } }

        Module module;

        [field:SerializeField] [field:Range(0, 6)] public int GroupID { get; private set; }

        public Pose RelativePose => PoseUtility.GetRelativePose(Module.transform.WorldPose(), transform.WorldPose());

        public Linkage ActiveContact { get; set; }

        void Awake() {
            module = GetComponentInParent<Module>();
        }

        public string Name => name;

        private void OnDrawGizmosSelected() {
           
            Gizmos.color = Color.yellow;

            if (Application.isPlaying && ActiveContact != null) {
                var c = Color.green;
                c.a = 0.5f;
                Gizmos.color = c;
            }

            Gizmos.matrix = transform.localToWorldMatrix;

            if (Polarity == Polarities.Male) {
                Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.2f); 
            } else if (Polarity== Polarities.Female) {
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 0.3f);
            } else if (Polarity == Polarities.TwoWay) {

                if (Application.isPlaying && ActiveContact != null) {
                    var onASide = ActiveContact.ASidePlugs.Contains(this);
                    if (onASide) Gizmos.DrawWireSphere(Vector3.zero, 0.25f);
                    else         Gizmos.DrawSphere(Vector3.zero, 0.25f);
                } else {
                    Gizmos.DrawSphere(Vector3.zero, 0.22f);
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    class InjectModificationRuleAttribute : Attribute {

    }

    
    internal interface IModificationRuleInjector {
        IEnumerable<ModificationOpportunity> Inject(Ship ship, LinkQueryContext context);
    }

    // [InjectModificationRule]
    public class SpineExtension : IModificationRuleInjector {

        IEnumerable<ModificationOpportunity> IModificationRuleInjector.Inject(Ship ship, LinkQueryContext context) {
            // list all dangling link groups with the flavour "spine"

            var collection = context.phantomModules;
            if (context.explicitModule != null) collection = new List<Module>() { context.explicitModule };
            foreach (var pmodule in collection) {
                var matches = MatchingUtility.FindPossibleMatches(
                    ModuleUtilities.ListUnoccupiedPlugs(ship).Where(p => p.Tag == "spine-ext"), 
                    ModuleUtilities.ListUnoccupiedPlugs(pmodule).Where(p => p.Tag == "spine-ext")
                )
                .ToArray()
                ;

                foreach (var match in matches) {
                    // if (MatchingUtility.AllPlugsContainTag(match, "spine-ext")) {
                        yield return new BuildAndAttachOpportunity() {
                            name = "Extend spine",
                            orientation = 0,
                            symmetry = 0,
                            targetContact = match, 
                            type = OpportunityTypes.OfferTweak,
                        };
                    // }
                }
            }

            yield break;
        }
    }

    [InjectModificationRule]
    public class SpineAttachment : IModificationRuleInjector {
        IEnumerable<ModificationOpportunity> IModificationRuleInjector.Inject(Ship ship, LinkQueryContext context) {
            var collection = context.phantomModules;
            if (context.explicitModule != null) collection = new List<Module>() { context.explicitModule };
            foreach (var pmodule in collection) {
                var matches = MatchingUtility.FindPossibleMatches(
                    ModuleUtilities.ListUnoccupiedPlugs(ship).Where(p => p.Tag == "spine-attach"), 
                    ModuleUtilities.ListUnoccupiedPlugs(pmodule).Where(p => p.Tag == "spine-attach")
                )
                .ToArray()
                ;

                foreach (var match in matches) {
                    // if (MatchingUtility.AllPlugsContainTag(match, "spine-ext")) {
                        yield return new BuildAndAttachOpportunity() {
                            name = $"Attach to {MatchingUtility.ShipboardModuleOf(match)}",
                            orientation = 0,
                            symmetry = 0,
                            targetContact = match, 
                            type = OpportunityTypes.OfferTweak,
                        };
                    // }
                }
            }

            yield break;
        }
        
    }


    class PlugGroup {
        public List<IPlug> plugs = new();
    }

    internal class LinkQueryContext {
        public int concreteSymmetry = 1;
        public List<Module> phantomModules;
        public Module explicitModule;
    }

    public enum OpportunityTypes {
        Hidden,
        OfferTweak,
        ConstructFromMenu,
    }

    public abstract class ModificationOpportunity {
        public string name;

        public virtual string Print() => "";
    }
}
