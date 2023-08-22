using System;
using System.Collections.Generic;
using Core.Units;
using UnityEngine;

namespace Core {

    static public class Constants {
        public const decimal c = 299792458; // m/s
        public const decimal g = 9.80665m; // m/s^2
        public const decimal AU = 149597870700; // m

        public const int SECONDS_IN_HOUR = 60 * 60;
        public const int SECONDS_IN_DAY = SECONDS_IN_HOUR * 24;
        public const int SECONDS_IN_YEAR = SECONDS_IN_DAY * 365;

        const decimal DISTANCE_LIGHT_SECOND = c * 1m;
        const decimal DISTANCE_LIGHT_YEAR = DISTANCE_LIGHT_SECOND * SECONDS_IN_YEAR;
    }

    internal static class UnitInfoBroker {
        static UnitInfoBroker()
        {
            new Units.UnitInfo_Time();
            new Units.UnitInfo_Dist();
            new Units.UnitInfo_V();
            new Units.UnitInfo_Accel();
            new Units.UnitInfo_Mass();
        }

        static public IUnitInfo<TUnit> GetUnitInfo<TUnit>() => (IUnitInfo<TUnit>)_registered[typeof(TUnit)];

        static Dictionary<Type, object> _registered = new();

        internal static void Register<T>(UnitInfo<T> unitInfo) => _registered[typeof(T)] = unitInfo;
    }

    
    static class TestConsumer {
        [UnityEditor.MenuItem("Void/SPACE MATH: do new measurements")]
        static public void Test() {
            var ts1 = new TimeSI(100);
            var ts2 = new TimeSI(1, TimeUnits.Years);
            var t3 = ts1 + ts2;
            Debug.Log(t3);

            var v = new Velocity(300_000, VelocityUnits.MetersPerSecond);
            Debug.Log(v);
        }


    }
}