using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scanner.HexShip {
    public class ConstructionContext : MonoBehaviour {

        public enum Symmetries {
            Single,
            Double,
            Triple,
            Hexuple,
        }

        List<ConstructibleModuleDeclaration> declarations;

        private void Start() {
            declarations = HexagonalPlayground.DeclareModules().ToList();
            // generate buttons for declarations and for symmetries
        }
    }
}