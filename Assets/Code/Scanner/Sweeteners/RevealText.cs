using TMPro;
using UnityEngine;

namespace Scanner.Sweeteners {

    internal class RevealText : MonoBehaviour {

        [SerializeField] int frequency;
        [SerializeField] int cursorOn;
        [SerializeField] int cursorOff;
        TMP_Text tmpro;
        float accumulatedTime;

        private void Start() {
            tmpro = GetComponent<TMP_Text>();
            if (!tmpro.text.EndsWith("_")) tmpro.text += "_";
            accumulatedTime = 0f; // 1f / frequency;
            tmpro.maxVisibleCharacters = 1;
        }

        int cursor = 0;

        [ContextMenu("Reset")]

        private void Reset() {
            cursor = 0;
            accumulatedTime = 0f;
            // tmpro.text = "";
        }

        private void LateUpdate() {
            if (tmpro == null) return;
            
            accumulatedTime += Time.deltaTime;
            var interval = 1f / frequency;

            while (accumulatedTime > interval) {
                accumulatedTime -= interval;
                cursor++;
            }

            if (cursor > tmpro.textInfo.characterCount - 1) {                    
                //var cycleLength = cursorOn + cursorOff;
                //if (cycleLength > 0) {
                //    var q = Time.frameCount % cycleLength;                        
                //    tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount - ((q < cursorOn) ? 0 : 1);    
                //}
            } else {
                tmpro.maxVisibleCharacters = cursor;

            }
        }
    }
}
