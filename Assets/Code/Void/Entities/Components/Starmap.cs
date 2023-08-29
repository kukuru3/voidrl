﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Void.Entities.Components {
    public class Starmap : Container {

    }

    public class StellarObject : Container {
        public string name;
        public Vector3 galacticPosition;

        public IEnumerable<SubstellarObjectDeclaration> ContainedSubstellars => ListContainedEntities().Select(e => e.Get<SubstellarObjectDeclaration>());

        public IList<SubstellarObjectDeclaration> Primaries => ContainedSubstellars.Where(ss => IsStellarRootType(ss.type)).ToList();

        static public bool IsStellarRootType(StellarSubobjects type) => type switch { 
            StellarSubobjects.MainSequenceStar => true, 
            StellarSubobjects.BrownDwarf => true,
            StellarSubobjects.WhiteDwarf => true,
            _ => false
        };
    }

    public enum Designation {
        StarSystem,
        MultipleSystem,
    }

    public enum StellarSubobjects {
        MainSequenceStar,
        WhiteDwarf,
        BrownDwarf,
        JovianPlanet,
        NeptunianPlanet,
        TerrestrialPlanet,
        Unconfirmed,
    }

    public class SubstellarObjectDeclaration : Component {
        public string name;
        public StellarSubobjects type;
        public Vector3 galacticPosition;
        public string  spectral;
    }

}
