using Core;
using Core.h3x;
using System.Collections.Generic;

namespace Scanner.Atomship {
    // always start with exactly one module at QRZ 0,0,0.
    // be able to raycast that gives us both the module hit and the "direction" of the ray.


    // requires
    // allows
    // blocks


    public abstract class Constraint {

    }

   

    public enum FeatureTypes {
        Part,
        Connector,
        ProhibitedSpace,
    }

    public enum ConnectionTypes {
        Forbidden,
        Allowed,
        Implicit,
        Primary,
    }

    public class Feature {
        public FeatureTypes type;
        public Hex3 localCoords;
        public QRZDir localDirection;
        public int graphicVariant;
        public ConnectionTypes connType;
    }

    public class StructureModel {
        public List<Feature> features = new();
    }
}
