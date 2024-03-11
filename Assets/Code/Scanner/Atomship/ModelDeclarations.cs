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

    public enum QRZDir : byte{
        None,
        Top, RightTop, RightBot, Bottom, LeftBot, LeftTop, 
        Forward, Backward
    }

    public enum FeatureTypes {
        Part,
        Connector,
    }

    public enum ConnectionTypes {
        Forbidden,
        Allowed,
        Implicit,
        Primary,
    }

    public class Feature {
        public FeatureTypes type;
        public Hex3 coords;
        public int graphicVariant;
        public QRZDir direction;
        public ConnectionTypes connType;
        //public Constraint constraint;
    }

    public class StructureModel {
        public List<Feature> features = new();
    }
}
