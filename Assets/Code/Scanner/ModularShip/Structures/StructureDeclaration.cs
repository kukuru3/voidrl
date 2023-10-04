using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Scanner.ModularShip.Structures {
    internal class StructureDeclaration {
        public string id;
        public string variant;
        internal Slotting slotting;
        internal List<Restriction> restrictions = new();
        internal List<StructuralEffect> effects = new();
    }

    abstract class StructuralEffect {

    }
    abstract class Restriction {

    }

    abstract class Slotting {
        
    }

    class RingSlotting : Slotting {
        public int spinalSlots;
        public int arcSlots;
    }

    class CircularSlotting : Slotting {
        public int spinalSlots;
    }

    class SpinalSlotting : Slotting {
        public int spineSegments;
    }

    class RingLevelRestriction : Restriction {
        public int ringLevel;
    }

    class RingLevelSpanRestriction : Restriction {
        public int ringLvlMin;
        public int ringLvlMax;
    }

    class RequiredQualityRestriction : Restriction {
        public string quality;
    }


    class MandatorySymmetryRestriction : Restriction {
        public List<int> allowedSymmetries = new();
    }

    class UnlockRingTilesEffect: StructuralEffect {
        public int ringOffset = 1;
        public int arcOffset;
        public int spinalOffset;
        public int arcSize;
        public int spinalSize;
        public List<string> BestowedTileQualities = new();
    }

    static class StructureHardcoder {
        static internal IEnumerable<StructureDeclaration> HardcodeStructures() {

            yield return new StructureDeclaration {
                id = "Spinal Segment",
                restrictions = { new RingLevelRestriction() {ringLevel = 0 }, },
                effects = { 
                    new UnlockRingTilesEffect { spinalOffset = 1, spinalSize = 1, ringOffset = 0 },
                    new UnlockRingTilesEffect { spinalOffset = 0, spinalSize = 3, ringOffset = 1 },
                },
                slotting = new SpinalSlotting { spineSegments = 3 },
            };

            yield return new StructureDeclaration {
                id = "Outer Ring Interface",
                restrictions = { 
                    new RingLevelRestriction() {ringLevel = 1 },
                    new MandatorySymmetryRestriction() { allowedSymmetries = { 1 } }
                },
                effects = { new UnlockRingTilesEffect { } },
                slotting = new CircularSlotting { spinalSlots = 2 }
            };

            yield return new StructureDeclaration {
                id = "Engine Injection assembly",
                slotting = new CircularSlotting { spinalSlots = 2 },
                effects = { new UnlockRingTilesEffect { arcSize = 12 } }
            };

            yield return new StructureDeclaration {
                id = "Heat radiator base - large",
                slotting = new RingSlotting { arcSlots = 3, spinalSlots = 3 },
                restrictions = { 
                    new MandatorySymmetryRestriction { allowedSymmetries = { 2,3,4 } },
                    new RingLevelRestriction {ringLevel = 1 },
                },
                effects = { 
                    new UnlockRingTilesEffect { arcSize = 1, ringOffset = 1, BestowedTileQualities = { "radiator" } },
                },
                
            };

            yield return new StructureDeclaration {
                id = "Radiator segment",
                slotting = new RingSlotting { arcSlots = 1, spinalSlots = 1 },
                restrictions = {     
                    new RequiredQualityRestriction { quality = "radiator" },
                    new RingLevelSpanRestriction {  ringLvlMin =  2, ringLvlMax = 4 }
                },
                effects = { 
                    new UnlockRingTilesEffect { arcSize = 1, ringOffset = 1, BestowedTileQualities = { "radiator" } },
                },

            };

            yield return new StructureDeclaration {
                id = "Plasma pulse interstellar engine",
                variant = "ring mounted",
                slotting = new RingSlotting { arcSlots = 2, spinalSlots = 2 },
            };

            yield return new StructureDeclaration {
                id = "Reservoir",
                variant = "circular large",
                slotting = new RingSlotting { arcSlots = 4, spinalSlots = 4 },
            };

            yield return new StructureDeclaration {
                id = "Reservoir",
                variant = "circular small",
                slotting = new RingSlotting { arcSlots = 2, spinalSlots = 2 },
            };

            yield return new StructureDeclaration {
                id = "Reservoir",
                variant = "longitudinal",
                slotting = new RingSlotting { arcSlots = 2, spinalSlots = 6 },
            };

            yield return new StructureDeclaration {
                id = "Toroid reactor",
                slotting = new CircularSlotting { spinalSlots = 2 },
            };

            yield return new StructureDeclaration {
                id = "Meteorite shield",
                slotting = new SpinalSlotting { spineSegments = 1 },
            };

            yield return new StructureDeclaration {
                id = "Rotator mechanism",
                slotting = new RingSlotting { arcSlots = 2, spinalSlots = 2 } 
            };

            yield return new StructureDeclaration {
                id = "Magnetic shield",
            };            

            yield return new StructureDeclaration {
                id = "Hangar bay",
            };

            yield return new StructureDeclaration {
                id = "Maintenance bay",
            };

            yield return new StructureDeclaration {
                id = "Container bay",
            };

            yield return new StructureDeclaration {
                id = "Observation bay",
            };

            yield return new StructureDeclaration {
                id = "Pressurized module",
                variant = "axial",
                slotting = new RingSlotting { arcSlots = 3, spinalSlots = 4 },
            };

            yield return new StructureDeclaration {
                id = "Pressurized module",
                variant = "ring",
                slotting = new RingSlotting { arcSlots = 4, spinalSlots = 3 },
            };

            yield return new StructureDeclaration {
                id = "Flywheel",
                slotting = new CircularSlotting { spinalSlots = 1 },
            };
        }
    }
}
