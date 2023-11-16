using System;
using System.Collections.Generic;
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
        private ShipModificationController ctrlr;

        private void Awake() {
            ctrlr = GetComponent<ShipModificationController>();
            btnCancel.Clicked += OnCancelClicked;
            btnApply.Clicked += OnApplyClicked;
        }

        private void OnApplyClicked() {
            ctrlr.ConcretizeCurrentModification();
        }

        private void OnCancelClicked() {
            ctrlr.ClearTentativeModification();
        }

        public void RegenerateModificationButtons(IEnumerable<ModificationOpportunity> modifications) {
            foreach (Transform child in modificationsHolder) Destroy(child.gameObject);

            int i = 0; 
            foreach (var mod in modifications) {
                var btn = GenerateButton(modificationsHolder, mod.name);
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

        private Button GenerateButton(Transform parent, string name) {
            var go = Instantiate(genericButton, parent);
            var btn = go.GetComponent<Button>();
            foreach (var lbl in btn.GetComponentsInChildren<TMPro.TMP_Text>(true)) lbl.text = $"{name}";
            return btn;
        }
    }
}
