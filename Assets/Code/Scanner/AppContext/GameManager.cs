using System.Collections.Generic;
using K3.Modules;

namespace Scanner.AppContext {
    class GameManager : BaseModule {

        List<BaseGameSegment> activeSegments = new();
        
        protected override void Launch() {
            
        }

        protected override void Teardown() { 
            CleanupRemainingSegments(); 
        }
                
        public void AddSegment(BaseGameSegment segment) {
            if (activeSegments.Contains(segment)) return;
            activeSegments.Add(segment);
            segment.Inject();    
        }

        public void RemoveSegment(BaseGameSegment segment) {
            if (!activeSegments.Contains(segment)) return;
            segment.Cleanup();
            activeSegments.Remove(segment);
        }

        public void CleanupRemainingSegments() {
            foreach (var segment in activeSegments) segment.Cleanup();
            activeSegments.Clear(); 
        }
    }
}
