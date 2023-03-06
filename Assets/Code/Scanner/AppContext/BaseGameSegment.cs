using System.Collections.Generic;
using System.Linq;
using K3.Modules;

namespace Scanner.AppContext {
    // layer such as main menu, etc.

    public abstract class BaseSegment {        

        List<BaseModule> modules = new();

        internal void Inject() { 
            CreateModules();
        }
        
        internal void Cleanup() { 
            modules.Reverse();
            foreach (var module in modules) Core.AppContext.ModuleHelper.RemoveModule(module);
            modules.Clear();
        }

        protected abstract void CreateModules();

        protected void Install(BaseModule module) {
            modules.Add(module);
            Core.AppContext.ModuleHelper.InstallModule(module);
        }
    }
}
