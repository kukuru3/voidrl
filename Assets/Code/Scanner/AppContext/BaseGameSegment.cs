using System.Collections.Generic;
using System.Linq;
using K3.Modules;

namespace Scanner.AppContext {
    // layer such as main menu, etc.

    public abstract class BaseGameSegment {        

        List<BaseModule> modules = new();

        internal void Inject() { 
            modules = CreateModules().ToList();
        }
        internal void Cleanup() { 
            modules.Reverse();
            foreach (var module in modules) Core.AppContext.ModuleHelper.RemoveModule(module);
            modules.Clear();
        }

        protected abstract IEnumerable<BaseModule> CreateModules();

        protected void Install(BaseModule module) {
            Core.AppContext.ModuleHelper.InstallModule(module);
        }
    }
}
