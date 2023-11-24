using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



namespace Scanner.Megaship.ShipFunctions {

    public interface IPointMassProvider {
        IEnumerable<PointMass> GetPointMasses();
    }

    public struct PointMass {
        internal Vector3 localPosition;
    }

    internal abstract class BaseMassProvider : MonoBehaviour, IPointMassProvider {
        public abstract IEnumerable<PointMass> GetPointMasses();
        private void OnDrawGizmosSelected() {
            Gizmos.matrix = transform.localToWorldMatrix;
            var c = Color.red;
            c.a = 1f;
            Gizmos.color = c;
            foreach (var pm in GetPointMasses()) {
                Gizmos.DrawSphere(pm.localPosition, 0.1f);
            }  
        }
    }

    public class ModuleMass : MonoBehaviour {

        internal const float ProceduralResolution = 1f;

        [SerializeField][Range(0.01f, 100f)] float massKilotons;

        List<IPointMassProvider> providers;

        public IEnumerable<PointMass> AllMassPointsInModuleSpace() {
            if (providers == null)  providers = GetComponentsInChildren<IPointMassProvider>().ToList();
            foreach (var provider in providers) { 
                // individual providers return points in their local space. 
                var transformationMatrix = transform.worldToLocalMatrix * ((Component)provider).transform.localToWorldMatrix;
                foreach (var pm in provider.GetPointMasses()) {
                    yield return new PointMass { localPosition = transformationMatrix.MultiplyPoint3x4(pm.localPosition) };
                }
            }
        }
    }
}