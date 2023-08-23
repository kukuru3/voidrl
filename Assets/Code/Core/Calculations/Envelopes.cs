using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Units;
using UnityEditor.SceneManagement;

namespace Core.Calculations {

    public static class Extensions {
        static public decimal Root(this decimal item) {
            return (decimal)Math.Sqrt((double)item);
        }

        static public decimal Ln(this decimal item) {
            return (decimal)Math.Log((double)item);
        }

        static public decimal Log10(this decimal item) {
            return (decimal)Math.Log10((double)item);
        }

        public static double LambertW(double x)
        {
            // LambertW is not defined in this section
            if (x < -Math.Exp(-1))
                throw new Exception("The LambertW-function is not defined for " + x + ".");

            // computes the first branch for real values only

            // amount of iterations (empirically found)
            int amountOfIterations = Math.Max(4, (int)Math.Ceiling(Math.Log10(x) / 3));

            // initial guess is based on 0 < ln(a) < 3
            double w = 3 * Math.Log(x + 1) / 4;

            // Halley's method via eqn (5.9) in Corless et al (1996)
            for (int i = 0; i < amountOfIterations; i++)
                w = w - (w * Math.Exp(w) - x) / (Math.Exp(w) * (w + 1) - (w + 2) * (w * Math.Exp(w) - x) / (2 * w + 2));

            return w;
        }
    }

    public struct BrachistochroneCalculation {
        public Velocity topV;
        public TimeSI burnTimePrograde;
        public TimeSI coastTime;
        public TimeSI burnTimeRetrograde;
        public Mass propellantExpenditure;
        public Distance distanceAfterBurning;
        public Accel acceleration;
        public decimal estimatedAccelerationFactor;
    }

    public class Brachistochrone {
        public static BrachistochroneCalculation CalculateBrachistochrone(Mass dryMass, Mass propellantMass, Distance distance, Velocity exhaustV, CustomSIValue propellantFlow, bool allowRefills) {
            if (allowRefills) {
                propellantMass = new Mass(propellantMass.ValueSI / 2);
            }

            var m0 = (dryMass + propellantMass).ValueSI;
            var lnM = (m0 / dryMass.ValueSI).Ln();
            var d = distance.ValueSI;
            var F = exhaustV.ValueSI * propellantFlow.ValueSI;

            var a0 = F / m0;

            // end acceleration is F / drymass, but that's only at the end
            // most of the time is spent near F / wetmass.
            // it will take 2nd order integration to get the real value.
            var aEff = F / m0 * (1m + lnM).Root();

            if (allowRefills) {
                aEff = F / m0;
            }


            var flow = propellantFlow.ValueSI;
            var halfBurnTimeBasedOnFuel = propellantMass.ValueSI / flow / 2;

            var neededBurnTimeHalfForMidTurnover = (d / aEff).Root();

            var tBurnHalf = neededBurnTimeHalfForMidTurnover;

            if (!allowRefills && neededBurnTimeHalfForMidTurnover > halfBurnTimeBasedOnFuel) { // not enough fuel for true brachisto
                tBurnHalf = halfBurnTimeBasedOnFuel;
            }

            var vAtBurnTimeHalf = aEff * tBurnHalf;
            var distanceAtBurnTimeHalf = aEff * tBurnHalf * tBurnHalf / 2;

            var remainingCoastDistance = d - distanceAtBurnTimeHalf * 2;
            var coastTime = 0m;

            if (remainingCoastDistance > 100m) {
                coastTime = remainingCoastDistance / vAtBurnTimeHalf;
            }

            var propellantExpenditure = flow * tBurnHalf * 2;

            var result = new BrachistochroneCalculation {
                burnTimePrograde = new TimeSI(tBurnHalf),
                burnTimeRetrograde = new TimeSI(tBurnHalf),
                coastTime = new TimeSI(coastTime),
                distanceAfterBurning = new Distance(distanceAtBurnTimeHalf),
                propellantExpenditure = new Mass(propellantExpenditure),
                topV = new Velocity(vAtBurnTimeHalf),
                acceleration = new Accel(aEff),
                estimatedAccelerationFactor = aEff / a0,

            };

            return result;
        }

