using Core;
using K3.Modules;
using Scanner.ScannerView;

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

    public class GameReferences {
        public CameraController3D scannerCamera;
    }

    class GameModule : BaseModule {
        public GameReferences gameRefs;

        protected override void Launch() {
            gameRefs = new GameReferences();
            CollectReferences();
        }
        private void CollectReferences() {
            gameRefs.scannerCamera = CustomTag.Find(ObjectTags.ScannerCamera).GetComponent<CameraController3D>();
        }
    }
}
