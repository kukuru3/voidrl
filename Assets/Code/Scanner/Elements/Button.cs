using System;
using UnityEngine;

namespace OldScanner {

    abstract class Element : MonoBehaviour {
        public bool IsHighlighted { get; private set; }
        public Vector2 LastCursorLocalPos { get; internal set; }

        protected internal virtual void OnLostHilite() {
            this.IsHighlighted = false;
        }

        protected internal virtual void OnGainedHilite() {
            this.IsHighlighted = true;
        }
    }

    class Button : Element {
        [SerializeField] GameObject regular;
        [SerializeField] GameObject active;

        int framesHL;

        private void LateUpdate() {
            var vstate = VisualState();
            regular.SetActive(!vstate);
            active.SetActive(vstate);

            if (IsHighlighted) framesHL++; else framesHL = 0;
            
            if (IsHighlighted && Input.GetMouseButtonDown(0)) {
                Click();
            }
        }

        float timeLastClick = -1000f;

        private void Click() {
            timeLastClick = Time.time;
        }

        private bool VisualState() {

            if (timeLastClick > Time.time - 0.3f) {
                return Time.frameCount % 4 < 2;
            }

            if (IsHighlighted) {
                if (framesHL < 7) {
                    return framesHL % 2 == 0;
                }
                return true;
            }
            return false;
        }
    }
}