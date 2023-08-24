using System;
using System.Runtime.InteropServices;
using System.Xml;
using Core;
using Core.Calculations;
using Core.Units;
using UnityEngine;

namespace Core.Calculations {
    public class NumericRootFindingBrachisto {

        static (decimal tFull, decimal Snew, decimal vMax) RootFindingPass(decimal F, decimal m, decimal mFlow, decimal tTurnover) {
            // we only do root finding when full burn trajectory overshoots the distance, so we do not have to track
            // the propellant mass separately. It will never run out.
            var v = 0m;
            var x = 0m;
            var t = 0m;

            var vMax = 0m;

            var i = 0; 


            var Δt = tTurnover / 100; // would be 1000 timesteps for linear acceleration.

            while(true) { // effectively functions as while (true)
                var a = F / m;
                var decel = t > tTurnover;
                if (decel) a *= -1m;
                v += 0.5m * a * Δt;
                x += v * Δt;
                v += 0.5m * a * Δt;
                t += Δt;
                m-= mFlow;

                vMax = Math.Max(vMax, v);

                if (v < 0) {
                    Debug.Log($"Projection for tBurn={new TimeSI(tTurnover)}, iterations: {i}; tFull = {new TimeSI(t)}, distance = {new Distance(x)}");
                    return (t, x, vMax);
                }

                if (++i > 100000) { throw new Exception("???"); }
            }
        }

        static (decimal tProgradeBurn, decimal tRetrogradeBurn, decimal vTurnover) RootFinding(decimal tTurnover, decimal S, decimal distance, decimal F, decimal m, decimal mFlow) {
            var T     = tTurnover;
            var vMax  = 0m;
            var tFull = 0m;

            Debug.Log("RF!");
            
            var delta = S - distance;

            for (var i = 0; i < 10; i++) {
                
                var oldDelta = delta;
                var oldRho = S / distance;
                var oldS = S;
                var oldT = T;
                var oldVMax = vMax;

                T *= (distance / S).Root();

                (tFull, S, vMax) = RootFindingPass( F, m, mFlow, T);

                var rho = S / distance;
                Debug.Log($"rf pass complete for TBurn= {new TimeSI(T)}; <color=#fc6><b>RHO: {oldRho:f3}->{rho:f3}</b></color>;");
            }
            Debug.Log($"RF done");
            return (T, tFull - T, vMax);
        }

        static public BrachistochroneCalculation FindBrachistoViaRootFinding(Distance distance, Mass dryMass, Mass propellantMass, Velocity vExhaust, CustomSIValue propellantMassFlow) {
            // a trick as old as time for those of us who are incompetent of derivatives
            // plug in solutions and see how they relate to a goal.
            
            var d = distance.ValueSI;

            var F = vExhaust.ValueSI * propellantMassFlow.ValueSI;
            var Ve = vExhaust.ValueSI;
            
            var m0 = dryMass.ValueSI;
            var mPropellant = propellantMass.ValueSI;
            var flow = propellantMassFlow.ValueSI;

            var a0 = F / m0;

            var tIdeal = (4m * d / a0).Root();

            var Δt = tIdeal / 1000m;

            var criticalDeltaV = Ve * ((m0 + mPropellant) / m0).Ln() / 2;

            var decelerating = false ;

            decimal v = 0;
            decimal x = 0;
            decimal t = 0;

            decimal criticalV = 0;
            decimal distanceAfterBurn = 0;


            decimal t0 = 0m;
            decimal tDecel = 0m;

            for (var i = 0; i < 10000; i++) {

                // leapfrog:
                // vₑ = v₀ + ½a₀Δt //vₑ is "vee zero point five" basically.
                // x₁ = x₀ + vₑΔt
                // v₁ = vₑ + ½a₁Δt
                var a = F / (m0 + mPropellant);                
                a = decelerating ? -a : a;
                
                v += 0.5m * a * Δt;
                x += v * Δt;
                v += 0.5m * a * Δt;
                t += Δt;

                mPropellant -= flow * Δt;

                var m = m0 + mPropellant;

                if (mPropellant < 0) { 
                    tDecel = t - t0;
                    break; 
                }

                if (!decelerating) { 
                    var deltaVRemaining = Ve * (m / m0).Ln();
                    if (deltaVRemaining <= criticalDeltaV) {
                        criticalV = v;
                        decelerating = true;
                        t0 = t;
                        distanceAfterBurn = x;
                    }
                } else {
                    if (v <= 0) {
                        tDecel = t - t0;
                        break;
                    }
                }
            }

            var cv = new Velocity(criticalV);
            var dd = new Distance(x);
            var tt = new TimeSI(t);


            // Debug.Log($"critical v: {cv}, crossed {dd}, time {tt}");
            

            var remainingDistance = distance.ValueSI - dd.ValueSI;

            if (remainingDistance < 0) { // full burn puts us over
                var result = RootFinding(t0, x, distance.ValueSI, F, m0 + propellantMass.ValueSI, flow);
                return new BrachistochroneCalculation {
                    acceleration = new Accel(a0),
                    burnTimePrograde = new TimeSI(result.tProgradeBurn),
                    burnTimeRetrograde = new TimeSI(result.tRetrogradeBurn),
                    coastTime = new TimeSI(0),
                    distanceAfterBurning = new Distance(-1),
                    propellantExpenditure = new Mass(flow * (result.tProgradeBurn + result.tRetrogradeBurn)),
                    estimatedAccelerationFactor = -1,
                    topV = new Velocity(result.vTurnover),
                };
            }

            var tCoast = new TimeSI(remainingDistance / criticalV);
            // Debug.Log($"accel: {new TimeSI(t0)}, coast: {new TimeSI(tCoast)}, decel: {new TimeSI(tDecel)}, TOTAL: {new TimeSI(t0 + tCoast +tDecel )}");

            var Tacc = new TimeSI(t0);
            var Tdecc = new TimeSI(tDecel);

            return new BrachistochroneCalculation {
                acceleration = new Accel(a0),
                burnTimePrograde =  Tacc,
                burnTimeRetrograde = Tdecc,
                coastTime = tCoast,
                distanceAfterBurning = new Distance(distanceAfterBurn),
                propellantExpenditure = propellantMass - new Mass(mPropellant),
                estimatedAccelerationFactor = -1,
                topV = cv,
            };
            

            // finalize:
          

            // naive brachisto with constant a0 would last:

            // 1000 timesteps for acceleration

            
        }
    }
}
