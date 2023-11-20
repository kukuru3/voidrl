using System.Collections.Generic;
using UnityEngine;

namespace Scanner.Megaship {
    class ParentDependents : MonoBehaviour, IContactProcessor {
        [SerializeField] Transform explicitParent;

        void Start() {
            if (explicitParent == null) explicitParent = transform;
        }

        public void OnContactChanged(Linkage activeContact) {
            var localPlug = GetComponent<IPlug>();
            var localModule = localPlug.Module;

            if (activeContact != null) {
                var otherModules = activeContact.OtherModulesInContact(localModule);
                foreach (var module in otherModules) module.transform.parent = explicitParent.transform;
            }
        }
    }
}
