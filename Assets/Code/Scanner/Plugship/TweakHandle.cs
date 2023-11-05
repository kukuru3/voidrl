using System.Collections.Generic;
using UnityEngine;

namespace Scanner.Plugship {

    // TWEAKS are how you modify your ship.
    // plugs are not visible on the screen per se.
    // every time your module list changes, the shipbuilder recalculates a list of possible TWEAKS

    public class TweakHandle : MonoBehaviour {
        public Tweak Tweak { get; set; }
    }

    public abstract class Tweak {

    }

    public struct Attachment {
        public IPlug shipPlug;
        public Module phantom;
        public int    indexOfPlugInPhantomList;
    }
    public class AttachAndConstructModule : Tweak {
        public Attachment attachment;
        // this hinges on GetComponentsInChildren ordering being deterministic, which it should be? maybe? Question mark?
    }

    public class AttachAndConstructChoice : Tweak {
        public List<Attachment> attachments;
    }

    public class DeconstructModule : Tweak {
        public Module shipModuleToBeDeconstructed;
    }
}
