using UnityEngine;

namespace Scanner {
    [RequireComponent(typeof(Button))]
    public abstract class ButtonResponder : MonoBehaviour {
        private void Start() {
            var b = GetComponent<Button>();
            b.Clicked += OnButtonClicked;
        }

        protected abstract void OnButtonClicked();
    }
}