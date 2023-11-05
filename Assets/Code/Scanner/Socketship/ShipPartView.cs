using UnityEngine;

namespace Scanner.Socketship {
    public class ShipPartView : MonoBehaviour {
        public bool IsPhantom => Part?.ship == null;

        public Part Part { get; private set; }
        internal ShipBuilderController ParentView { get; private set; }
        public void Bind(ShipBuilderController parentView, Part part) {
            ParentView = parentView;
            Part = part;
            OnBound();
        }

        protected virtual void OnBound() {

        }
    }
}
