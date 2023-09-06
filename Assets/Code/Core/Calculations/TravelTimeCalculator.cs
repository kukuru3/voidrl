using Core.Units;
using UnityEngine;

namespace Core.Calculations {
    public class  TravelTimeCalculator {
        public struct TravelTime {
            public decimal progradeBurnTime;
            public decimal retrogradeBurnTime;
            public decimal coastTime;
            public decimal turnoverV;
            public bool isBrachistochrone;

            public decimal TotalTime => progradeBurnTime + retrogradeBurnTime + (isBrachistochrone ? 0 : coastTime);
        }


        struct AscensionProfileSnapshot {
            public decimal t;
            public decimal v;
            public decimal x;
        }

        const int PRIMARY_ITERATIONS = 2000;
        static AscensionProfileSnapshot[] ascensionProfile = new AscensionProfileSnapshot[PRIMARY_ITERATIONS];

        static public TravelTime CalculateComplexWithRootFinding(Distance distance, Mass dryMass, Mass propellantMass, Velocity vExhaust, CustomSIValue propellantMassFlow, Velocity maxToleratedVelocity = null) {
            var d = distance.ValueSI;
            var F = vExhaust.ValueSI * propellantMassFlow.ValueSI;
            var Ve = vExhaust.ValueSI;
            var m0 = dryMass.ValueSI;

            var tIdeal = propellantMass.ValueSI / propellantMassFlow.ValueSI;
            var Δt = tIdeal / 1000m;

            var flow = propellantMassFlow.ValueSI;
            var mP = propellantMass.ValueSI;

            var halfΔv = Ve * ((m0 + mP) / m0).Ln() * 0.505m;

            var maxV = decimal.MaxValue;
            if (maxToleratedVelocity != null) maxV = maxToleratedVelocity.ValueSI;

            // build initial profile: burn all the way up to deltaV / 2, then immediately burn retrograde

            decimal v = 0,t = 0,x = 0;

            var decel = false;

            var result = new TravelTime();
            var maxIAscent = 0; 

            for (var i = 0; i < PRIMARY_ITERATIONS; i++) {
                var a = F / (m0 + mP);
                a = decel ? -a : a;
                var oldV = v;
                v += 0.5m * a * Δt;
                x += v * Δt;
                v += 0.5m * a * Δt;
                t += Δt;

                mP -= flow * Δt;

                if (!decel) {
                    ascensionProfile[i] = new AscensionProfileSnapshot { t = t, v = v, x = x };
                    maxIAscent = i;
                }

                var ΔvRemainingCurrent = Ve * ((m0 + mP) / m0).Ln();

                if (!decel && ΔvRemainingCurrent < halfΔv - v + oldV) {
                    decel = true;
                    result.turnoverV = v;
                    result.progradeBurnTime = t;
                }

                if (!decel && v > maxV) {
                    decel = true;
                    result.turnoverV = v;
                    result.progradeBurnTime = t;
                }

                if (decel && v <= 0) {
                    result.retrogradeBurnTime = t - result.progradeBurnTime;
                    break;
                }

                if (decel && mP < -10000) {
                    Debug.LogError("Spending fuel we don't have!");
                }
            }

            if (x >= distance.ValueSI) { // we overshot. We need to do a descending binary search.
                result = DoSearch(dryMass.ValueSI + propellantMass.ValueSI, d, F, flow, ascensionProfile,maxIAscent);                
            } else {                 
                var remainingDistance = distance.ValueSI - x;                
                result.isBrachistochrone = false;
                result.coastTime = remainingDistance / result.turnoverV;
            }
            
            return result;
        }

        const int DESCENT_STEPS = 100;
        const decimal MIN_STEP_GRANULARITY = 0m; // 86400; // 1 day seems okay

        static (decimal tRetrograde, decimal distanceRetrograde)  SimulateRetrogradeSteps(decimal initialV, decimal initialM, decimal F, decimal flow) {
            // if this was constant deceleration:
            var Δt = initialV * initialM / F / DESCENT_STEPS;
            if (Δt < MIN_STEP_GRANULARITY) Δt = MIN_STEP_GRANULARITY;

            var m = initialM;
            decimal x = 0, v = initialV, t = 0;

            for (var i = 0; i <= DESCENT_STEPS; i++) {
                var a = -F / m;
                
                v += 0.5m * a * Δt;
                x += v * Δt;
                v += 0.5m * a * Δt;
                t += Δt;
                m -= flow * Δt;

                if (v <= 0) return (t, x);
            }
            throw new System.Exception("wtf u doing here");
        }

        private static TravelTime DoSearch(decimal m0, decimal targetDistance, decimal F, decimal flow, AscensionProfileSnapshot[] ascensionProfile, int maxAscensionProfileIndex) {
            var index = maxAscensionProfileIndex / 2;
            var span  = maxAscensionProfileIndex / 2;

            decimal tr = 0m;
            // execute a binary search using retrograde steps simulation
            while (span > 0) { 
                span/= 2;
                // Debug.Log($"Binary search step: {index}, {span}");

                var m = m0 - flow * ascensionProfile[index].t;
                decimal dr;
                (tr, dr) = SimulateRetrogradeSteps(ascensionProfile[index].v, m, F, flow);
                var dAtIndex = ascensionProfile[index].x + dr;
                if (dAtIndex > targetDistance) 
                    index -= span; 
                else 
                    index += span;
            }

            return new TravelTime {
                isBrachistochrone = true,
                progradeBurnTime = ascensionProfile[index].t,
                turnoverV = ascensionProfile[index].v,
                coastTime = 0,
                retrogradeBurnTime = tr
            };
        }
    }
}