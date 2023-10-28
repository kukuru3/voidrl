using UnityEngine;

namespace Scanner.Megaship {
    /// <summary>A structural plug that helps with slotting</summary>
    class Plug : MonoBehaviour {

        public enum Polarity {
            None, 
            Out,
            In,
        }

        [field:SerializeField] internal Polarity PType { get; private set; }

        private void Awake() {
            this.Module = GetComponentInParent<Module>();
            Module.RegisterPlug(this);
        }

        public Module Module { get; private set; }
        public Connection Connection { get; private set; }

        public void OnConnect(Connection c) {
            Connection = c;
        }

        public void OnDisconnect() {
            Connection = null; 
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.yellow;

            if (Connection != null) {
                var c = Color.green;
                c.a = 0.5f;
                Gizmos.color = c;
            }

            Gizmos.matrix = transform.localToWorldMatrix;

            if (PType == Polarity.Out) {
                Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.2f); 
            } else if (PType == Polarity.In) {
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 0.3f);
            } else {
                if (Connection != null) {
                    var id = Connection.ConnectionIndexOf(this);
                    if (id == 1) Gizmos.DrawWireSphere(Vector3.zero, 0.25f);
                    if (id == 2) Gizmos.DrawSphere(Vector3.zero, 0.2f);
                } else {
                    Gizmos.DrawSphere(Vector3.zero, 0.22f);
                }
            }
        }
    }


}
