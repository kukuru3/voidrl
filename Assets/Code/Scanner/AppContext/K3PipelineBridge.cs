using K3.Modules;
using K3.Pipeline;

namespace Scanner.AppContext {
    class PipelineBridge : CommonAppInitializer {

        IModuleContainer container;

        protected override void InitializeApplication(IModuleContainer context) {
            this.container = context;
            Core.AppContext.ModuleHelper.Writer.InjectContainer(context);
            var gameManager = new GameManager();
            context.InstallModule(gameManager);
            
            gameManager.AddSegment(new CoreSegment());
        }
    }
}
