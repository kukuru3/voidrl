namespace Core.Units {
    public enum AccelUnits {
        MetersPerSecondSquared,
        G,
    }

    public class Accel : SIValue<AccelUnits> {
        public Accel(decimal km_per_s2): base(km_per_s2) { }
        public Accel(decimal value, AccelUnits unit) : base(value, unit) { }

        static public Accel operator+(Accel a, Accel b) => new Accel(a.value + b.value);
        static public Accel operator-(Accel a, Accel b) => new Accel(a.value - b.value);
        static public Accel operator*(Accel a, decimal b) => new Accel(a.value * b);
        static public Accel operator/(Accel a, decimal b) => new Accel(a.value / b);

        static public Velocity operator* (Accel a, TimeSI t) => new Velocity(a.value * t.ValueSI);
        static public Velocity operator* (TimeSI t, Accel a) => new Velocity(a.value * t.ValueSI);

        static public TimeSI operator/(Velocity v, Accel a) => new TimeSI(v.ValueSI / a.ValueSI);
    }

    class UnitInfo_Accel : UnitInfo<AccelUnits> {
        public override decimal GetMultiplier(AccelUnits t) => t switch { 
            AccelUnits.MetersPerSecondSquared => 1m, 
            AccelUnits.G => Constants.g, 
            _ => throw new System.Exception() };

        public override string GetSuffix(AccelUnits t) => t switch { AccelUnits.MetersPerSecondSquared => "m/s²", AccelUnits.G => "g", _ => $"{t}" };
    }    
}
