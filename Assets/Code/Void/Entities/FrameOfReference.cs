using System.Collections.Generic;

namespace Void.Entities {
    public enum Layers {
        Tactical,
        Vicinity,
        Stellar,
        Galactic
    }

    public class FrameOfReference {
        public Layers layer;
        
        public IEnumerable<FrameOfReference> Children => throw new System.NotImplementedException();
    }
}
