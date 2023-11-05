using UnityEngine;

namespace Scanner.Socketship {

    public class GenericShipPartView : ShipPartView {
        [SerializeField] TMPro.TMP_Text label;

        protected override void OnBound() {
            label.text = Part.declaration.name;
        }
    }
}
