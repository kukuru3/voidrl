using System;

namespace Core {
    public abstract class UnitInfo<TUnit> : IUnitInfo<TUnit> {
        public UnitInfo()
        {
            UnitInfoBroker.Register(this);
        }
        public abstract string GetSuffix(TUnit t);
        public abstract decimal GetMultiplier(TUnit t);
        public decimal GetInverseMultiplier(TUnit t) => 1m / GetMultiplier(t);
    }

    public interface IUnitInfo<TUnit> {
        decimal GetInverseMultiplier(TUnit t);
        decimal GetMultiplier(TUnit t);
        string GetSuffix(TUnit t);
    }

    public abstract class SIValue {
        protected readonly decimal value;
        public SIValue(decimal value) { this.value = value; }
    }

    public class CustomSIValue : SIValue {
        internal readonly string unit;
        public CustomSIValue(decimal value, string unit) : base(value) { this.unit = unit; }
    }

    public abstract class SIValue<TUnit> : SIValue {
        public SIValue(decimal value) : base(value) { }
        public SIValue(decimal value, TUnit fromUnit) : base(value * UnitInfoBroker.GetUnitInfo<TUnit>().GetMultiplier(fromUnit)) { } 

        internal IUnitInfo<TUnit> UnitInfo => UnitInfoBroker.GetUnitInfo<TUnit>();

        public decimal ValueSI => value;
        public decimal As(TUnit unit) => value * UnitInfo.GetInverseMultiplier(unit);
        public string  PrintAs(TUnit unit) => $"{As(unit):F3}{UnitInfo.GetSuffix(unit)}";

        public override string ToString() {
            var absVal = Math.Abs(value);
            TUnit bestUnit = default;
            foreach (var unit in K3.Enums.IterateValues<TUnit>()) {
                var num = absVal * UnitInfo.GetInverseMultiplier(unit);
                if (num > 0.5m) {
                    bestUnit = unit;
                }
            }
            return PrintAs(bestUnit);
        }
    }
}