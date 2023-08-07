namespace Void {
    public static class SpaceMath {
        public const decimal c = 299792458; // m/s
        public const decimal g = 9.80665m; // m/s^2
        public const decimal AU = 149597870700; // m

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

        const int SECONDS_IN_HOUR = 60 * 60;
        const int SECONDS_IN_DAY = SECONDS_IN_HOUR * 24;
        const int SECONDS_IN_YEAR = SECONDS_IN_DAY * 365;

        const decimal DISTANCE_LIGHT_SECOND = c * 1m;
        const decimal DISTANCE_LIGHT_YEAR = DISTANCE_LIGHT_SECOND * SECONDS_IN_YEAR;

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
    }
}
