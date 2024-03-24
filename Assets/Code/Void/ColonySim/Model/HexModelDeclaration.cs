using System;
using System.Collections.Generic;
using Core.H3;

namespace Void.ColonySim.Model {
    using HNode = HexBlueprint.HexNode;
    using HConnector = HexBlueprint.HexConnector;

    public class HexBlueprint {
        public string identity;
        public List<HNode> nodes = new();
        public List<HConnector> connections = new();

        [Serializable] public class HexNode {
            public int index;
            public H3 hex;
        }

        [Serializable] public class HexConnector {
            public int index;
            public H3 sourceHex;
            public PrismaticHexDirection direction;
            public int flags;
        }
    }
}
