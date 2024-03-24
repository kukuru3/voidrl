using System.Collections.Generic;

namespace Void.ColonySim {
    

    public class DistroNode {
        internal List<DistroLine> lines = new List<DistroLine>();

        internal float value; // + for producers, - for consumers
    }

    public class DistroLine {
        internal readonly DistroNode a;
        internal readonly DistroNode b;
    }

    public class DistributionGraph {

    }


    public class DistributionSystem {
        
    }
}
