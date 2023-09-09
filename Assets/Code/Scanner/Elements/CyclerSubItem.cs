using UnityEngine;

namespace Scanner {
    class CyclerSubItem : MonoBehaviour {
        [SerializeField] GameObject activeIndicator;
        public int IndexInCycler { get; private set; }

        private Cycler cycler;

        internal void Init(Cycler cycler, int index) {
            IndexInCycler = index;
            this.cycler = cycler;
            GetComponent<Button>().Clicked += HandleClick;
        }

        private void HandleClick() {
            cycler.CyclerIndex = IndexInCycler;
        }

        private void LateUpdate() {
            this.activeIndicator.SetActive(cycler.CyclerIndex == this.IndexInCycler);
        }
    }
}