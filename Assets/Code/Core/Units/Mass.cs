namespace Core.Units {
    public class Mass : SIValue<MassUnits> {
        public Mass(decimal kg) : base(kg) { }
        public Mass(decimal value, MassUnits units) : base(value, units) { }
        static public Mass operator+ (Mass a, Mass b) => new Mass(a.value + b.value);
        static public Mass operator- (Mass a, Mass b) => new Mass(a.value - b.value);

        static public Mass operator* (Mass a, decimal b) => new Mass(a.value * b);
        static public Mass operator/ (Mass a, decimal b) => new Mass(a.value / b);

        public override string ToString() {
            if (As(MassUnits.g)<1) return PrintAs(MassUnits.mg);
            if (As(MassUnits.kg)<3) return PrintAs(MassUnits.g);
            if (As(MassUnits.t )<3) return PrintAs(MassUnits.kg);
            if (As(MassUnits.kt )<3) return PrintAs(MassUnits.t);
            if (As(MassUnits.Mt )<3) return PrintAs(MassUnits.kt);
            return PrintAs(MassUnits.Mt);
        }
    }
    
    public enum MassUnits {
        mg,
        g,
        kg,
        t,
        kt,
        Mt,
    }

    internal class UnitInfo_Mass : UnitInfo<MassUnits> {
        public override decimal GetMultiplier(MassUnits unit)  => unit switch { 
            MassUnits.mg => 0.000001m, MassUnits.g => 0.001m, MassUnits.kg => 1m, MassUnits.t => 1000m, MassUnits.kt => 1000000m, MassUnits.Mt => 1000000000m, 
            _ => throw new System.InvalidOperationException("unit?")
        };
        public override string GetSuffix(MassUnits u) => u.ToString();
    }
}
