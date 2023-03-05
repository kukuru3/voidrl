using System.Collections.Generic;
using K3.Modules;

namespace Scanner.AppContext {
    public class CoreSegment : BaseGameSegment {
        protected override IEnumerable<BaseModule> CreateModules() {
            var mainModule = new MainModule();
            Install(mainModule);
            yield return mainModule;
        }
    }

    class MainModule : BaseModule {
        
    }
}
