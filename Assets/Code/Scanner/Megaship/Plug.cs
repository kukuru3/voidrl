using System;
using System.Collections.Generic;
using System.Linq;
using K3;
using UnityEngine;

namespace Scanner.Megaship {

    // a Point Linkable
    internal class Plug : MonoBehaviour, IPlug {

        public override string ToString() {
            if (module == null) return $"[ORPHAN]:{Name}";
            if (module.Ship == null) {
                return $"[PHANTOM]:{Name}";
            } else {
                return $"{module.Name}:{Name}";
            }
        }

        [field:SerializeField] public Polarities Polarity { get; private set; }
        [field:SerializeField] public string Tag { get; private set; }
        
        public Module Module { get { module = module != null ? module : GetComponentInParent<Module>(); return module; } }

        Module module;
        private Linkage activeContact;

        [field:SerializeField] [field:Range(0, 6)] public int GroupID { get; private set; }

        [field:SerializeField] [field:Range(0, 6)] public int SymmetryGroup { get; private set; }

        public Pose RelativePose => PoseUtility.GetRelativePose(Module.transform.WorldPose(), transform.WorldPose());

        public Linkage ActiveContact { get => activeContact; set { if (value == activeContact) return; activeContact = value; PropagateContactChange(); } }

        private void PropagateContactChange() {
            foreach (var responder in GetComponents<IContactProcessor>()) {
                responder.OnContactChanged(ActiveContact);
            }
        }

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

    interface IContactProcessor {
        void OnContactChanged(Linkage activeContact);
    }

    [AttributeUsage(AttributeTargets.Class)]
    class InjectModificationRuleAttribute : Attribute {

    }

    [AttributeUsage(AttributeTargets.Class)]
    class PostprocessShipAttribute : Attribute {

    }

    internal interface IShipPostprocessor {
        void Postprocess(Ship ship);
    }
    
    internal interface IModificationRuleInjector {
        IEnumerable<ModificationOpportunity> Inject(Ship ship, LinkQueryContext context);
    }

    static class ModificationBuilders {
        //public static BuildAndAttachOpportunity ConstructBuildModificationObject(string actionName, Linkage targetContact, Module phantomModule) {
            
        //}
    }

    //[InjectModificationRule]
    //public class SpineExtension : IModificationRuleInjector {

    //    IEnumerable<ModificationOpportunity> IModificationRuleInjector.Inject(Ship ship, LinkQueryContext context) {
    //        // list all dangling link groups with the flavour "spine"

    //        var collection = context.phantomModules;
    //        if (context.explicitModule != null) collection = new List<Module>() { context.explicitModule };
    //        foreach (var pmodule in collection) {
    //            var matches = MatchingUtility.FindPossibleMatches(
    //                ModuleUtilities.ListUnoccupiedPlugs(ship).Where(p => p.Tag == "spine-ext"), 
    //                ModuleUtilities.ListUnoccupiedPlugs(pmodule).Where(p => p.Tag == "spine-ext")
    //            )
    //            .ToArray()
    //            ;

    //            foreach (var match in matches) {
    //                // if (MatchingUtility.AllPlugsContainTag(match, "spine-ext")) {
    //                    yield return new BuildAndAttachOpportunity() {
    //                        name = "Extend spine",
    //                        targetContact = match,
    //                        phantomModule = pmodule,
    //                    };
    //                // }
    //            }
    //        }

    //        yield break;
    //    }
    //}

    [InjectModificationRule]
    public class DestroyModule : IModificationRuleInjector {
        IEnumerable<ModificationOpportunity> IModificationRuleInjector.Inject(Ship ship, LinkQueryContext context) {
            // find all the modules that are "leaf" modules. Since the ship is a graph, "leaf" modules are
            // all modules that would not result in the graph being split into two subgraphs.

            // but for our purposes, we will be using a simpler definition: a leaf module is one that ONLY has male-type plugs active.

            foreach (var module in ship.AllShipModules()) {
                if (!ModuleUtilities.ListAllPlugs(module).Any(IsPlugProhibitivelyImportant)) {
                    yield return new DestroyModuleOpportunity() {
                        name = $"Destroy {module.Name}",
                        targetModule = module,
                    };
                }
            }
        }

        bool IsPlugProhibitivelyImportant(IPlug plug) => plug.Polarity != Polarities.Male && plug.ActiveContact != null;
    }

    [InjectModificationRule]
    public class AttachViaPlugSpatialFit : IModificationRuleInjector {
        IEnumerable<ModificationOpportunity> IModificationRuleInjector.Inject(Ship ship, LinkQueryContext context) {
            var collection = context.phantomModules;
            if (context.explicitModule != null) collection = new List<Module>() { context.explicitModule };
            foreach (var pmodule in collection) {
                var matches = MatchingUtility.FindPossibleMatches(
                    ModuleUtilities.ListUnoccupiedPlugs(ship), 
                    ModuleUtilities.ListUnoccupiedPlugs(pmodule) // .Where(p => p.Polarity == Polarities.Male)
                )
                .ToArray()
                ;

                foreach (var match in matches) {
                    string n = "";
                    try { 
                        n = $"{MatchingUtility.AttachedModuleOf(match).Name} to {MatchingUtility.ShipboardModuleOf(match).Name}";
                    } catch (NullReferenceException e) {
                        Debug.LogError(e);
                    }

                    yield return new BuildAndAttachOpportunity() {
                        name = n,
                        targetContact = match, 
                        phantomModule = pmodule,
                    };
                    // }
                }
            }

            yield break;
        }

    }

    //[InjectModificationRule]
    //public class FacilityConstruction : IModificationRuleInjector {
    //    IEnumerable<ModificationOpportunity> IModificationRuleInjector.Inject(Ship ship, LinkQueryContext context) {
    //        var collection = context.phantomModules;
    //        if (context.explicitModule != null) collection = new List<Module>() { context.explicitModule };
    //        foreach (var pmodule in collection) {
    //            var matches = MatchingUtility.FindPossibleMatches(
    //                ModuleUtilities.ListUnoccupiedPlugs(ship).Where(p => p.Tag == "ring"), 
    //                ModuleUtilities.ListUnoccupiedPlugs(pmodule).Where(p => p.Tag == "ring")
    //            )
    //            .ToArray()
    //            ;

    //            foreach (var match in matches) {
    //                string n = "";
    //                try { 
    //                    n = $"{MatchingUtility.AttachedModuleOf(match).Name} to {MatchingUtility.ShipboardModuleOf(match).Name}";
    //                } catch (NullReferenceException e) {
    //                    Debug.LogError(e);
    //                }

    //                yield return new BuildAndAttachOpportunity() {
    //                        name = $"Build facility {pmodule.Name} at {match.BSidePlugs.First().Name}",
    //                        targetContact = match, 
    //                        phantomModule = pmodule,
    //                    };
    //                // }
    //            }
    //        }

    //        yield break;
    //    }
        
    //}


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
