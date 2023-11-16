using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanner.ModularShip {

    public interface IPlug {
        public OldModule Module { get; }
        public Joint Joint { get; set; }
        public bool IsConnected { get; }

        public int IndexInParentModule { get; }

        public IPlug ConnectedTo { get; }
        public OldModule ConnectedToModule { get; }

        public Transform OrientationMatchingTransform { get; }

        bool EvaluateConditions();

        public Polarity Polarity { get; }
        public string SlotTag { get; }
    }

    internal abstract class BasePlug : MonoBehaviour, IPlug {
        [field:SerializeField] public Polarity Polarity { get; private set; }
        [field:SerializeField] public string SlotTag { get; private set; }

        public bool IsConnected => Joint != null;

        internal List<PlugEnableCriterion> Criteria { get; private set; }

        public int IndexInParentModule => Module?.AllPlugs.IndexOf(this) ?? -1;

        public OldModule Module { get; internal set; }

        public IPlug ConnectedTo => Joint?.Other(this);
        public OldModule ConnectedToModule => ConnectedTo?.Module;

        public Joint Joint { get; set; }

        public Transform OrientationMatchingTransform => transform;

        public bool EvaluateConditions() {
            foreach(var criterion in Criteria) {
                if (!criterion.Test(this)) return false;
            }
            return true;
        }

        protected virtual void Awake() {
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
                } else {
                    Gizmos.DrawSphere(Vector3.zero, 0.22f);
                }
            }
        }
    }

    internal abstract class PlugEnableCriterion : MonoBehaviour {
        public abstract bool Test(IPlug plug);
    }

}
