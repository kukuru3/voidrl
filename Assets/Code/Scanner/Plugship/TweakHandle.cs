using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scanner.Plugship {

    // TWEAKS are how you modify your ship.
    // plugs are not visible on the screen per se.
    // every time your module list changes, the shipbuilder recalculates a list of possible TWEAKS

    public class TweakHandle : MonoBehaviour {
        public Tweak Tweak { get; set; }

        void Start() {
            GetComponentInChildren<Button>().Clicked += () => ExecuteTweak();
        }

        void ExecuteTweak() {
            Tweak.Execute();
        }
    }

    public abstract class Tweak {
        internal abstract void Execute();
    }

    public struct PotentialAttachment {
        public IPlug shipPlug;
        public Module phantom;
        public int    indexOfPlugInPhantomList;
    }
    public class AttachAndConstructModule : Tweak {
        internal override void Execute() {
            Context.ShipbuildingContext.SelectActiveTweak(this);
            Context.ShipbuildingContext.GenerateStructureButtons(new[] { attachment });
        }
        public PotentialAttachment attachment;
        // this hinges on GetComponentsInChildren ordering being deterministic, which it should be? maybe? Question mark?
    }

    public class AttachAndConstructButMustChoose : Tweak {
        internal override void Execute() => throw new NotImplementedException();
        public List<PotentialAttachment> attachments;
    }

    public class DeconstructModule : Tweak {
        internal override void Execute() => throw new NotImplementedException();
        public Module shipModuleToBeDeconstructed;
    }
}
