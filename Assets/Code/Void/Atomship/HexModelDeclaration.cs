using System;
using System.Collections.Generic;
using Core.H3;

namespace Void.Model {
    using HNode = HexModelDefinition.HexNode;
    using HConnector = HexModelDefinition.HexConnector;

    public class StructureDeclaration {
        public HexModelDefinition hexModel;
        public string ID { get; set; }
    }
    
    public class HexModelDefinition {
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