        public static void CalculateBrachistochroneWithPropellantLoss(Mass dryMass, Mass propellantMass, Distance distance, Velocity exhaustV, CustomSIValue propellantFlow) {
            var m0 = (dryMass + propellantMass).ValueSI;
            var lnM = (decimal)Math.Log((double)(m0 / dryMass.ValueSI));
            var d = distance.ValueSI;
            var F = exhaustV.ValueSI * propellantFlow.ValueSI;

            var deltaV0 = exhaustV.ValueSI * lnM;

            // insideW = (d f)/(e M v) - 1/e;
            // d = M/f - (M e^(  W(insideW) + 1) ) /f
            
            var a0 = F / m0;

            var tNaive = (d / a0).Root();

            // in reality, acceleration will increase in the latter stage 

            // first, a numeric method:
            // estimate timestep:
            int NUMERIC_STEPS = 1000;
            var Δt = tNaive / NUMERIC_STEPS;


            var dryMassKg = dryMass.ValueSI;
            var currentMassPropellantKg = propellantMass.ValueSI;
            var flow = propellantFlow.ValueSI;

            var x = 0m;
            var v = 0m;
            
            for (var i = 0; i < NUMERIC_STEPS*5; i++) {

                var m = dryMassKg + currentMassPropellantKg;
                var deltaVRemaining = exhaustV.ValueSI * (m / dryMassKg).Ln();

                var decelerating = deltaVRemaining <= deltaV0 / 2;
                
                var a = F / m;

                if (decelerating) a *= -1m;
              
                // leapfrog:
                // vₑ = v₀ + ½a₀Δt //vₑ is "vee zero point five" basically.
                // x₁ = x₀ + vₑΔt
                // v₁ = vₑ + ½a₁Δt

                v += 0.5m * a * Δt;
                x += v * Δt;
                v += 0.5m * a * Δt;

                currentMassPropellantKg -= flow * Δt;
            }
        }
    }

    public class EnvelopeCalculations {
        /// <param name="initialRelativeVelocity">if greater than zero, the target is APPROACHING us</param>
        public static Envelope GetFastestRendezvous(Velocity initialRelativeVelocity, Accel acceleration, Distance distance) {
            // in this nomenclature, positive speed indicates that the objects are approaching one another.
            var v = initialRelativeVelocity;

            var a = acceleration;
            var d = distance;

            var tStop = v / a;
            var dStop = v * tStop / 2;

            var dSym = d - dStop;

            var solution = SolveQuadratic(a.ValueSI, 2 * v.ValueSI, -dSym.ValueSI);

            var t1 = LeastPositive(solution);

            // accel t1, decel t1 + tStop

            if (!t1.HasValue) throw new System.InvalidOperationException($"Imaginary t1");
            var tt1 = new TimeSI(t1.Value);
            var e = new Envelope();
            if (tt1.ValueSI > 0) e.AddStage( new Stage { name = "Burn", duration =tt1, acceleration = a });
            if ((tt1 + tStop).ValueSI > 0) e.AddStage(new Stage { name = "Break", duration = tt1 + tStop, acceleration = a * -1m});

            return e;
            
            //var t2 = velocity.ValueSI * velocity.ValueSI / acceleration.ValueSI / 2;

            //if (initialRelativeVelocity.ValueSI < 0) {
            //    velocity = new Velocity(Math.Abs(velocity.ValueSI));
            //    // special consideration.
            //    var t1 = acceleration.ValueSI * distance.ValueSI + velocity.ValueSI * velocity.ValueSI / 2;
            //    t1 = t1.Root() / acceleration.ValueSI;

            //    var result = new Envelope() { stages = new Stage[2] };
            //    result.stages[0] = new Stage { name = "Burn", duration = new TimeSI(t1 + t2), acceleration = acceleration };
            //    result.stages[1] = new Stage { name = "Deel", duration = new TimeSI(t2), acceleration = acceleration * -1 };
            //    return result;
            //} else {
            //    var c = velocity * (velocity / acceleration) / 2 - distance;
            //    var t1 = LeastPositive(SolveQuadratic(acceleration.ValueSI, 2 * initialRelativeVelocity.ValueSI, c.ValueSI ));
            //    if (t1.HasValue) {
            //        var result = new Envelope() { stages = new Stage[2] };
            //        result.stages[0] = new Stage { name = "Burn", duration = new TimeSI(t1.Value), acceleration = acceleration };
            //        result.stages[1] = new Stage { name = "Decel", duration = new TimeSI(t1.Value + t2), acceleration = acceleration * -1 };
            //        return result;
            //    }
            //    throw new System.InvalidOperationException("Can't calculate");                
            //}
        }

