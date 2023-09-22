using UnityEditor;
using UnityEngine;

namespace Scanner.ModularShip {

    public enum SlotTypes {
        Male,
        Female,
        Bidirectional,
        Special,
    }

    public class Slot : MonoBehaviour {
        [SerializeField] internal string slottingTag;

        [field:SerializeField] public SlotTypes Direction { get; internal set; }

        public Slot ConnectedTo { get; private set; }

        public void EstablishConnection(Slot other) {
            ConnectedTo = other;
        }

        public void Disconnect() {
            ConnectedTo = null;
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = SlotColorUtils.GetColor(this);
            Gizmos.matrix = transform.localToWorldMatrix;

            if (Direction == SlotTypes.Male) {
                Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.2f);
                Gizmos.DrawLine(Vector3.zero, Vector3.forward * 0.5f);
            } else if (Direction == SlotTypes.Female) {
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 0.2f);
                Gizmos.DrawLine(Vector3.zero, Vector3.forward * -0.3f);
            } else if (Direction == SlotTypes.Bidirectional) {
                Gizmos.DrawSphere(Vector3.zero, 0.2f);
            }
        }
    }

    public static class SlotColorUtils {
        public static Color GetColor(Slot slot) {
            var t = slot.slottingTag.ToLowerInvariant();
            if (t.Contains("spinal")) return Color.cyan;
            if (t.Contains("radial")) return Color.red;
            if (t.Contains("lateral")) return Color.green;

            return Color.yellow;
        }
    }
}