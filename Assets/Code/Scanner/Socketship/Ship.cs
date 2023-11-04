using System.Collections.Generic;
using UnityEngine;

namespace Scanner.Socketship {
    public class PartDeclaration {
    }

    public abstract class Contact {
        public Vector2 offset;
        List<string> tags = new();
    }

    public class Socket : Contact {
        
    }

    public class Plug : Contact {
        List<IPlugCriterion> plugCriteria;
    }

    public interface IPlugCriterion {

    }
}
