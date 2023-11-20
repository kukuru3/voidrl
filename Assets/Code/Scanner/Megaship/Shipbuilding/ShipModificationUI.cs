using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using K3;
using Scanner.ScannerView;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Graphs;
using UnityEngine;

namespace Scanner.Megaship {
    internal class ShipModificationUI : MonoBehaviour {
        [Header("Transforms")]
        [SerializeField] Transform modificationsHolder;
        [SerializeField] Transform buildableButtonsHolder;

        [Header("Prefabs")]
        [SerializeField] GameObject genericButton;

        [Header("Object references")]
        [SerializeField] Button btnApply;
        [SerializeField] Button btnCancel;
        [SerializeField] Camera builderCam3d;
        private ShipModificationController ctrlr;

        private void Awake() {
            ctrlr = GetComponent<ShipModificationController>();
            btnCancel.Clicked += OnCancelClicked;
            btnApply.Clicked += OnApplyClicked;
            ctrlr.OnShipChanged += HandleShipChanged;
        }

        private void Start() {
            GenerateConstructionUI();
        }

        private void GenerateConstructionUI() {
            int i = 0;
            foreach (var template in ctrlr.Templates) {
                var btn = GenerateGenericButton(modificationsHolder, template.Name);
                btn.transform.Translate(0, - 32f * i++, 0);
                var t = template;
                btn.Clicked += () => SetActivePhantom(t);
            }
        }

        class TentativeModification {
            public Pose pose;
            public ModificationOpportunity opportunity;
        }

        Module selectedTemplate;
        List<TentativeModification> currentModPool = new();

        private void SetActivePhantom(Module template) {
            selectedTemplate = template;
            RegeneratePossibleActions();
        }

        private void RegeneratePossibleActions() {
            currentModPool.Clear();
            if (selectedTemplate == null) {
                ctrlr.ClearHologram();
                return;
            } 
            var listed = ctrlr.ListModificationsForTemplate(selectedTemplate);
            foreach (var m in listed) {
                if (m is BuildAndAttachOpportunity bao) {
                    currentModPool.Add(new TentativeModification { opportunity = bao, pose = GetPose(m) });
                }
            }
        }

        private Pose GetPose(ModificationOpportunity m) {
            if (m is BuildAndAttachOpportunity b) {
                var pose = ctrlr.ModulePoseToObeyLinkage(b.phantomModule, b.targetContact);
                return pose;
            } else if (m is DestroyModuleOpportunity d) {
                return d.targetModule.transform.WorldPose();
            }
            return default;
        }

        private void OnApplyClicked() {
            ctrlr.ConcretizeCurrentModification();
        }

        private void OnCancelClicked() {
            ctrlr.ClearTentativeModification();
        }

        private void Update() {
            UpdateConstructionUI();
            
            if (Input.GetMouseButtonDown(0)) {
                if (ctrlr.CurrentModification != null) {
                    ctrlr.ConcretizeCurrentModification();
                }
            }
        }

        public void RegenerateModificationButtons(IEnumerable<ModificationOpportunity> modifications) {
            foreach (Transform child in modificationsHolder) Destroy(child.gameObject);

            int i = 0; 
            foreach (var mod in modifications) {
                var btn = GenerateGenericButton(modificationsHolder, mod.name);
                btn.transform.Translate(0, - 32f * i++, 0);
                btn.Clicked += () => ctrlr.HandleModificationClicked(mod);
            }
        }

        internal void UpdateVolatileState() {
            if (ctrlr.CurrentModification == null) {
                btnApply.gameObject.SetActive(false);
                btnCancel.gameObject.SetActive(false);
            } else {
                btnApply.gameObject.SetActive(true);
                btnCancel.gameObject.SetActive(true);
            }
        }

        private Button GenerateGenericButton(Transform parent, string name) {
            var go = Instantiate(genericButton, parent);
            var btn = go.GetComponent<Button>();
            foreach (var lbl in btn.GetComponentsInChildren<TMPro.TMP_Text>(true)) lbl.text = $"{name}";
            return btn;
        }

        private void HandleShipChanged() {
            // old:
            //var opportunities = ctrlr.RegenerateAllPossibleModifications();
            //RegenerateModificationButtons(opportunities);
            //UpdateVolatileState();

            // new: 
            RegeneratePossibleActions();
        }

        void UpdateConstructionUI() {
            
            float bestDistance = float.MaxValue;
            TentativeModification bestCandidate = null;
            // find the most suitable match:
            foreach (var candidate in currentModPool) {

                if (candidate.opportunity is BuildAndAttachOpportunity attach) {
                    foreach (var massPointInModuleSpace in attach.phantomModule.MassComponent.AllMassPointsInModuleSpace()) {
                        var worldPosOfMassPointInTransformedModuleSpace = candidate.pose.TransformPoint(massPointInModuleSpace.localPosition);
                        var ssPos = builderCam3d.WorldToScreenPoint(worldPosOfMassPointInTransformedModuleSpace);
                        ssPos.z = 0;
                        var distance = Vector3.Distance(Input.mousePosition, ssPos);
                        if (distance < bestDistance) {
                            bestDistance = distance;
                            bestCandidate = candidate;
                        }
                    }
                }
            }
            if (bestCandidate != null && bestDistance < 120f) {
                if (bestCandidate.opportunity is BuildAndAttachOpportunity bop) {
                    var p = bop.targetContact.pairings[0];
                    ctrlr.GenerateAttachableHologram(bop);
                    //var hologram = ctrlr.CreateHologramModule(bop.phantomModule);
                    //var h = bop.WithModuleTransfer(hologram);
                    //ctrlr.PositionModuleToObeyLinkage(hologram, h.targetContact);
                }
            } else {
                ctrlr.ClearHologram();
            }
        }

        // create a button for each phantom module
        // clicking a button immediately requests generation of all possible contacts by the controller for the module 
        // every update, find the most suitable match for the generated requests.
        // there is a threshold for feasibility.

        // when a match is found, notify the controller to create the hologram as usual
        // update hologram position per frame (in case of rotating stuff)

        // deletion can be handled in a similar fashion, or we can just have module selection (but that would require colliders)

    }
}
