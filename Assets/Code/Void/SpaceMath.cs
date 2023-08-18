using System;

namespace Void {
    public static class SpaceMath {

        static public decimal Root(this decimal item) {
            return (decimal)Math.Sqrt((double)item);
        }

        public const decimal c = 299792458; // m/s
        public const decimal g = 9.80665m; // m/s^2
        public const decimal AU = 149597870700; // m
        const int SECONDS_IN_HOUR = 60 * 60;
        const int SECONDS_IN_DAY = SECONDS_IN_HOUR * 24;
        const int SECONDS_IN_YEAR = SECONDS_IN_DAY * 365;

        const decimal DISTANCE_LIGHT_SECOND = c * 1m;
        const decimal DISTANCE_LIGHT_YEAR = DISTANCE_LIGHT_SECOND * SECONDS_IN_YEAR;

        public struct KineticProfile {
            public decimal timeToFullAcceleration;
            public decimal distanceOfFullAcceleration;
        }

        public static KineticProfile GetProfile(decimal velocityInCs, decimal accelerationInGs) {
            var v = c * velocityInCs;
            var a = g * accelerationInGs;

            var s = v * v / (2 * a);
            var t = v / a;

            return new KineticProfile { distanceOfFullAcceleration = s, timeToFullAcceleration = t };
        }

        

        public static string FormatDistance(decimal distanceInMeters) {
            var au = distanceInMeters / AU;
            var ls = distanceInMeters / DISTANCE_LIGHT_SECOND;
            var km = distanceInMeters / 1000m;
            return $"{au:F0}AU / {ls:F0}ls / {km:F0}km";
        }

        public static string FormatTime(decimal timeInSeconds) {
            var years = timeInSeconds / SECONDS_IN_YEAR;
            if (years > 0.5m) return $"{years:F1} years";
            
            var days = timeInSeconds / SECONDS_IN_DAY;
            if (days > 3m) {
                return $"{days:F0} days";
            } else {
                return $"{days*24:F0} hours";
            }            
        }

        public static decimal LightYearsToMeters(decimal lightYears) => DISTANCE_LIGHT_YEAR * lightYears;

        public struct TravelEstimate {
            public decimal distance;
            public decimal maxV;
            public decimal coastTime;
            public decimal totalTime;
        }

        public static TravelEstimate EstimateTravelTime(decimal distance, decimal maxV, decimal accel) {
            var tReachMaxV = maxV / accel;
            // we have two identical LEGS, accelerating and decelerating
            // t = sqr(2s/a), but here, s is one LEG, which is half the distance
            var tLeg = (decimal)Math.Sqrt((double)(distance / accel));

            if (tReachMaxV < tLeg ) { // if we would reach maxV before we reach the leg of the journey, then it's a more complex equation.
                // first, we go until we reach maxV; then, we 
                var coastTime = distance / maxV - tReachMaxV;
                return new TravelEstimate {
                    distance = distance,
                    maxV = maxV,
                    coastTime = coastTime,
                    totalTime = tReachMaxV  * 2 + coastTime,
                };  
                // | tRM |  tCoast | tRM |
                // |     -----------         <- vMax
                // |    /|         |\
                // |   / |         | \      // distance is the surface under the graph
                // |  /  |         |  \     // so Dist = (tRM + tCoast) * vMax
                // | /   |         |   \    // so tCoast = Dist / vMax - tRM
                // +----------------------

            } else { // otherwise it's super simple, accel / decel
                return new TravelEstimate {
                    distance = distance,
                    maxV = tLeg * accel,
                    coastTime = 0,
                    totalTime = 2 * tLeg,
                };
            }
        }
    }
}
