using System;
using static Core.Units.DistanceUnits;

namespace Core.Units {
    public enum DistanceUnits {
        Milimeter,
        Meter,
        Kilometer,
        LightSecond,
        AU,
        LightYear,
    };


    public class Distance : SIValue<DistanceUnits> {
        public Distance(decimal meters) : base(meters) { }
        public Distance(decimal value, DistanceUnits fromUnit) : base(value, fromUnit) { }

        static public Distance operator+(Distance a, Distance b) => new Distance(a.value + b.value);
        static public Distance operator-(Distance a, Distance b) => new Distance(a.value - b.value);

        static public Distance operator*(Distance a, decimal b) => new Distance(a.value * b);
        static public Distance operator/(Distance a, decimal b) => new Distance(a.value / b);


        static public Velocity operator/(Distance d, TimeSI t ) => new Velocity(d.ValueSI / t.ValueSI);
        static public TimeSI operator/(Distance d, Velocity v) => new TimeSI(d.ValueSI / v.ValueSI);

        public override string ToString() {

            // bool Threshold(TimeUnits u, decimal lessThan) => System.Math.Abs(As(u)) < lessThan ;

            if (Math.Abs(value) < 0.1m) return $"{PrintAs(Milimeter)}";
            if (Math.Abs(value) < 1000m) return $"{PrintAs(Meter)}";
            if (Math.Abs(value) < 1000000m) return $"{PrintAs(Kilometer)}";
            if (Math.Abs(As(AU)) < 1) return $"{PrintAs(Kilometer)} | {PrintAs(LightSecond)}";
            if (Math.Abs(As(LightYear)) < 1) return $"{PrintAs(AU)} | {PrintAs(LightSecond)} | {PrintAs(Kilometer)}";
            return PrintAs(LightYear);
        }
    }

    internal class UnitInfo_Dist : UnitInfo<DistanceUnits> {
        public override decimal GetMultiplier(DistanceUnits t) => t switch {
           Milimeter => 0.001m,
           Meter => 1m,
           Kilometer => 1000m,
           LightSecond => Constants.c,
           LightYear => Constants.c * Constants.SECONDS_IN_YEAR,
           AU => Constants.AU,
            _ => throw new NotImplementedException($"unit not impl : {t}")
        };

        public override string GetSuffix(DistanceUnits t) => t switch {
           Milimeter => "mm",
           Meter => "m",
           Kilometer => "km",
           LightSecond => "lsec",
           LightYear => "lyr",
           AU => "AU",
            _ => t.ToString()
        };
    }
}
