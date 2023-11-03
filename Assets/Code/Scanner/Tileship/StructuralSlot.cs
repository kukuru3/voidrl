using System.Collections.Generic;
using UnityEngine;

namespace Scanner.Tileship {
    
    internal class Ship {
        List<ShipPart> parts;
    }

    internal class Socket {
        public string[] tags;
    }

    internal class ShipPart {
        
    }

    internal class ShipPartDeclaration {
        public string name;
        public bool hidden;
        internal SlottingDeclaration slotsInto = new();

    }

    internal class SlottingDeclaration {
        public List<SlottingInstruction> instructions = new();
    }

    internal abstract class SlottingInstruction {
        
    }

    internal class RequiresTag  : SlottingInstruction {
        internal readonly string id;
        public RequiresTag(string id) {
            this.id = id;
        }
    }


    internal struct SlottingData {
        internal Socket socket;
        internal Ship ship;
    }

    static class HardcodedPartDeclarations {
        public static IEnumerable<ShipPartDeclaration> GetDeclarations() {
            yield return new ShipPartDeclaration() {
                name = "root",
                hidden = true,
            };

            yield return new ShipPartDeclaration {
                name = "spine",
                slotsInto = new SlottingDeclaration { instructions = { new RequiresTag("spine")  } },
            };

            yield return new ShipPartDeclaration {
                name = "small ring",
                slotsInto = new SlottingDeclaration { instructions = { new RequiresTag("spine") } }
            };
        }
    }


    static class InitialShipLayoutGenerator {
        // first, generate the "root slot" of the spine.
        // then, generate spine front and back.
        // that has empty slots.
        // later on, it is possible to add spine extensions

        static internal void HardcodeInitialSpine() {

        }
    }
}
