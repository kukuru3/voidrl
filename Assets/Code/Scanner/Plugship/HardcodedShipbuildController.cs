using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanner.Plugship {

    public interface IShipbuildingContext {
        public enum UIStates {
            Tweaks, // chilling, looking at tweaks
            ActionSelect, //we have yet to select an action offered by the tweak.
            ActionConfirm // additional step?
        }


        UIStates UIState { get; }

        void GenerateUISelectionForAttachment(IEnumerable<PotentialAttachment> attachments);
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

        [SerializeField] Module[] modulePrefabs;
        [SerializeField] internal TweakHandle[] tweakHandlePrefabs;
        [SerializeField] internal Button phantomBuildButtonPrefab;
        [SerializeField] Button confirmButton;
        [SerializeField] Button cancelButton;
        [SerializeField] Transform buildUIcontainerObject;
        [SerializeField] Material hologramMaterial;

        public IShipBuilder Builder { get; private set; }
        List<Module> templateInstances = new List<Module>();

        GameObject activeGhost;
        List<GameObject> maintainedBuildUI = new List<GameObject>();

        IShipbuildingContext.UIStates uistate;
        public IShipbuildingContext.UIStates UIState { get => uistate; private set { uistate = value;  } }

        private void UpdateElementVisibility() {
            if (uistate == IShipbuildingContext.UIStates.Tweaks) ReplaceActiveBuildGhost(null);
            foreach (var go in maintainedBuildUI) go.SetActive(UIState == IShipbuildingContext.UIStates.ActionSelect);
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

        Module GenerateTemplate(Module prefab) {
            var m = Instantiate(prefab, transform);
            m.name = $"PHANTOM: [{prefab.name}]";
            m.transform.localPosition = new Vector3(1000, 0, 0);
            
            foreach (var mc in m.gameObject.GetComponentsInChildren<MeshRenderer>()) { mc.sharedMaterial = hologramMaterial; }
            return m;
        }

        public void GenerateUISelectionForAttachment(IEnumerable<PotentialAttachment> attachments) { 
            UIState = IShipbuildingContext.UIStates.ActionSelect;

            foreach (var obj in maintainedBuildUI) Destroy(obj);
            maintainedBuildUI.Clear();

            int counter = 0;

            foreach (var att in attachments) {
                var btnGO = Instantiate(phantomBuildButtonPrefab, buildUIcontainerObject);
                var btn = btnGO.GetComponentInChildren<Button>();
                btn.transform.localPosition = Vector3.down * counter++ * 50;
                   
                btn.Clicked += () => ProcessAttachmentPreview(att);
                foreach (var lbl in btn.GetComponentsInChildren<TMPro.TMP_Text>(true)) lbl.text = $"Construct {att.phantom.Name}";
            }
        }

        private void ProcessAttachmentPreview(PotentialAttachment att) {
            UIState = IShipbuildingContext.UIStates.ActionConfirm;            
            ReplaceActiveBuildGhost(att.phantom);
            var activeGhostModule = activeGhost.GetComponent<Module>();
            VisualUtils.AssignGhostShader(activeGhost.GetComponentInChildren<Module>());
            Builder.PositionModuleForPlugInterface(activeGhostModule.AllPlugs[att.indexOfPlugInPhantomList], att.shipPlug);

        }

        private void ReplaceActiveBuildGhost(Module template) {
            if (activeGhost != null) GameObject.Destroy(activeGhost);
            activeGhost = null;
            if (template != null) { 
                activeGhost = GameObject.Instantiate(template.gameObject, transform);
                activeGhost.name = $"ACTIVE GHOST: {template.Name}";
            }
        }

        int moduleID = 0;

        Module GenerateModule(string name) {
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
            Builder.InsertModuleWithoutPlugs(GenerateModule("SpineSegment"));

            confirmButton.Clicked += () => ConfirmCurrentOption();
            cancelButton.Clicked += () => CancelCurrentOption();
        }

        private void CancelCurrentOption() {
            UIState = IShipbuildingContext.UIStates.Tweaks;
        }

        private void ConfirmCurrentOption() {

        }
    }
}
