using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Void.Generators;

namespace System.Runtime.CompilerServices {
    public class IsExternalInit { }
}

namespace Void {

    public class DynamicGalaxy {
        Dictionary<Vector3Int, GalacticSector> _lookup;
        List<GalacticSector> loadedSectors = new();

        public void AddSector(GalacticSector sector) {
            loadedSectors.Add(sector);
            _lookup = null;
        }

        void RegenerateLookupIfNecessary() {
            _lookup ??= loadedSectors.ToDictionary(s => s.Coords);
        }

        public GalacticSector  GetSectorAt(Vector3Int coords) {
            RegenerateLookupIfNecessary();
            if (_lookup.TryGetValue(coords, out var result)) return result;

            var sector = App.Context.SectorGenerator.GenerateNewSector(coords);
            AddSector(sector);
            return sector;
        }
    }

    public class GalacticSector {
        public Vector3Int Coords { get; init; }
    }

    
}
