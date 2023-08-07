using UnityEngine;
using Void;

namespace Scanner.AppContext {
    class TestUtil {
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

        public void RunTestsOnLaunch() {
            TestProfile(0.05m, 1m);
            TestProfile(0.1m, 1m);
            TestProfile(0.1m, 5m);
            TestProfile(0.2m, 5m);
        }
    }
}