        public static (decimal x1, decimal x2) SolveQuadratic(decimal a, decimal b, decimal c) {
            var term = (b * b -  4 * a * c);
            term = term.Root();

            if (term < 0) throw new System.InvalidOperationException("Imaginary root");
            
            var value1 = (-b + term) / (2 * a);
            var value2 = (-b - term) / (2 * a);
            
            return (value1, value2);
        }
        public static decimal? LeastPositive((decimal a, decimal b) tuple) {
            return LeastPositive(new List<decimal>() { tuple.a, tuple.b } );
        }

        public static decimal? LeastPositive(IEnumerable<decimal> values) {
            decimal? bestSoFar = default;
            foreach (var v in values) {
                if (v < 0) continue;
                if (!bestSoFar.HasValue || v < bestSoFar) bestSoFar = v;
            }
            return bestSoFar;
        }


    }

    static public class Tester {
        [UnityEditor.MenuItem("Void/SPACE MATH : test envelopes...")]
        static public void Foo() {
            var v = new Velocity(1, VelocityUnits.MetersPerSecond);
            // EnvelopeCalculations.GetFastestRendezvous( new Velocity(0), new Accel(1), new Distance(100));
            // EnvelopeCalculations.GetFastestRendezvous(new Velocity(200m.Root()), new Accel(1), new Distance(100));
            PrintTest(new Velocity(0), new Accel(1), new Distance(100));
            // PrintTest(new Velocity(-5), new Accel(1), new Distance(100));

            PrintTest(new Velocity(10), new Accel(1), new Distance(100));
            PrintTest(new Velocity(200m.Root()), new Accel(1), new Distance(100));
            //PrintTest(new Velocity(30), new Accel(1), new Distance(100));
        }

        private static void PrintTest(Velocity velocity, Accel accel, Distance distance) {
            var envelope = EnvelopeCalculations.GetFastestRendezvous(velocity, accel, distance);
            var v = velocity;
            var s = new Distance(0);
            var t = new TimeSI(0);

            Debug.Log($"Start conditions: t = {t}, v = {v} and s = {s}");

            foreach (var stage in envelope.Stages) {
                t += stage.duration;
                s += v * stage.duration + stage.acceleration * stage.duration * stage.duration / 2;
                v += stage.duration * stage.acceleration;
                Debug.Log($"At the end of stage `{stage.name}`, t = {t}, v = {v} and s = {s}");
            }
        }
    }

    public struct Stage {
        public string name;
        public Accel acceleration;
        public TimeSI duration;
    }

    public struct Envelope {
        public Envelope(params Stage[] initialStages)
        {
            stages = new List<Stage>();
            foreach(var s in initialStages) AddStage(s);
        }

        public void AddStage(Stage s) { 
            if (stages == null) stages = new List<Stage>();
            stages.Add(s);
        }
        public IReadOnlyList<Stage> Stages => stages;

        List<Stage> stages;

        public TimeSI TotalTime { get {
            var t = new TimeSI(0);
            foreach (var stage in stages) {
                t += stage.duration;
            }
            return t;
        } }

        public static void Simulate(Velocity initialV, Envelope envelope) {
            var v = initialV;
            var s = new Distance(0);
            var t = new TimeSI(0);
            foreach (var stage in envelope.stages) {
                t += stage.duration;
                s += v * stage.duration + stage.acceleration * stage.duration * stage.duration / 2;
                v += stage.duration * stage.acceleration / 2;
                Debug.Log($"At the end of stage `{stage.name}`, t = {t}, v = {v} and s = {s}");
            }
        }
    }

}
