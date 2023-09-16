using UnityEngine;

namespace Scanner.ModularShip {

    public enum SlotTypes {
        Male,
        Female,
        Bidirectional,
        Special,
    }

    public class Slot : MonoBehaviour {
        [SerializeField] string slottingTag;

        [field:SerializeField] public SlotTypes Direction { get; internal set; }

        public Slot ConnectedTo { get; private set; }

        public void EstablishConnection(Slot other) {
            ConnectedTo = other;
        }

        public void Disconnect() {
            ConnectedTo = null;
        }
    }


}