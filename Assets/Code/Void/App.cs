using UnityEngine;
using Void.AppContext;
using Void.Generators;

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
        SystemGenerator SystemGenerator { get; }
        SectorGenerator SectorGenerator { get; }

        SceneReferences SceneReferences { get; }
    }
    
    internal class AppContext : IAppContext {
        public SystemGenerator SystemGenerator { get; set; }
        public SectorGenerator SectorGenerator { get; set; }

        public SceneReferences SceneReferences { get; set; }
    }

    public class SceneReferences {

        public T Find<T>() where T : UnityEngine.Object => GameObject.FindObjectOfType<T>();
    }
}