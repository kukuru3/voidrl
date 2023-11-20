using System.Collections.Generic;
using Scanner.Megaship.ShipFunctions;
using UnityEngine;

namespace Scanner.Megaship {
    public class Module : MonoBehaviour {
        [field:SerializeField] public string Name { get; private set; }
        public bool IsPhantom => Ship == null;

        private ModuleMass massComponent;
        public ModuleMass MassComponent { get { massComponent ??= GetComponent<ModuleMass>(); return massComponent; } }
        internal Ship Ship { get; set; }
    }
}
