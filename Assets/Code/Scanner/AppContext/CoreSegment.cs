using System.Collections.Generic;
using K3.Modules;

namespace Scanner.AppContext {
    public class CoreSegment : BaseSegment {
        protected override void CreateModules() {
            Install(new MainModule());
        }
    }

    public class GameSegment : BaseSegment {
        protected override void CreateModules() {
            Install(new GameModule());
        }
    }

    class MainModule : BaseModule {
        
    }

    class GameModule : BaseModule {
         
    }
}
