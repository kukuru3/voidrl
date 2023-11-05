using UnityEngine;

namespace Scanner.Socketship {

    public class ShipPartView : MonoBehaviour, IPartOfShipbuilderView {

        [SerializeField] Shapes.Rectangle rect;
        [SerializeField] TMPro.TMP_Text label;

        public Part Part { get; private set; }
        internal ShipBuilderController ParentView { get; private set; }
        
        public bool IsPhantom => Part == null;

        public void Bind(ShipBuilderController parentView, Part part) {
            this.Part = part;
            ParentView = parentView;

            label.text = part.declaration.name;
        }
    }
}
