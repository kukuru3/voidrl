using System.Collections.Generic;
using UnityEngine;

namespace Scanner.Megaship {
    class ParentDependents : MonoBehaviour, IContactProcessor {
        public void OnContactChanged(Linkage activeContact) {
            var localPlug = GetComponent<IPlug>();
            var localModule = localPlug.Module;

            if (activeContact != null) {
                var otherModules = activeContact.OtherModulesInContact(localModule);
                foreach (var module in otherModules) module.transform.parent = localModule.transform;
            }
        }
    }
}
