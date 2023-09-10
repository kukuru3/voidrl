using Cysharp.Threading.Tasks;
using K3.Pipeline;
using UnityEngine.LowLevel;
using Void.Scripting;
using UniTaskLoopHelper = Cysharp.Threading.Tasks.PlayerLoopHelper;
namespace Scanner.AppContext {
    
   public class Bootstrapper : IPipelineInjector {
       public void Inject(IPipeline pipeline) {

            InjectUniTaskCallbaks();

            ScriptAPI.Register(new MessagePump("ui"));
            ScriptAPI.Register(new MessagePump("game"));

            ScriptAPI.InitializePython();
       }
        private static void InjectUniTaskCallbaks() {
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            UniTaskLoopHelper.Initialize(ref loop, InjectPlayerLoopTimings.All);
            PlayerLoop.SetPlayerLoop(loop);
        }
   }
}

