using System.Collections.Generic;
using System.Linq;
using K3;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Scanner.Megaship {

    // includes the UI
    internal class ShipModificationController : MonoBehaviour {
        [SerializeField] Module[] modulePrefabs;
        [SerializeField] Transform shipRoot;
        [SerializeField] Transform phantomsRoot;

        [SerializeField] ShipModificationUI ui;

        [SerializeField] Material hologramMaterial;

        private Module[] phantomModules;
        private IList<IModificationRuleInjector> ruleInjectors;
        private List<IShipPostprocessor> postProcessors;

        private void Start() {
            // GeneratePhantoms();
            CollectPhantomsFromTemplate();
            CollectRules();
            CollectProcessors();
            Debug.Log($"Collected {ruleInjectors.Count} rules and {postProcessors.Count} processors");
            GenerateInitialShip();
            HandleShipChanged();
        }

        private void CollectPhantomsFromTemplate() {
            phantomModules = phantomsRoot.GetComponentsInChildren<Module>(true);
            foreach (var pm in phantomModules) {
                pm.gameObject.SetActive(false);
                pm.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }
        
        private void GeneratePhantoms() { 
            phantomModules = modulePrefabs.Select(GeneratePhantomInstance).ToArray();
        }

        private void CollectRules() {
            ruleInjectors = K3.ReflectionUtility.AutoinstantiateTypesMarkedWithAttribute<IModificationRuleInjector, InjectModificationRuleAttribute>();
        }

        private void CollectProcessors() {
            postProcessors = K3.ReflectionUtility.AutoinstantiateTypesMarkedWithAttribute<IShipPostprocessor, PostprocessShipAttribute>();
        }


        Module GeneratePhantomInstance(Module prefab) {
            var go = Instantiate(prefab.gameObject, transform);
            go.SetActive(false);
            return go.GetComponent<Module>();
        }

        public Ship Ship { get; set; }

        void GenerateInitialShip() {
            Ship = shipRoot.GetComponent<Ship>();
            var spine = GenerateModule("spine");
            Ship.AddRootModule(spine);
            spine.Ship = Ship;
            HandleShipChanged();
        }

        private void HandleShipChanged() {
            foreach (var pp in postProcessors) pp.Postprocess(Ship);
            var opportunities = RegenerateModifications();
            ui.RegenerateModificationButtons(opportunities);
            ui.UpdateVolatileState();
        }

        public Module GenerateModule(string name) {
            var sourceModule = phantomModules.First(m => m.Name.ToUpperInvariant() == name.ToUpperInvariant());
            var copy = Instantiate(sourceModule.gameObject, shipRoot).GetComponent<Module>();
            copy.gameObject.name = copy.Name;
            copy.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            copy.gameObject.SetActive(true);
            return copy;
        }

        public IList<ModificationOpportunity> RegenerateModifications() {
            var lqc = new LinkQueryContext() {
                concreteSymmetry = 1,
                explicitModule = null,
                phantomModules = this.phantomModules.ToList(),
            };

            List<ModificationOpportunity> opportunities = new();

            foreach (var injector in ruleInjectors) {
                var ops = injector.Inject(Ship, lqc);
                foreach (var op in ops) {
                    // Debug.Log($"Injector {injector.GetType().Name} injected opportunity: {op.name} ; {op.Print()}");
                    opportunities.Add(op);
                }
            }

            ConcatenateOpportunitiesList(opportunities);

            return opportunities;
        }

        private void ConcatenateOpportunitiesList(List<ModificationOpportunity> opportunities) {

            // Find attachment opportunities that would attach:
            // - an identical module
            // - to the plugs of the same symmetry group
            // - the plugs having an identical SYMMETRY PATH (this is going to be an issue...)

            foreach (var opportunity in opportunities) {
                if (opportunity is BuildAndAttachOpportunity bao) {
                    var sp = bao.targetContact.AllShipboardPlugs();
                    if (sp.Count() == 1) { 
                        if (sp.First().SymmetryGroup > 0) {

                        }
                    }
                }
            }
        }

        GameObject hologramModuleObject;

        void ClearHologram() {
            if (hologramModuleObject != null) Destroy(hologramModuleObject); hologramModuleObject = null;
        }

        public Module CreateHologramModule(Module phantom) {
            ClearHologram();
            hologramModuleObject = Instantiate(phantom.gameObject, shipRoot);
            hologramModuleObject.SetActive(true);
            hologramModuleObject.name = phantom.Name;
            foreach (var mc in hologramModuleObject.GetComponentsInChildren<MeshRenderer>()) { 
                mc.sharedMaterial = hologramMaterial; 
            }
            return hologramModuleObject.GetComponent<Module>();
        }

        internal void PositionModuleToObeyLinkage(Module toPosition, Linkage targetContact) {
            var pairing = targetContact.pairings.First();
            IPlug shipPlug = pairing.a;
            IPlug attachedPlug = pairing.b;
            if (pairing.a.Module.IsPhantom) {
                shipPlug = pairing.b; attachedPlug = pairing.a;
            }

            var worldPoseOfShipPlug = (shipPlug as Component).transform.WorldPose();
            var worldPoseOfModuleSoThatPlugsCoincide = worldPoseOfShipPlug.Mul(attachedPlug.RelativePose.Inverse());
            // Debug.Log($"sp = {shipPlug}; attP={attachedPlug}; WP = {worldPoseOfModuleSoThatPlugsCoincide.Pretty()}, wpsp = {worldPoseOfShipPlug.Pretty()}, aprp = {attachedPlug.RelativePose.Pretty()}");
            toPosition.transform.AssumePose(worldPoseOfModuleSoThatPlugsCoincide);
        }

        public ModificationOpportunity CurrentModification { get; private set; }



        internal void ClearTentativeModification() {
            CurrentModification = null;
        }

        internal void ConcretizeCurrentModification() {
            if (CurrentModification is BuildAndAttachOpportunity attach) {

                var holoModule = hologramModuleObject.GetComponent<Module>();
                var concreteModule = GenerateModule(holoModule.Name);
                var concreteAttachment = attach.WithModuleTransfer(concreteModule);

                // since this hinges on being able to determine which module is the phantom module,
                // and we determine phantoms by them not being associated with a Ship,
                // this means that concreteModule.Ship MUST NOT be assigned at this point, but rather
                // later on, near the end of the process.
                PositionModuleToObeyLinkage(concreteModule, concreteAttachment.targetContact);

                ConcretizeLinkage(concreteAttachment.targetContact);
                Ship.AddLinkage(concreteAttachment.targetContact);
                concreteModule.Ship = this.Ship;
                
            } else if (CurrentModification is DestroyModuleOpportunity destroi) {
                DestroyModule(destroi.targetModule);
            }
            FinalizeModification();
        }

        private void FinalizeModification() {
            CurrentModification = null;
            HandleShipChanged();
            ClearHologram();
            ui.UpdateVolatileState();
        }

        private void ConcretizeLinkage(Linkage targetContact) {
            foreach (var pairing in targetContact.pairings) {
                pairing.a.ActiveContact = targetContact;
                pairing.b.ActiveContact = targetContact;
            }
        }

        internal void GenerateAttachableHologram(BuildAndAttachOpportunity attach) {
            var hologram = CreateHologramModule(attach.phantomModule);
            attach = attach.WithModuleTransfer(hologram);
            PositionModuleToObeyLinkage(hologram, attach.targetContact);
            CurrentModification = attach;
            ui.UpdateVolatileState();
        }

        internal void DestroyModule(Module m) { 
            var contacts = ModuleUtilities.AllShipLinkagesOf(m).ToList();
            
            foreach (var link in contacts) {
                foreach (var pairing in link.pairings) {
                    pairing.a.ActiveContact = null;
                    pairing.b.ActiveContact = null;
                }
                Ship.RemoveContact(link);
            }

            if (Ship.IsRootModule(m)) Ship.RemoveRootModule(m);

            Destroy(m.gameObject);

            FinalizeModification();
        }

        internal void HandleModificationClicked(ModificationOpportunity mod) {
            if (mod is BuildAndAttachOpportunity attach) {
                GenerateAttachableHologram(attach);
            } else if (mod is DestroyModuleOpportunity dmod) {
                GenerateDestructionHologram(dmod);
            }
        }

        private void GenerateDestructionHologram(DestroyModuleOpportunity destruction) {
            CreateHologramModule(destruction.targetModule);
            
            var mrs = hologramModuleObject.GetComponentsInChildren<MeshRenderer>();
            var m = mrs.First().sharedMaterial;

            var copy = new Material(m);
            copy.color = new Color(1, 0, 0, 0.5f);

            foreach (var mr in mrs) mr.sharedMaterial = copy;
            CurrentModification = destruction;
            ui.UpdateVolatileState();
        }
    }
}
