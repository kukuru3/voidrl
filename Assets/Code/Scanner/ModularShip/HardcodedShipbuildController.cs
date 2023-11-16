using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanner.ModularShip {

    public interface IShipbuildingContext {
        public enum UIStates {
            Tweaks, // chilling, looking at tweaks
            ActionSelect, //we have yet to select an action offered by the tweak.
            ActionConfirm // additional step?
        }

        UIStates UIState { get; }

        void GenerateStructureButtons(IEnumerable<PotentialAttachment> attachments);
        void SelectActiveTweak(Tweak tweak);
    }

    public static class Context {

        static HardcodedShipbuildController _shipbuildctrlr;
        public static IShipbuildingContext ShipbuildingContext { get {
            _shipbuildctrlr ??= GameObject.FindObjectOfType<HardcodedShipbuildController>();
            return _shipbuildctrlr;
        } }
    }

    internal class HardcodedShipbuildController : MonoBehaviour, IShipbuildingContext {

        [SerializeField] OldModule[] modulePrefabs;
        [SerializeField] internal TweakHandle[] tweakHandlePrefabs;
        [SerializeField] internal Button phantomBuildButtonPrefab;
        [SerializeField] Button confirmButton;
        [SerializeField] Button cancelButton;
        [SerializeField] Transform buildUIcontainerObject;
        [SerializeField] Material hologramMaterial;

        public IShipBuilder Builder { get; private set; }
        List<OldModule> templateInstances = new List<OldModule>();

        GameObject activeGhost;
        List<GameObject> maintainedBuildUI = new List<GameObject>();

        IShipbuildingContext.UIStates uistate;
        public IShipbuildingContext.UIStates UIState { get => uistate; private set { uistate = value;  } }

        private void UpdateElementVisibility() {
            if (uistate == IShipbuildingContext.UIStates.Tweaks) ReplaceActiveBuildGhost(null);
            if (maintainedBuildUI.Count > 1) {
                foreach (var go in maintainedBuildUI) go.SetActive(UIState != IShipbuildingContext.UIStates.Tweaks);
            } else {
                foreach (var go in maintainedBuildUI) go.SetActive(UIState == IShipbuildingContext.UIStates.ActionSelect);
            }
            confirmButton.gameObject.SetActive(UIState == IShipbuildingContext.UIStates.ActionConfirm);
            cancelButton.gameObject.SetActive(UIState != IShipbuildingContext.UIStates.Tweaks);
            Builder.ApplyUIMode(uistate);
        }

        void LateUpdate() { 
            UpdateElementVisibility();
        }

        public void SelectActiveTweak(Tweak tweak) {
            Builder.ActiveTweak = tweak;
        }


        Transform templateHolder;

        OldModule GenerateTemplate(OldModule prefab) {
            var m = Instantiate(prefab, transform);
            m.name = $"PHANTOM: [{prefab.name}]";
            m.transform.localPosition = new Vector3(1000, 0, 0);
            
            foreach (var mc in m.gameObject.GetComponentsInChildren<MeshRenderer>()) { mc.sharedMaterial = hologramMaterial; }
            return m;
        }

        public Action ExecutionDelegate;

        // when clicking on a tweak, this generates the buttons of all the structures you can build on that tweak.

        public void GenerateStructureButtons(IEnumerable<PotentialAttachment> attachments) { 
            UIState = IShipbuildingContext.UIStates.ActionSelect;

            foreach (var obj in maintainedBuildUI) Destroy(obj); 
            maintainedBuildUI.Clear();

            int counter = 0;

            foreach (var att in attachments) {
                var btn = Instantiate(phantomBuildButtonPrefab, buildUIcontainerObject);
                btn.transform.localPosition = 50 * counter++ * Vector3.down;
                   
                btn.Clicked += () => ActionPreview_ConstructStructure(att);
                maintainedBuildUI.Add(btn.gameObject);
                foreach (var lbl in btn.GetComponentsInChildren<TMPro.TMP_Text>(true)) lbl.text = $"{att.phantom.Name}";
            }

            ActionPreview_ConstructStructure(attachments.First());
        }

        private void ActionPreview_ConstructStructure(PotentialAttachment directive) {
            UIState = IShipbuildingContext.UIStates.ActionConfirm;            
            ReplaceActiveBuildGhost(directive.phantom);
            var activeGhostModule = activeGhost.GetComponent<OldModule>();
            VisualUtils.AssignGhostShader(activeGhost.GetComponentInChildren<OldModule>());
            Builder.PositionModuleForPlugInterface(activeGhostModule.AllPlugs[directive.indexOfPlugInPhantomList], directive.shipPlug);
            ExecutionDelegate = () => {
                var newModule = GenerateModule(directive.phantom.Name);

                Builder.PositionModuleForPlugInterface(newModule.AllPlugs[directive.indexOfPlugInPhantomList], directive.shipPlug);
                Builder.Connect(directive.shipPlug, newModule.AllPlugs[directive.indexOfPlugInPhantomList]);
                Builder.ActiveTweak = null;
                UIState = IShipbuildingContext.UIStates.Tweaks;
            };
        }

        private void ReplaceActiveBuildGhost(OldModule template) {
            if (activeGhost != null) GameObject.Destroy(activeGhost);
            activeGhost = null;
            if (template != null) { 
                activeGhost = GameObject.Instantiate(template.gameObject, transform);
                activeGhost.name = $"ACTIVE GHOST: {template.Name}";
            }
        }

        int moduleID = 0;

        OldModule GenerateModule(string name) {
            foreach (var p in modulePrefabs) if (p?.name == name || p.Name == name) { 
                var m = Instantiate(p, transform);
                m.gameObject.name = $"MODULE: {m.Name} [{++moduleID}]";
                return m;
            }
            throw new KeyNotFoundException($"Module by name of `{name}` not known"); 
        }

        private void Start() {
            Builder = GetComponentInChildren<IShipBuilder>();
            templateInstances = modulePrefabs.Select(GenerateTemplate).ToList();
            templateHolder = new GameObject("Templates").transform;
            templateHolder.transform.parent = transform;
            foreach (var i in templateInstances) i.transform.parent = templateHolder;
            Builder.RegisterTemplates(templateInstances);
            Builder.InsertModuleWithoutPlugs(GenerateModule("Spine Cylinder"));

            confirmButton.Clicked += () => ConfirmCurrentOption();
            cancelButton.Clicked += () => CancelCurrentOption();
        }

        private void CancelCurrentOption() {
            UIState = IShipbuildingContext.UIStates.Tweaks;
        }

        private void ConfirmCurrentOption() {
            ExecutionDelegate?.Invoke();
        }
    }
}
