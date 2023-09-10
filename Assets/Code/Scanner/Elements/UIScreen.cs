using UnityEngine;

namespace Scanner {
    // unlike a Window, a Screen is meant to occupy all parts of the monitor ui real estate
    // and no more than a single screen should be visible at a time
    class UIScreen : UIGroup {
        
    }
    static public class ElementExtensions {
        static internal Screen GetScreenOf(this Element e) => e.GetComponentInParent<Screen>(true);
        static internal UIGroup GetGroupOf(this Element e) => e.GetComponentInParent<UIGroup>(true);

        static internal void Hide(this UIGroup group) {
            group.gameObject.SetActive(false);
        }

        static internal void Show(this UIGroup group) {
            group.gameObject.SetActive(true);
        }
    }
}