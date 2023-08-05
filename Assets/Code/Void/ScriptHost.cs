using System.IO;
using System.Reflection;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace Void.Scripting {

    public class ScriptHost {
        private ScriptEngine engine;
        private ScriptScope mainScope;

        public ScriptHost() {
            CreateScriptingEngine();
        }

        public void PrepareScope() {
            mainScope = engine.CreateScope();
            mainScope.ImportModule("clr");
            engine.Runtime.LoadAssembly(GetUnityAssembly());
        }

        private void CreateScriptingEngine() {
            engine = Python.CreateEngine();
        }
        
        Assembly GetUnityAssembly() => Assembly.GetAssembly(typeof(UnityEngine.GameObject));

        
        public void Execute(string pythonCode) {
            engine.Execute(pythonCode, mainScope);
        }

        public object ExecuteFunction(string function, params object[] parameters) {
            //var m = mainScope.GetVariable(module) as IronPython.Runtime.PythonModule;
            //var f = m?.Scope.Storage.Dictionary[function];
            //if (f != null)
            //    return engine.Operations.Invoke(f, parameters) as object;
            //return null;
            var f = mainScope.GetVariable(function);
            return engine.Operations.Invoke(f, parameters);
        }

        public void ExposeAssemblyToScripts(Assembly assembly) => engine.Runtime.LoadAssembly(assembly);

        public void LoadScriptFilesFromDirectory(string path) {
            var di = new DirectoryInfo(path);
            if (!di.Exists) throw new DirectoryNotFoundException($"Not found : {path}");
            var files = di.EnumerateFiles("*.py", SearchOption.AllDirectories);
            foreach (var file in files) {
                var scope = engine.ExecuteFile(file.FullName, mainScope);
            }
        }

        public void LoadScriptFile(string path) {
            var fi = new FileInfo(path);
            if (!fi.Exists) throw new FileNotFoundException($"Engine cannot execute file: {path}");
            engine.ExecuteFile(fi.FullName, mainScope);
        }

    }
}
