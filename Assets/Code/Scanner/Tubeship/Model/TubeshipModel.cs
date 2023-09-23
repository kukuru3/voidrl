using System;
using System.Collections.Generic;

namespace Scanner.TubeShip {

    class Tube {
        public Tube(int spineSegments, int arcSegments) {
            SpineSegments = spineSegments;
            ArcSegments = arcSegments;
            allCoords = BuildCoords();
        }
        public TubularCoords[] AllCoords => allCoords;

        private TubularCoords[] BuildCoords() {
            var list = new List<TubularCoords>();
            for (var s = 0; s < SpineSegments; s++)
            for (var a = 0; a < ArcSegments; a++)
                list.Add(new TubularCoords() {tube = this, arcPos = a, spinePos = s });
            
            return list.ToArray();
        }

        public int ArcSegments { get; }
        public int SpineSegments { get; }
        
        TubularCoords[] allCoords;
    }

    // an active instance of a facility that occupies tile(s) on a tube
    class TubeFacility { 
        public List<TubularCoords> occupiedCoordinates = new();
    }

    struct TubularCoords {
        public Tube tube;
        public int arcPos;
        public int spinePos;
    }

    class TubeshipModel {
        List<Tube> tubes = new List<Tube>();

        public void AddTube(Tube tube) => tubes.Add(tube);

        public IReadOnlyList<Tube> AllTubes => tubes;
    }
    
}


