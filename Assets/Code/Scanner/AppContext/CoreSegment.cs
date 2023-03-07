using Core;
using K3.Modules;
using Scanner.ScannerView;
using UnityEngine;
using Void;
using Void.Generators;

namespace Scanner.AppContext {
    public class CoreSegment : BaseSegment {
        protected override void CreateModules() {
            var cm = new CoreModule();
            cm.AddComponent(new Loading.HardcodedDataInitializer());
            Install(cm);
            Install(new SectorGenerator());
        }
    }

    public class GameSegment : BaseSegment {
        protected override void CreateModules() {
            Install(new GameModule());
        }
    }

    class CoreModule : BaseModule {
        void TestProfile(decimal speedInC, decimal accelInG) {
            var profile = SpaceMath.GetProfile(speedInC, accelInG);
            var d = SpaceMath.FormatDistance(profile.distanceOfFullAcceleration);
            var t = SpaceMath.FormatTime(profile.timeToFullAcceleration);
            var distanceToAlphaCentauriLY = SpaceMath.LightYearsToMeters(4.1m);
            var speedInMetersPerSecond = speedInC * SpaceMath.c;
            var timeToReachACDisregardingAccel =distanceToAlphaCentauriLY / speedInMetersPerSecond;

            Debug.Log($"at {speedInC}c max speed and at {accelInG}g acceleration, "
                + $", it will take {t} to fully accelerate or decelerate, which will take {d}, "
                + $" reaching Alpha Centauri would take {SpaceMath.FormatTime(timeToReachACDisregardingAccel)}"
            );
        }

        protected override void Launch() {
            TestProfile(0.05m, 1m);
            TestProfile(0.1m, 1m);
            TestProfile(0.1m, 5m);
            TestProfile(0.2m, 5m);
        }
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
