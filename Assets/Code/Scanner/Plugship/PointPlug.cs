using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanner.Plugship {

    public interface IPlug {
        public Module Module { get; }
        public Joint Joint { get; }
        public bool IsConnected { get; }

        public IPlug ConnectedTo { get; }
        public Module ConnectedToModule { get; }
    }

    internal abstract class BasePlug : MonoBehaviour, IPlug {
        [field:SerializeField] public Polarity Polarity { get; private set; }
        [field:SerializeField] public string SlotTag { get; private set; }

        public bool IsConnected => Joint != null;

        internal List<PlugEnableCriterion> Criteria { get; private set; }

        public Module Module { get; internal set; }

        public IPlug ConnectedTo => Joint?.Other(this);
        public Module ConnectedToModule => ConnectedTo?.Module;

        public Joint Joint { get; internal set; }

        

        protected virtual void Start() {
            Criteria = GetComponentsInChildren<PlugEnableCriterion>(true).ToList();
        }
    }

    internal class PointPlug : BasePlug {
         private void OnDrawGizmos() {
            Gizmos.color = Color.yellow;

            if (Joint != null) {
                var c = Color.green;
                c.a = 0.5f;
                Gizmos.color = c;
            }

            Gizmos.matrix = transform.localToWorldMatrix;

            if (Polarity == Polarity.Out) {
                Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.2f); 
            } else if (Polarity== Polarity.In) {
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 0.3f);
            } else {

                if (Joint != null) {
                    var id = Joint.IndexOf(this);
                    if (id == 1) Gizmos.DrawWireSphere(Vector3.zero, 0.25f);
                    if (id == 2) Gizmos.DrawSphere(Vector3.zero, 0.25f);
                    //var id = Connection.ConnectionIndexOf(this);
                    //if (id == 1) Gizmos.DrawWireSphere(Vector3.zero, 0.25f);
                    //if (id == 2) Gizmos.DrawSphere(Vector3.zero, 0.2f);
                } else {
                    Gizmos.DrawSphere(Vector3.zero, 0.22f);
                }
            }
        }
    }

    internal abstract class PlugEnableCriterion : MonoBehaviour {

    }

    //#if UNITY_EDITOR
    //[CanEditMultipleObjects]
    //[CustomEditor(typeof(Plug))]
    //class PlugInspector : Editor {
    //    public override void OnInspectorGUI() {
    //        base.OnInspectorGUI();
    //    }
    //}
    //#endif

}
