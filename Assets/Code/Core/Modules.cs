using System.Collections.Generic;
using K3.Modules;
using Core.AppContext;
using System;

public static class App {
    public static T Module<T>() => ModuleHelper.GetModule<T>();
    
    public static IEnumerable<BaseModule> ListModules() => ModuleHelper.ListModules();
    
    public static TComponent Component<TModule, TComponent>() where TComponent : class =>
        (Module<TModule>() as BaseModule)?.GetModuleComponent<TComponent>();
}

namespace Core.AppContext {
    
    public static class ModuleHelper {
        static IModuleContainer context;
        internal static T GetModule<T>() => context.GetModule<T>();
        
        internal static IEnumerable<BaseModule> ListModules() => context.Modules;
        
        public static class Writer {
            public static void InjectContainer(IModuleContainer container) => context = container;
        }

        public static void InstallModule<T>() where T : BaseModule, new() => InstallModule(new T());
        public static void InstallModule<T>(T module) where T : BaseModule => context.InstallModule(module);
        public static void RemoveModule<T>(T module) where T : BaseModule => context.RemoveModule(module);
    }
}
