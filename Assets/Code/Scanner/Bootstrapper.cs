using K3.Pipeline;
using Void.Scripting;

namespace Scanner.AppContext {
    
    public class Bootstrapper : IPipelineInjector {
        public void Inject(IPipeline pipeline) {

            ScriptAPI.Register(new MessagePump("ui"));
            ScriptAPI.Register(new MessagePump("game"));

            ScriptAPI.InitializePython();
        }
    }
}

