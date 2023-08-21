using System;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace Scanner.Sweeteners {
    internal class LinkLogic : MonoBehaviour {

        [SerializeField] Camera uicamera;
        private TMP_Text tmpro;

        private void Start() {
            tmpro = GetComponent<TMP_Text>();
        }
        private void LateUpdate() {
            CheckForLink();
            CheckForContinuousEffects();

            if (Input.GetMouseButtonDown(0)) {
                if (currentHoverLink.HasValue) {
                    var range = GetRangeFrom(currentHoverLink.Value);
                    var isVisibleFirst = tmpro.textInfo.characterInfo[range.Start].isVisible;
                    // var isVisibleLast  = tmpro.textInfo.characterInfo[range.End].isVisible;
                    if (isVisibleFirst) { 
                        var id = currentHoverLink.Value.GetLinkID();
                        Debug.Log($"CLICKED link id: {id}");
                        timeOfHover = Time.time;
                    }
                }
            }
        }

        private void CheckForContinuousEffects() {
            if (currentHoverLink.HasValue) {
                var t = Time.time - timeOfHover;
                if (t < 0.15f) {
                    var state = Time.frameCount % 4 < 2;
                    var range = GetRangeFrom(currentHoverLink.Value);
                    UpdateRangeColors(tmpro.textInfo,range, false, state ? Color.white : Color.black );
                } else if (t - Time.deltaTime < 2f) {
                    var range = GetRangeFrom(currentHoverLink.Value);
                    UpdateRangeColors(tmpro.textInfo,range, false, Color.white );
                }
            }
        }

        string lastLinkID;        
        Color32[] newVertexColors;
        TMP_LinkInfo? currentHoverLink;
        float timeOfHover;

        Range prevRange;
        Color32 rememberedColor;

        Range GetRangeFrom(TMP_LinkInfo link) {
            var a = link.linkTextfirstCharacterIndex;
            var b = a + link.linkTextLength;
            return a..b;
        }

        private void CheckForLink() {
            var linkIndex = TMP_TextUtilities.FindIntersectingLink(tmpro, Input.mousePosition, uicamera);
            if (linkIndex == -1) { OnLinkHover(null); return; }
            OnLinkHover(tmpro.textInfo.linkInfo[linkIndex]);    
        }

        private void OnLinkHover(TMP_LinkInfo? info ) {
            var linkID = info?.GetLinkID();
            if (linkID == lastLinkID) return;
            SetActiveLink(info);
            lastLinkID = linkID;
            timeOfHover = Time.time;
        }

        private void SetActiveLink(TMP_LinkInfo? activeLinkInfo) {
            var textInfo = tmpro.textInfo;

            if (prevRange.End.Value > 0) {
                UpdateRangeColors(textInfo, prevRange, false, rememberedColor);
                prevRange = 0..0;
            }

            if (activeLinkInfo.HasValue) {
                prevRange = GetRangeFrom(activeLinkInfo.Value);
                UpdateRangeColors(textInfo, prevRange, true, Color.red);
            }
            
            currentHoverLink = activeLinkInfo;
        }

        private void UpdateRangeColors(TMP_TextInfo textInfo, Range range, bool rememberColor, Color color = default) {
            if (range.End.Value == 0) return;
            
            for (var c = range.Start.Value; c < range.End.Value; c++) {
                var chr = textInfo.characterInfo[c];
                var materialIndex = chr.materialReferenceIndex;
                newVertexColors = textInfo.meshInfo[materialIndex].colors32;
                var vertexIndex = chr.vertexIndex;

                if (chr.isVisible) {
                    if (rememberColor) rememberedColor = newVertexColors[vertexIndex];
                    newVertexColors[vertexIndex + 0] = color;
                    newVertexColors[vertexIndex + 1] = color;
                    newVertexColors[vertexIndex + 2] = color;
                    newVertexColors[vertexIndex + 3] = color;
                }
            }
            tmpro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }
}
