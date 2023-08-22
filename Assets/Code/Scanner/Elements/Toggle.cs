using System;
using UnityEngine;

namespace Scanner {
    class Toggle : Element {
        [SerializeField] GameObject[] checkmarks;
        [SerializeField] GameObject activeCheckbox;
        [SerializeField] GameObject inactiveCheckbox;

        [SerializeField] GameObject textObject;

        public bool ToggleState { get; set; }
        int framesHL;
        int framesChecked;
        int framesSinceCheckStateChanged;
        private void LateUpdate() {
            framesSinceCheckStateChanged++;
            if (IsHighlighted) framesHL++; else framesHL = 0;
            if (ToggleState) framesChecked++; else framesChecked = 0;
            if (IsHighlighted && Input.GetMouseButtonDown(0)) {
                Click();
            }

            var showText = true;
            var showActive = IsHighlighted;

            if (IsHighlighted && framesHL < 8) {
                showText = framesHL % 2 == 0;
                showActive = framesHL % 2 == 0;
            }

            textObject.SetActive(showText);

            var showCheck = ToggleState;
            

            if (framesSinceCheckStateChanged < 20) {
                showActive = framesSinceCheckStateChanged % 4 < 2;
            }

            foreach (var checkmark in checkmarks) {
                checkmark.SetActive(showCheck);
            }

            activeCheckbox.SetActive(showActive);
            inactiveCheckbox.SetActive(!showActive);
        }

        private void Click() {
            ToggleState = !ToggleState;
            framesSinceCheckStateChanged = 0;
            ValueChanged?.Invoke();
        }

        internal void SetCaption(string name) => this.textObject.GetComponent<TMPro.TMP_Text>().text = name;

        public event Action ValueChanged;
    }
}