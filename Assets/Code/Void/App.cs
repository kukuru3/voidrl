using UnityEngine;
using Void.AppContext;

namespace Void {
    // shortcuts:
    public static class App {
        static public IAppContext Context => ContextHolder.context;
    }
}

namespace Void.AppContext {

    static internal class ContextHolder {
        static internal IAppContext context;
    }
        
    public interface IAppContext {
    }
    
    internal class AppContext : IAppContext {
    }

    public class SceneReferences {
    }
}

