namespace Core.Units {
    public enum VelocityUnits {
        MetersPerSecond,
        KMH,
        CentiC,
        C,
    }

    public class Velocity : SIValue<VelocityUnits> {
        public Velocity(decimal mps) : base(mps) { }
        public Velocity(decimal value, VelocityUnits fromUnit) : base(value, fromUnit) { }

        static public Velocity operator+(Velocity a, Velocity b) => new Velocity(a.value + b.value);
        static public Velocity operator-(Velocity a, Velocity b) => new Velocity(a.value - b.value);

        static public Velocity operator*(Velocity a, decimal b) => new Velocity(a.value * b);
        static public Velocity operator/(Velocity a, decimal b) => new Velocity(a.value / b);

        static public Distance operator*(TimeSI t, Velocity v ) => new Distance(t.ValueSI * v.ValueSI);
        static public Distance operator*(Velocity v, TimeSI t) => new Distance(t.ValueSI * v.ValueSI);

        public override string ToString() {
            if (As(VelocityUnits.MetersPerSecond) < 3000) return PrintAs(VelocityUnits.MetersPerSecond);
            if (As(VelocityUnits.MetersPerSecond) < 300000) return $"{PrintAs(VelocityUnits.MetersPerSecond)} | {PrintAs(VelocityUnits.CentiC)}";
            return PrintAs(VelocityUnits.CentiC);
        }
    }

     internal class UnitInfo_V : UnitInfo<VelocityUnits> {
        public override decimal GetMultiplier(VelocityUnits t) {
            return t switch {
                VelocityUnits.MetersPerSecond => 1m,
                VelocityUnits.KMH => 3.6m,
                VelocityUnits.CentiC => Constants.c / 100,
                VelocityUnits.C => Constants.c,
            };
        }

        public override string GetSuffix(VelocityUnits t) {
            return t switch { VelocityUnits.MetersPerSecond => "m/s", VelocityUnits.KMH => "km/h", VelocityUnits.C => "c", VelocityUnits.CentiC => "%c", };
        }
    }
}
