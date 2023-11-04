using System.Collections.Generic;
using UnityEngine;

namespace Scanner.Tileship {
    
    public class Ship {
        List<ShipPart> parts;
    }



    public class ShipPart {
        internal readonly Ship ship;
        internal readonly ShipPartDeclaration declaration;

        public ShipPart(Ship ship, ShipPartDeclaration declaration) {
            this.ship = ship;
            this.declaration = declaration;
        }
    }

    internal class SocketDeclaration {
        public string[] tags;
        public Vector2 offset;
    }

    public class ShipPartDeclaration {
        public string name;
        public bool hidden;
        internal SlottingDeclaration slotsInto = new();
        internal List<SocketDeclaration> sockets = new();
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

    static class HardcodedPartDeclarations {
        public static IEnumerable<ShipPartDeclaration> GetDeclarations() {
            yield return new ShipPartDeclaration() {
                name = "root",
                hidden = true,
            };

            yield return new ShipPartDeclaration {
                name = "spine segment",
                slotsInto = new SlottingDeclaration { instructions = { new RequiresTag("spine-ext")  } },
                sockets = new List<SocketDeclaration> { 
                    new SocketDeclaration { tags = new[] {"spine-ext" }, offset = new Vector2(2,0) },
                    new SocketDeclaration { tags = new[] {"spine-ext" }, offset = new Vector2(-2,0) },
                    new SocketDeclaration { tags = new[] {"spine-attach"}},
                },
            };

            var sd = new ShipPartDeclaration {
                name = "small ring",
                slotsInto = new SlottingDeclaration { instructions = { new RequiresTag("spine-attach") } },
            };
            sd.sockets.AddRange(GenerateSocketRect(1, 6, new Vector2(0,0), new Vector2(1,0), new Vector2(0,1), "facility-slot" ) );
            yield return sd;
        }

        private static IEnumerable<SocketDeclaration> GenerateSocketRect(int countX, int countY, Vector2 center, Vector2 offsetX, Vector2 offsetY, params string[] tags) {
            var zero = center - offsetX * (countX - 1f) / 2f - offsetY * (countY - 1f) / 2f;
           
            var arr = new SocketDeclaration[countX,countY];

            for (var x = 0; x < countX; x++)
            for (var y = 0; y < countY; y++) {
                var s = new SocketDeclaration() { offset = zero + x * offsetX + y * offsetY, tags = tags };
                arr[x,y] = s;
            }

            // todo handle adjacency
            
            for (var x = 0; x < countX; x++)
                for (var y = 0; y < countY; y++) 
                    yield return arr[x,y];
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
