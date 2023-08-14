using UnityEngine;
using Void;

namespace Scanner.AppContext {
    public class TestUtil {
        static void TestProfile(decimal speedInC, decimal accelInG) {
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


        [UnityEditor.MenuItem("Void/SPACE MATH")]
        static public void RunTestsOnLaunch() {
            TestProfile(0.1m, 0.0001m);
            TestProfile(0.1m, 0.001m);
            TestProfile(0.1m, 0.01m);
            TestProfile(0.1m, 0.1m);
        }
    }
}
