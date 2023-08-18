using static Core.Units.TimeUnits;

namespace Core.Units {
    public enum TimeUnits {
        Miliseconds, 
        Seconds,
        Hours,
        Days,
        Years,
    }

    public class TimeSI : SIValue<TimeUnits> {
        public TimeSI(decimal seconds) : base(seconds) { }
        public TimeSI(decimal value, TimeUnits fromUnit) : this(value * UnitInfoBroker.GetUnitInfo<TimeUnits>().GetMultiplier(fromUnit)) { }

        static public TimeSI operator+ (TimeSI a, TimeSI b) => new TimeSI(a.value + b.value);
        static public TimeSI operator- (TimeSI a, TimeSI b) => new TimeSI(a.value - b.value);

        static public TimeSI operator* (TimeSI a, decimal s) => new TimeSI(a.value * s);
        static public TimeSI operator/ (TimeSI a, decimal s) => new TimeSI(a.value / s);

        public override string ToString() {
            if (As(Seconds)< 1) return PrintAs(Miliseconds);
            if (As(Hours) < 1) return PrintAs(Seconds);
            if (As(Hours) < 24) return PrintAs(Hours);
            if (As(Hours) < 72) return $"{PrintAs(Hours)} | {PrintAs(Days)}";
            if (As(Days)  < 30) return $"{PrintAs(Days)}";
            if (As(Days)  < 1000) return $"{PrintAs(Days)} | {PrintAs(Years)} ";
            return PrintAs(Years);
        }
    }

    internal class UnitInfo_Time : UnitInfo<TimeUnits> {        
        public override decimal GetMultiplier(TimeUnits unit) => unit switch {
            Miliseconds => 0.001m,
            Seconds => 1m,
            Hours => 3600m,
            Days => 86400m,
            Years => 86400m * 365m,
            _ => throw new System.ArgumentException($"Invalid unit value: {unit}")
        };
        public override string GetSuffix(TimeUnits unit) => unit switch { 
            Miliseconds => "ms",
            Seconds => "s",
            Hours => " hours",
            Days => " days",
            Years => " years",
            _ => throw new System.ArgumentException($"Invalid unit value: {unit}")
        };
    }
}
