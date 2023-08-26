using System;
using System.ComponentModel.Composition.Primitives;
using K3;
using UnityEngine;

namespace Scanner {
    class Slider : Element {
        [SerializeField] float colliderFractionMargin;
        [SerializeField] Shapes.Line fillLine;
        [SerializeField] Shapes.Line masker;
        [SerializeField] GameObject pip;
        [SerializeField] TMPro.TMP_Text[] captions;

        [SerializeField] float pixelFrom;
        [SerializeField] float pixelTo;

        [SerializeField] float maxWidth;

        [SerializeField] Shapes.RegularPolygon[] innerPips;
        [SerializeField] Shapes.RegularPolygon[] outerPips;

        [SerializeField] float innerPipWidthNormal;
        [SerializeField] float outerPipWidthNormal;

        [SerializeField] float innerPipWidthActive;
        [SerializeField] float outerPipWidthActive;

        [SerializeField] float outerPipOffsetNormal;
        [SerializeField] float outerPipOffsetActive;
        [SerializeField] GameObject[] blinkers;

        public void SetCaption(string caption) {
            foreach (var c in captions) c.text = caption;
        }

        public void SetValueExternal(float t) {
            SetValue(t);
            UpdateVisuals();
        }
        private void LateUpdate() {
            if (IsHighlighted && Input.GetMouseButton(0)) {
                var t = LastCursorLocalPos.x + 1f;
                t /= 2;                
                t = t.Map(0f + colliderFractionMargin, 1f - colliderFractionMargin, 0f, 1f, true);
                SetValue(t);
            }
            UpdateVisuals();
        }

        private void UpdateVisuals() {
            if (IsHighlighted) framesHL++; else framesHL = 0;
            var draw = !IsBlink();
            foreach (var blinker in blinkers) blinker.SetActive(draw);
            fillLine.ColorStart = fillLine.ColorEnd = IsHighlighted ? Color.white * 1.25f : Color.white;
        }

        // float hiliteAlpha = 0f;

        protected internal override void OnGainedHilite() {
            base.OnGainedHilite();
            foreach (var pip in innerPips) { pip.Radius = innerPipWidthActive; }
            foreach (var pip in outerPips) { 
                pip.Radius = outerPipWidthActive;
                var lp = pip.transform.localPosition;
                lp.y = Mathf.Sign(lp.y) * outerPipOffsetActive;
                pip.transform.localPosition = lp;
            }
        }

        protected internal override void OnLostHilite() {
            base.OnLostHilite();
            foreach (var pip in innerPips) { pip.Radius = innerPipWidthNormal; }
            foreach (var pip in outerPips) { 
                pip.Radius = outerPipWidthNormal; 
                var lp = pip.transform.localPosition;
                lp.y = Mathf.Sign(lp.y) * outerPipOffsetNormal;
                pip.transform.localPosition = lp;
            }
        }

        public float Value { get; private set; }

        public event Action<float> ValueChanged;

        void SetValue(float q) {

            this.Value = q;
            this.ValueChanged?.Invoke(Value);
            var px = Mathf.Lerp(pixelFrom, pixelTo, q);

            //var pixel = Mathf.Lerp(marginLeewayPixels, maxWidth - marginLeewayPixels, q);
            //var value = pixel.Map(marginLeewayPixels, maxWidth - marginLeewayPixels, 0, 1, true);
            //this.Value = value;            
            masker.End = new Vector3(px, 0, 0);
            pip.transform.localPosition = new Vector3(px, 0, 0);

        }

        int framesHL;

        private bool IsBlink() {

            //if (timeLastClick > Time.time - 0.3f) {
            //    return Time.frameCount % 4 < 2;
            //}

            if (IsHighlighted) {
                if (framesHL < 11) {
                    return framesHL % 2 == 0;
                }
            }
            return false;
        }

    }
}